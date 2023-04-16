using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RTProject : MonoBehaviour
{
    public ComputeShader shader;
    public Material material;
    public GameObject objectToProject;
    public Camera cam;

    RenderTexture particlesTexture;

    private void Start()
    {
        OnRenderEvent.OnRenderImageEvent += OnRenderCam;
    }

    private void OnRenderCam(RenderTexture source, RenderTexture destination)
    {
        if (particlesTexture == null)
        {
            particlesTexture = new RenderTexture(Screen.width, Screen.height, 32, RenderTextureFormat.ARGB32);
            particlesTexture.enableRandomWrite = true;
            particlesTexture.Create();
        }

        shader.SetTexture(0, "Result", particlesTexture);
        shader.SetInt("objectPosX", (int)objectToProject.transform.position.x);
        shader.SetInt("objectPosY", (int)objectToProject.transform.position.y);
        shader.SetInt("objectPosZ", (int)objectToProject.transform.position.z);
        shader.SetVector("camSize", new Vector2(cam.pixelWidth, cam.pixelHeight));
        shader.SetMatrix("projectionMatrix", cam.projectionMatrix * cam.worldToCameraMatrix);
        int workgroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int workgroupsY = Mathf.CeilToInt(Screen.height / 8.0f);

        shader.Dispatch(0, workgroupsX, workgroupsY, 1);

        // Blit the material to the destination texture
        Graphics.Blit(particlesTexture, destination, material);
    }  
}
