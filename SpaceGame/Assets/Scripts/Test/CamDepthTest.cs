using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamDepthTest : MonoBehaviour
{
    public Camera cam;
    public ComputeShader shader;
    public Material material;
    public Texture test;
    public bool render;

    RenderTexture renderTexture;

    ComputeBuffer depthBuffer;
    float[] depthValue;

    void Start()
    {
        OnRenderEvent.OnRenderImageEvent += OnRenderCam;
        cam.depthTextureMode = DepthTextureMode.Depth;

        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }

        depthValue = new float[1];
        depthBuffer = new ComputeBuffer(1, sizeof(float));
        shader.SetTexture(0, "_OutputTexture", renderTexture);       
    }

    void OnRenderCam(RenderTexture src, RenderTexture dest)
    {
        if(render) Graphics.Blit(renderTexture, dest, material);
        shader.SetTexture(0, "_DepthTexture", Shader.GetGlobalTexture("_CameraDepthTexture"));
        shader.SetFloat("camFarPlane", cam.farClipPlane);
        shader.SetFloat("camNearPlane", cam.nearClipPlane);
        shader.SetMatrix("projMatrix", cam.nonJitteredProjectionMatrix);
        shader.SetMatrix("viewMatrix", cam.worldToCameraMatrix);
        shader.SetBuffer(0, "sampleDepth", depthBuffer);
        depthBuffer.SetData(depthValue);
        shader.Dispatch(0, cam.pixelWidth / 8, cam.pixelHeight / 8, 1);
        depthBuffer.GetData(depthValue);

        Debug.Log(depthValue[0]);
    }
}
