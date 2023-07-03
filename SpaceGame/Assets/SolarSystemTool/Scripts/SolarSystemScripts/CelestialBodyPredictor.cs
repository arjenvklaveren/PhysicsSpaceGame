using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class CelestialBodyPredictor : MonoBehaviour
{
    [SerializeField, Range(5000, 25000)] private int timeSteps;
    [SerializeField, Range(1,250)] private int lineDetail;
    [SerializeField] private CelestialBody relativeToBody;
    bool predict = true;

    CelestialBodyManager manager;
    List<VirtualBody> bodyClones = new List<VirtualBody>();

    private void Update()
    {
        if (Application.isPlaying) SetActive(false);
        if (manager == null) manager = GetComponentInParent<CelestialBodyManager>();
        if(manager.GetBodies().Count > 0 && predict) SimulatePath();      
    }

    void SimulatePath()
    {
        //(re)set list data
        bodyClones.Clear();
        List<LineRenderer> paths = new List<LineRenderer>();
        List<List<Vector3>> pointArrayList = new List<List<Vector3>>();

        int relativeIndex = 0;
      
        //instantiate virtual bodies of real bodies
        for (int i = 0; i < manager.GetBodies().Count; i++)
        {
            VirtualBody bodyClone = new VirtualBody(manager.GetBodies()[i]);
            bodyClones.Add(bodyClone);
            paths.Add(manager.GetBodies()[i].SetPathLine());
            pointArrayList.Add(new List<Vector3>());
            if (relativeToBody != null && bodyClone.position == relativeToBody.transform.position) relativeIndex = i; 
        }

        //simulate virtual bodies
        for (int i = 0; i < timeSteps; i++)
        {
            for (int j = 0; j < bodyClones.Count; j++)
            {
                //simulate and draw paths if not colliding at step
                Vector3 calculateNewtonForce = CalculateNewton(bodyClones[j]);
                if (calculateNewtonForce == new Vector3(404, 404, 404)) break;                                
                bodyClones[j].velocity += calculateNewtonForce;
                bodyClones[j].position += bodyClones[j].velocity * Universe.timeStep;
                if (i % lineDetail == 0)
                {
                    Vector3 newPos = bodyClones[j].position;
                    if (relativeToBody) newPos -= (bodyClones[relativeIndex].position - relativeToBody.transform.position);
                    pointArrayList[j].Add(newPos);
                }               
            }
        }

        //set path positions from relative array
        for (int i = 0; i < bodyClones.Count; i++)
        {
            paths[i].positionCount = pointArrayList[i].Count;
            paths[i].SetPositions(pointArrayList[i].ToArray());
        }
    }

    Vector3 CalculateNewton(VirtualBody VB)
    {
        Vector3 forceVector = Vector3.zero;
        foreach (VirtualBody globe in bodyClones)
        {
            if (VB == globe) continue;

            //unit force direction vector
            Vector3 forceDirection = globe.position - VB.position;
            float forceDirectionLength = forceDirection.magnitude;

            //return specific vector if collided
            if (forceDirectionLength <= VB.radius + globe.radius) return new Vector3(404, 404, 404);

            forceDirection.Normalize();

            //force magnitude vector
            float force = Universe.G * (globe.mass * VB.mass) / (forceDirectionLength * forceDirectionLength);

            //total force vector
            forceVector += (forceDirection * force) / VB.mass;
        }
        return forceVector;
    }

    public bool GetActive() { return predict; }
    public void SetActive(bool status)
    {
        if (manager == null || status == predict) return;
        predict = status;
        foreach(CelestialBody body in manager.GetBodies()) { body.GetComponentInChildren<LineRenderer>().enabled = status; }
    }
    public void SetRelativeBody(CelestialBody body) { relativeToBody = body; }
    public void SetTimeSteps(int maxSteps) { timeSteps = maxSteps; }
    public int GetTimeSteps() { return timeSteps; }
}

//create fake "virtual" object of celestial body using its data
class VirtualBody
{
    public Vector3 position;
    public Vector3 velocity;
    public float mass;
    public float radius;

    public VirtualBody(CelestialBody CB)
    {
        if (CB == null) return;
        position = CB.transform.position;
        if(Application.isPlaying)  velocity = CB.GetVelocity(); 
        else velocity = CB.GetInitialVelocity(); 
        mass = CB.mass;
        radius = CB.transform.lossyScale.x / 2;
    }
}
