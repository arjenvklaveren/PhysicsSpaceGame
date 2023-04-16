using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRings : MonoBehaviour
{
    Particle[] particles;
    public PlanetRing[] rings;

    Ring[] ringsS;

    public ComputeShader shader;
    public Material particleMaterial;
    public Camera cam;

    private ComputeBuffer particlesBuffer;
    private ComputeBuffer ringsBuffer;

    float timeStep = 0.02f;

    public GameObject planet;

    private int particleCount;
    public float ringStartDistance;

    float planetMass;

    int kernelID;
    uint threadGroupSize;
    int threadGroups;

    RenderTexture particlesTexture;

    bool firstLoop = true;

    struct Particle
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 color;
    }

    public struct Ring
    {
        public Vector4 color;
        public int density;
        public int width;
    }

    void Start()
    {
        planetMass = planet.transform.localScale.x * 50;
        kernelID = shader.FindKernel("CalcRings");

        ringsS = new Ring[rings.Length];
        for(int i = 0; i < rings.Length; i++)
        {
            particleCount += rings[i].density;

            ringsS[i].color = rings[i].ringColor;
            ringsS[i].density = rings[i].density;
            ringsS[i].width = rings[i].width;
        }

        shader.SetBool("firstLoop", true);
        shader.SetFloat("particleCount", particleCount);
        shader.SetFloat("timeStep", timeStep);
        shader.SetFloat("planetMass", planetMass);
        shader.SetFloat("planetRadius", planet.transform.lossyScale.x / 2);
        shader.SetInt("ringCount", rings.Length);
        shader.SetFloat("startRingDistance", ringStartDistance);

        particles = new Particle[particleCount];
        particlesBuffer = new ComputeBuffer(particleCount, sizeof(float) * 9);
        shader.SetBuffer(kernelID, "particles", particlesBuffer);
        particlesBuffer.SetData(particles);

        ringsBuffer = new ComputeBuffer(rings.Length + 1, sizeof(float) * 6);
        shader.SetBuffer(kernelID, "rings", ringsBuffer);
        ringsBuffer.SetData(ringsS);

        shader.GetKernelThreadGroupSizes(kernelID, out threadGroupSize, out _, out _);
        threadGroups = (int)((particleCount + (threadGroupSize - 1)) / threadGroupSize);

        if (particlesTexture == null)
        {
            particlesTexture = new RenderTexture(Screen.width, Screen.height, 32, RenderTextureFormat.ARGB32);
            particlesTexture.enableRandomWrite = true;
            particlesTexture.Create();
        }    

        shader.SetTexture(kernelID, "particlesTexture", particlesTexture);
        shader.SetTexture(1, "particlesTexture", particlesTexture);
        OnRenderEvent.OnRenderImageEvent += OnRenderCam;
    }

    void FixedUpdate()
    {
        SimulateRing();
    }

    void OnRenderCam(RenderTexture src, RenderTexture dest)
    {      
        Graphics.Blit(particlesTexture, dest, particleMaterial);
    }

    void SimulateRing()
    {
        shader.SetVector("planetPos", planet.transform.position);       
        shader.SetVector("camSize", new Vector2(cam.pixelWidth, cam.pixelHeight));
        shader.SetFloat("time", Time.time + Random.Range(0.0f, 100.0f));
        shader.SetMatrix("projectionMatrix", cam.projectionMatrix * cam.worldToCameraMatrix);           
        shader.Dispatch(1, cam.pixelWidth / 8, cam.pixelHeight / 8, 1);
        shader.Dispatch(kernelID, threadGroups, 1, 1);


        if (firstLoop)
        {
            shader.SetBool("firstLoop", false);
            firstLoop = false;
        }
    }

    private void OnDestroy()
    {
        particlesBuffer.Dispose();
    }
}
