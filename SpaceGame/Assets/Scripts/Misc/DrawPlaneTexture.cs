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

    public void SetTexture(Texture2D ringTex)
    {
        shader = (ComputeShader)Instantiate(Resources.Load<ComputeShader>("Shaders/Compute/RingRender"));
        plane = this.gameObject;
        ring = transform.GetComponentInParent<BodyRings>();
        float ringScale = 0.1f + (0.1f * (ring.ringWidth + ring.ringOffset));
        plane.transform.localScale = new Vector3(ringScale, ringScale, ringScale);

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
        Graphics.Blit(ringTex, ringStripRT);

        shader.SetTexture(0, "Result", ringRender);
        shader.SetTexture(0, "RingStrip", ringStripRT);
        shader.SetVector("resolution", (Vector2)resolution);
        shader.SetFloat("planetRadius", ring.transform.lossyScale.x / 2);
        shader.SetFloat("ringWidth", ring.ringWidth);
        shader.SetFloat("ringOffset", ring.ringOffset);

        if(resolution.x > 80 && resolution.y > 80) shader.Dispatch(0, resolution.x / 8, resolution.y / 8, 1);

        plane.GetComponentInChildren<Renderer>().material.mainTexture = ringRender;
    }
}
