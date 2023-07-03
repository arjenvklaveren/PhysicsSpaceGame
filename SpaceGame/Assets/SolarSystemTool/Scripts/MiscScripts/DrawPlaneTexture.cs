using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawPlaneTexture : MonoBehaviour
{
    GameObject plane;
    RenderTexture ringRender;
    RenderTexture ringStripRT;

    BodyRings ring;

    Vector2Int resolution = new Vector2Int(2000,2000);

    ComputeShader shader;

    public void SetTexture(Texture2D ringTex, BodyRings ring)
    {
        //set data and scales
        shader = (ComputeShader)Instantiate(Resources.Load<ComputeShader>("Shaders/RingRender"));
        plane = this.gameObject;
        float ringScale = 0.1f + (0.1f * (ring.ringWidth + ring.ringOffset));
        plane.transform.localScale = new Vector3(ringScale, ringScale, ringScale);

        //create textures for shader and material use
        if (ringRender == null)
        {
            ringRender = new RenderTexture(resolution.x, resolution.y, 32, RenderTextureFormat.ARGB32);
            ringRender.enableRandomWrite = true;
            ringRender.Create();
        }
        if (ringStripRT == null)
        {
            ringStripRT = new RenderTexture(resolution.x, resolution.y, 32, RenderTextureFormat.ARGB32);
            ringStripRT.enableRandomWrite = true;
            ringStripRT.Create();
        }
        RenderTexture activeTemp = RenderTexture.active;
        Graphics.Blit(ringTex, ringStripRT);
        RenderTexture.active = activeTemp;


        //set shader variables
        shader.SetTexture(0, "Result", ringRender);
        shader.SetTexture(0, "RingStrip", ringStripRT);
        shader.SetVector("resolution", (Vector2)resolution);
        shader.SetFloat("planetRadius", ring.transform.lossyScale.x / 2);
        shader.SetFloat("ringWidth", ring.ringWidth);
        shader.SetFloat("ringOffset", ring.ringOffset);

        //dispatch shader
        shader.Dispatch(0, resolution.x / 8, resolution.y / 8, 1);

        //create new temporary material, set the texture to new rendertexture result and replace ring material

        Renderer planeRenderer = plane.GetComponentInChildren<Renderer>();
        Material tempMaterial = new Material(Shader.Find("UI/Unlit/Transparent"));
        tempMaterial.mainTexture = ringRender;
        planeRenderer.sharedMaterial = tempMaterial;
    }
}
