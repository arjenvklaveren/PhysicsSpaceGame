using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CelestialBodyManager : MonoBehaviour
{
    public static List<CelestialBody> bodies = new List<CelestialBody>();
    public List<CelestialBody> bodiesClone = new List<CelestialBody>();

    private void OnValidate()
    {
        bodies = bodiesClone;
    }

    void FixedUpdate()
    {
        AddNewton();
        DetectCollision();
    }

    void DetectCollision()
    {

    }

    void AddNewton()
    {
        for (int i = 0; i < bodies.Count; i++)
        {
            bodies[i].AddForce(CalculateNewton(bodies[i]));        
            bodies[i].UpdatePosition();        
        }
    }

    Vector3 CalculateNewton(CelestialBody CB)
    {
        Vector3 forceVector = Vector3.zero;
        foreach (CelestialBody globe in bodies)
        {
            if (CB == globe) continue;

            //force direction 
            Vector3 forceDirection = globe.transform.position - CB.transform.position;
            float forceDirectionLength = forceDirection.magnitude;
            forceDirection.Normalize();

            //force magnitude
            float force = Universe.G * (globe.mass * CB.mass) / (forceDirectionLength * forceDirectionLength);

            //force vector
            forceVector += (forceDirection * force) / CB.mass;           
        }
        return forceVector;
    }
}
