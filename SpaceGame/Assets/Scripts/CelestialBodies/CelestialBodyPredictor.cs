using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class CelestialBodyPredictor : MonoBehaviour
{
    [SerializeField, Range(5000, 100000)] private int timeSteps;
    [SerializeField, Range(1,250)] private int lineDetail;
    [SerializeField] private CelestialBody relativeToBody;

    List<VirtualBody> bodyClones = new List<VirtualBody>();

    private void Update()
    { 
        if(CelestialBodyManager.bodies.Count > 0) SimulatePath();      
    }

    void SimulatePath()
    {
        bodyClones.Clear();
        List<LineRenderer> paths = new List<LineRenderer>();
        List<List<Vector3>> pointArrayList = new List<List<Vector3>>();

        int relativeIndex = 0;
      
        for (int i = 0; i < CelestialBodyManager.bodies.Count; i++)
        {
            VirtualBody bodyClone = new VirtualBody(CelestialBodyManager.bodies[i]);
            bodyClones.Add(bodyClone);
            paths.Add(CelestialBodyManager.bodies[i].GetComponent<LineRenderer>());
            paths[i].startColor = CelestialBodyManager.bodies[i].GetComponent<MeshRenderer>().sharedMaterial.color;
            paths[i].endColor = CelestialBodyManager.bodies[i].GetComponent<MeshRenderer>().sharedMaterial.color;
            paths[i].widthMultiplier = 1;
            pointArrayList.Add(new List<Vector3>());

            if (relativeToBody != null && bodyClone.position == relativeToBody.transform.position) relativeIndex = i; 
        }

        for (int i = 0; i < timeSteps; i++)
        {
            for (int j = 0; j < bodyClones.Count; j++)
            {
                //simulate and draw paths
                Vector3 calculateNewtonForce = CalculateNewton(bodyClones[j]);
                if (calculateNewtonForce != new Vector3(404, 404, 404))
                {                
                    bodyClones[j].velocity += calculateNewtonForce;
                    bodyClones[j].position += bodyClones[j].velocity * Universe.timeStep;
                    if (i % lineDetail == 0)
                    {
                        Vector3 newPos = bodyClones[j].position;
                        if (relativeToBody) newPos -= (bodyClones[relativeIndex].position - relativeToBody.transform.position);
                        pointArrayList[j].Add(newPos);
                    }
                }
                else
                {
                    paths[j].startColor = Color.red;
                    paths[j].endColor = Color.red;
                    break;
                }
            }
        }

        for (int i = 0; i < bodyClones.Count; i++)
        {
            paths[i].positionCount = pointArrayList[i].Count;
            paths[i].SetPositions(pointArrayList[i].ToArray());
        }
    }

    Vector3 CalculateNewton(VirtualBody VB)
    {
        Vector3 forceVector = Vector3.zero;
        foreach (VirtualBody clone in bodyClones)
        {
            if (VB == clone) continue;

            //force direction 
            Vector3 forceDirection = clone.position - VB.position;
            float forceDirectionLength = forceDirection.magnitude;

            if(forceDirectionLength <= VB.radius + clone.radius)
            {
                return new Vector3(404, 404, 404);
            }

            forceDirection.Normalize();

            //force magnitude
            float force = Universe.G * (clone.mass * VB.mass) / (forceDirectionLength * forceDirectionLength);

            //force vector
            forceVector += (forceDirection * force) / VB.mass;
        }
        return forceVector;
    }
}

class VirtualBody
{
    public Vector3 position;
    public Vector3 velocity;
    public float mass;
    public float radius;

    public VirtualBody(CelestialBody CB)
    {
        position = CB.transform.position;
        if(Application.isPlaying)
        {
            velocity = CB.velocity;
        }
        else
        {
            velocity = CB.initialVelocity;
        }
        mass = CB.mass;
        radius = CB.transform.lossyScale.x / 2;
    }
}
