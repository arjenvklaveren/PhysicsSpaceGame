using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class CelestialBodyManager : MonoBehaviour
{
    public List<CelestialBody> bodies = new List<CelestialBody>();

    void FixedUpdate()
    {
        MoveBodiesNewton(); 
    }

    //add newton force to bodies in list and update their positions
    void MoveBodiesNewton()
    {
        for (int i = 0; i < bodies.Count; i++)
        {
            bodies[i].AddForce(CalculateNewton(bodies[i]));
            bodies[i].UpdatePosition();
        }
    }

    //calculate newton force based on all bodies in list
    public Vector3 CalculateNewton(CelestialBody CB)
    {
        Vector3 forceVector = Vector3.zero;
        foreach (CelestialBody globe in bodies)
        {
            if (CB == globe) continue;

            //unit force direction vector
            Vector3 forceDirection = globe.transform.position - CB.transform.position;
            float forceDirectionLength = forceDirection.magnitude;

            //return specific vector if collided
            if (forceDirectionLength <= CB.GetRadius() + globe.GetRadius()) return new Vector3(404, 404, 404);

            forceDirection.Normalize();

            //force magnitude vector
            float force = Universe.G * (globe.mass * CB.mass) / (forceDirectionLength * forceDirectionLength);

            //total force vector
            forceVector += (forceDirection * force) / CB.mass;           
        }
        return forceVector;
    }

    public void AddBody(CelestialBody body)
    {
        bodies.Add(body);
    }
    public void RemoveBody(CelestialBody body)
    {
        bodies.Remove(body);
        DestroyImmediate(body.gameObject);
    }
    public List<CelestialBody> GetBodies()
    {
        return bodies;
    }
}
