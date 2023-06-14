using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(CelestialBody))]

public class BodyRings : MonoBehaviour
{
    [RangeEx(0.25f, 4f, 0.25f), SerializeField] public float ringWidth = 1f;
    [RangeEx(0f, 2f, 0.25f), SerializeField] public float ringOffset = 0.25f;

    int ringDensity;

    Particle[] particles;
    Planet[] planets;

    int parentPlanetID;

    ComputeShader ringShader;
    Material particleMaterial;
    RenderTexture particlesTexture;

    private ComputeBuffer particlesBuffer;
    private ComputeBuffer planetBuffer;

    private int planetCount;

    int kernelID;
    uint threadGroupSize;
    int threadGroups;
   
    bool firstLoop = true;

    Texture depthTex;
    Texture2D ringTex;

    [HideInInspector] public List<Color32> ringTexData;

    Camera cam;

    float renderValue;

    DrawPlaneTexture planeRingTex;

    public bool OpenWindowButton;
    private bool prev;

    struct Particle
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector4 color;
        public OrbitData orbitData;
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

    private void OnValidate()
    {
        ringTex = Resources.Load<Texture2D>("Images/RingStrip");
        if (OpenWindowButton != prev)
        {
            prev = OpenWindowButton;
            TextureDrawWindow.Open(this);
        }
        if(ringTexData.Count == 0)
        {
            SetRingTextureData(ringTex);          
        }
        SetPlaneTexture();
    }

    void Start()
    {
        ringShader = (ComputeShader)Instantiate(Resources.Load<ComputeShader>("Shaders/Compute/RingsCompute"));
        particleMaterial = Resources.Load<Material>("Materials/Alpha");
        planeRingTex = GetComponentInChildren<DrawPlaneTexture>();
        planeRingTex.gameObject.SetActive(false);

        kernelID = ringShader.FindKernel("CalcRings");
        cam = Camera.main;

        planetCount = CelestialBodyManager.bodies.Count;
        parentPlanetID = CelestialBodyManager.bodies.IndexOf(GetComponent<CelestialBody>());
        planets = new Planet[planetCount];


        float scaleLerp = Mathf.InverseLerp(10, 1000, transform.lossyScale.x);
        float startDensity = Mathf.Lerp(100000, 1000000, scaleLerp);
        ringDensity = (int)(startDensity * (ringWidth + ringOffset));


        SetArrayData();
        SetStartVariables();
        SetBuffers();

        ringShader.GetKernelThreadGroupSizes(kernelID, out threadGroupSize, out _, out _);
        threadGroups = (int)((ringDensity + (threadGroupSize - 1)) / threadGroupSize);

        if (particlesTexture == null)
        {
            particlesTexture = new RenderTexture(1920, 1080, 32, RenderTextureFormat.ARGB32);
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
        ringShader.SetFloat("particleCount", ringDensity);
        ringShader.SetFloat("ringWidth", transform.lossyScale.x * (1 / Universe.scaleMultiplier) * ringWidth);
        ringShader.SetFloat("timeStep", Universe.timeStep);
        ringShader.SetFloat("randOffset", rand);
        ringShader.SetFloat("gravConstant", Universe.G);
        ringShader.SetFloat("camFarClipPlane", cam.farClipPlane);
        ringShader.SetFloat("camNearClipPlane", cam.nearClipPlane);
        ringShader.SetFloat("startRingDistance", ringOffset);
        ringShader.SetInt("parentPlanetID", parentPlanetID);
        ringShader.SetFloat("parentPlanetPitch", CelestialBodyManager.bodies[parentPlanetID].transform.eulerAngles.x);
        ringShader.SetInt("planetCount", planetCount);
        ringShader.SetTexture(0, "ringStripTexture", GetRingTextureFromData());
    }

    void SetBuffers()
    {
        particles = new Particle[ringDensity];
        particlesBuffer = new ComputeBuffer(ringDensity, sizeof(float) * 13);
        ringShader.SetBuffer(kernelID, "particles", particlesBuffer);
        particlesBuffer.SetData(particles);

        planetBuffer = new ComputeBuffer(CelestialBodyManager.bodies.Count, sizeof(float) * 5);
        ringShader.SetBuffer(0, "planets", planetBuffer);
    }

    void OnRenderCam(RenderTexture src, RenderTexture dest)
    {
        //Graphics.Blit(testTex, particlesTexture);
        Graphics.Blit(particlesTexture, dest, particleMaterial);
        depthTex = Shader.GetGlobalTexture("_CameraDepthTexture");
    }

    void SimulateRing()
    {
        for(int i = 0; i < planetCount; i++)
        {
            planets[i].position = CelestialBodyManager.bodies[i].transform.position;
        }

        planetBuffer.SetData(planets);
        
        ringShader.SetVector("camSize", new Vector2(cam.pixelWidth, cam.pixelHeight));
        ringShader.SetFloat("time", Time.time + Random.Range(0.0f, 100.0f));
        ringShader.SetFloat("renderValue", renderValue);
        ringShader.SetMatrix("projectionMatrix", cam.projectionMatrix * cam.worldToCameraMatrix);

        //Todo maybe not update
        ringShader.SetTexture(0, "camDepthTexture", depthTex);

        ringShader.Dispatch(1, cam.pixelWidth / 8, cam.pixelHeight / 8, 1);
        ringShader.Dispatch(kernelID, threadGroups, 1, 1);

        if (firstLoop)
        {
            ringShader.SetBool("firstLoop", false);
            firstLoop = false;
        }
    }

    public void SetRingTextureData(Texture2D texture)
    {
        ringTexData.Clear();
        List<Color32> temp = new List<Color32>();
        for(int i = 0; i < texture.width; i++)
        {
            temp.Add(texture.GetPixel(i, 1));
        }
        SetRingTextureData(temp);
    }
    public void SetRingTextureData(List<Color32> data)
    {
        ringTexData = data;
        SetPlaneTexture();       
    }
    public void ResetRingTextureData()
    {
        SetRingTextureData(ringTex);
    }

    public Texture2D GetRingTextureFromData()
    {
        Texture2D texture = new Texture2D(1000, 1);
        for(int i = 0; i < ringTexData.Count; i++)
        {
            texture.SetPixel(i, 1, ringTexData[i]);
        }
        texture.Apply();
        return texture;
    }

    public List<Color32> GetRingTextureData()
    {
        return ringTexData;
    }

    void SetPlaneTexture()
    {
        if (transform.gameObject.activeSelf == true)
        {
            planeRingTex = GetComponentInChildren<DrawPlaneTexture>();
            planeRingTex.SetTexture(GetRingTextureFromData());
        }
    }

    private void OnDestroy()
    {
        particlesBuffer.Dispose();
        planetBuffer.Dispose();
    }
}
