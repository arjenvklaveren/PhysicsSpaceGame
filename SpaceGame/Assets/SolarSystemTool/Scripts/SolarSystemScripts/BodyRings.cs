using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BodyRings : MonoBehaviour
{
    [RangeEx(0.25f, 4f, 0.25f), SerializeField] public float ringWidth = 1f;
    [RangeEx(0f, 2f, 0.25f), SerializeField] public float ringOffset = 0.25f;
    [Range(-180, 180), SerializeField] public int tilt = 0;

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
   
    bool firstLoop = true;

    Texture depthTex;

    RingTexture ringTex;

    Camera cam;

    float renderValue;

    DrawPlaneTexture planeRingTex;

    CelestialBodyManager manager;
    CelestialBody body;

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
        public Vector3 velocity;
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
        OnEditRing();
    }

    public void OnEditRing()
    {
        if (ringTex == null) ringTex = GetComponentsInChildren<RingTexture>()[0];
        if (ringTex.GetRawTex() == null) ringTex.ResetTex();
        if (ringTex) SetPlaneTexture();
        transform.eulerAngles = new Vector3(tilt, 0, 0);
    }

    void Start()
    {
        ringShader = (ComputeShader)Instantiate(Resources.Load<ComputeShader>("Shaders/RingsCompute"));
        particleMaterial = Resources.Load<Material>("Materials/Alpha");

        manager = GetComponentInParent<CelestialBodyManager>();
        body = GetComponentInParent<CelestialBody>();

        planeRingTex = GetComponentInChildren<DrawPlaneTexture>();
        planeRingTex.gameObject.SetActive(false);

        cam = Camera.main;

        planetCount = manager.GetBodies().Count;
        parentPlanetID = manager.GetBodies().IndexOf(body);
        planets = new Planet[planetCount];

        //set ring density based on conditions
        float scaleLerp = Mathf.InverseLerp(10, 1000, body.transform.lossyScale.x);
        float startDensity = Mathf.Lerp(100000, 1000000, scaleLerp);
        ringDensity = (int)(startDensity * (ringWidth + ringOffset));

        ringDensity = 20000000;


        SetArrayData();
        SetStartVariables();
        SetBuffers();

        if (particlesTexture == null)
        {
            particlesTexture = new RenderTexture(1920, 1080, 32, RenderTextureFormat.ARGB32);
            particlesTexture.enableRandomWrite = true;
            particlesTexture.Create();
        }

        ringShader.SetTexture(0, "particlesTexture", particlesTexture);
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
            planets[i].position = manager.GetBodies()[i].transform.position;
            planets[i].radius = manager.GetBodies()[i].transform.lossyScale.x / 2;
            planets[i].mass = manager.GetBodies()[i].mass;
        }
    }

    void SetStartVariables()
    {
        float rand = Random.Range(0, 1000000) * System.DateTime.Now.Millisecond / 100000;
        ringShader.SetBool("firstLoop", true);
        ringShader.SetFloat("particleCount", ringDensity);
        ringShader.SetFloat("ringWidth", body.transform.lossyScale.x * (1 / Universe.scaleMultiplier) * ringWidth);
        ringShader.SetFloat("timeStep", Universe.timeStep);
        ringShader.SetFloat("randOffset", rand);
        ringShader.SetFloat("gravConstant", Universe.G);
        ringShader.SetFloat("camFarClipPlane", cam.farClipPlane);
        ringShader.SetFloat("camNearClipPlane", cam.nearClipPlane);
        ringShader.SetFloat("startRingDistance", ringOffset);
        ringShader.SetInt("parentPlanetID", parentPlanetID);
        ringShader.SetFloat("parentPlanetPitch", manager.GetBodies()[parentPlanetID].transform.eulerAngles.x);
        ringShader.SetInt("planetCount", planetCount);
        ringShader.SetTexture(0, "ringStripTexture", ringTex.GetRingTextureFromData());
    }

    void SetBuffers()
    {
        particles = new Particle[ringDensity];
        particlesBuffer = new ComputeBuffer(ringDensity, sizeof(float) * 13);
        ringShader.SetBuffer(0, "particles", particlesBuffer);
        particlesBuffer.SetData(particles);


        planetBuffer = new ComputeBuffer(manager.GetBodies().Count, sizeof(float) * 8);
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
            planets[i].position = manager.GetBodies()[i].transform.position;
            planets[i].velocity = manager.GetBodies()[i].GetVelocity();
        }

        planetBuffer.SetData(planets);
        
        ringShader.SetVector("camSize", new Vector2(cam.pixelWidth, cam.pixelHeight));
        ringShader.SetFloat("time", Time.time + Random.Range(0.0f, 100.0f));
        ringShader.SetFloat("renderValue", renderValue);
        ringShader.SetMatrix("projectionMatrix", cam.projectionMatrix * cam.worldToCameraMatrix);

        //Todo maybe not update
        ringShader.SetTexture(0, "camDepthTexture", depthTex);

        ringShader.Dispatch(1, cam.pixelWidth / 8, cam.pixelHeight / 8, 1);
        ringShader.Dispatch(0, (int)((ringDensity + (512 - 1)) / 512), 1, 1);

        if (firstLoop) { ringShader.SetBool("firstLoop", false); firstLoop = false; }
    }

    public RingTexture GetRingTex() { return ringTex; }

    public void SetPlaneTexture()
    {
        if (transform.gameObject.activeSelf == true)
        {
            planeRingTex = GetComponentInChildren<DrawPlaneTexture>();
            planeRingTex.SetTexture(ringTex.GetRingTextureFromData(), this);
        }
    }

    private void OnDestroy()
    {
        particlesBuffer.Dispose();
        planetBuffer.Dispose();
    }
}
