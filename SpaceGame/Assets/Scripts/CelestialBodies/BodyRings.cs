using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(CelestialBody))]

public class BodyRings : MonoBehaviour
{
    public PlanetRing[] rings;

    Particle[] particles;
    Ring[] ringsS;
    Planet[] planets;

    int parentPlanetID;

    ComputeShader ringShader;
    Material particleMaterial;
    RenderTexture particlesTexture;

    private ComputeBuffer particlesBuffer;
    private ComputeBuffer ringsBuffer;
    private ComputeBuffer planetBuffer;
    private ComputeBuffer fakeOrbitBuffer;

    private int particleCount;
    private int planetCount;
    public float ringStartOffset;

    int kernelID;
    uint threadGroupSize;
    int threadGroups;
   
    bool firstLoop = true;

    Texture depthTex;
    public Texture ringStrip;
    Camera cam;

    float rotateValue;

    struct Particle
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector4 color;
        public OrbitData orbitData;
    }
    public struct Ring
    {
        public Vector4 color;
        public int density;
        public int width;
    }
    public struct Planet
    {
        public Vector3 position;
        public float mass;
        public float radius;
    }
    public struct OrbitData
    {
        public float fakeActive;
        public float distance;
        public float angle;
    }

    void Start()
    {
        if (rings.Length < 1) return;

        ringShader = (ComputeShader)Instantiate(Resources.Load<ComputeShader>("Shaders/Compute/RingsCompute"));
        particleMaterial = Resources.Load<Material>("Materials/Alpha");

        kernelID = ringShader.FindKernel("CalcRings");
        cam = Camera.main;

        planetCount = CelestialBodyManager.bodies.Count;
        parentPlanetID = CelestialBodyManager.bodies.IndexOf(GetComponent<CelestialBody>());
        planets = new Planet[planetCount];

        SetArrayData();
        SetStartVariables();
        SetBuffers();

        ringShader.GetKernelThreadGroupSizes(kernelID, out threadGroupSize, out _, out _);
        threadGroups = (int)((particleCount + (threadGroupSize - 1)) / threadGroupSize);

        if (particlesTexture == null)
        {
            particlesTexture = new RenderTexture(Screen.width, Screen.height, 32, RenderTextureFormat.ARGB32);
            particlesTexture.enableRandomWrite = true;
            particlesTexture.Create();
        }

        ringShader.SetTexture(kernelID, "particlesTexture", particlesTexture);
        ringShader.SetTexture(1, "particlesTexture", particlesTexture);

        cam.depthTextureMode = DepthTextureMode.Depth;
        OnRenderEvent.OnRenderImageEvent += OnRenderCam;
    }

    void FixedUpdate()
    {
        if (depthTex != null)
        {
            SimulateRing();
        }
    }

    void SetArrayData()
    {
        ringsS = new Ring[rings.Length];
        for (int i = 0; i < rings.Length; i++)
        {
            particleCount += rings[i].density;

            ringsS[i].color = rings[i].ringColor;
            ringsS[i].density = rings[i].density;
            ringsS[i].width = rings[i].width;
        }
        for (int i = 0; i < planetCount; i++)
        {
            planets[i].position = CelestialBodyManager.bodies[i].transform.position;
            planets[i].radius = CelestialBodyManager.bodies[i].transform.lossyScale.x / 2;
            planets[i].mass = CelestialBodyManager.bodies[i].mass;
        }
    }

    void SetStartVariables()
    {
        float rand = Random.Range(0, 1000000) * System.DateTime.Now.Millisecond / 100000;
        ringShader.SetBool("firstLoop", true);
        ringShader.SetFloat("particleCount", particleCount);
        ringShader.SetFloat("timeStep", Universe.timeStep);
        ringShader.SetFloat("randOffset", rand);
        ringShader.SetFloat("gravConstant", Universe.G);
        ringShader.SetInt("ringCount", rings.Length);
        ringShader.SetFloat("camFarClipPlane", cam.farClipPlane);
        ringShader.SetFloat("camNearClipPlane", cam.nearClipPlane);
        ringShader.SetFloat("startRingDistance", ringStartOffset);
        ringShader.SetInt("parentPlanetID", parentPlanetID);
        ringShader.SetFloat("parentPlanetPitch", CelestialBodyManager.bodies[parentPlanetID].transform.eulerAngles.x);
        ringShader.SetInt("planetCount", planetCount);
        ringShader.SetTexture(0, "ringStripTexture", ringStrip);
    }

    void SetBuffers()
    {
        particles = new Particle[particleCount];
        particlesBuffer = new ComputeBuffer(particleCount, sizeof(float) * 13);
        ringShader.SetBuffer(kernelID, "particles", particlesBuffer);
        particlesBuffer.SetData(particles);

        ringsBuffer = new ComputeBuffer(rings.Length + 1, sizeof(float) * 6);
        ringShader.SetBuffer(kernelID, "rings", ringsBuffer);
        ringsBuffer.SetData(ringsS);
      
        planetBuffer = new ComputeBuffer(CelestialBodyManager.bodies.Count, sizeof(float) * 5);
        ringShader.SetBuffer(0, "planets", planetBuffer);
    }

    void OnRenderCam(RenderTexture src, RenderTexture dest)
    {      
        Graphics.Blit(particlesTexture, dest, particleMaterial);
        depthTex = Shader.GetGlobalTexture("_CameraDepthTexture");
    }

    void SimulateRing()
    {
        for(int i = 0; i < planetCount; i++)
        {
            planets[i].position = CelestialBodyManager.bodies[i].transform.position;
        }

        rotateValue++;

        planetBuffer.SetData(planets);
        
        ringShader.SetVector("camSize", new Vector2(cam.pixelWidth, cam.pixelHeight));
        ringShader.SetFloat("time", Time.time + Random.Range(0.0f, 100.0f));
        ringShader.SetFloat("rotateDegrees", rotateValue);
        ringShader.SetMatrix("projectionMatrix", cam.projectionMatrix * cam.worldToCameraMatrix);           
        ringShader.SetTexture(0, "camDepthTexture", depthTex);          

        ringShader.Dispatch(1, cam.pixelWidth / 8, cam.pixelHeight / 8, 1);
        ringShader.Dispatch(kernelID, threadGroups, 1, 1);

        if (firstLoop)
        {
            ringShader.SetBool("firstLoop", false);
            firstLoop = false;
        }
    }

    private void OnDestroy()
    {
        particlesBuffer.Dispose();
        ringsBuffer.Dispose();
        planetBuffer.Dispose();
    }
}
