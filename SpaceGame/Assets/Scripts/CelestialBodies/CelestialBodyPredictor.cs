using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class CelestialBodyPredictor : MonoBehaviour
{
    [SerializeField, Range(100,10000)] private int timeSteps;
    [SerializeField, Range(1,10)] private int lineDetail;

    List<VirtualBody> bodyClones = new List<VirtualBody>();

    private void Update()
    { 
        SimulatePath();      
    }

    void SimulatePath()
    {
        bodyClones.Clear();
        List<LineRenderer> paths = new List<LineRenderer>();

        foreach(CelestialBody body in CelestialBodyManager.bodies)
        {
            VirtualBody bodyClone = new VirtualBody(body);
            bodyClones.Add(bodyClone);
            paths.Add(body.GetComponent<LineRenderer>());         
        }
        
        

        for(int i = 0; i < timeSteps; i++)
        {
            for (int j = 0; j < bodyClones.Count; j++)
            {               
                //simulate and draw paths
                bodyClones[j].velocity += CalculateNewton(bodyClones[j]);
                bodyClones[j].position += bodyClones[j].velocity * Universe.timeStep;
                

                paths[j].positionCount = i + 1;
                paths[j].SetPosition(i, bodyClones[j].position); 
                
            }
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
    }
}
