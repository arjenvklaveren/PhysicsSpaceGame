using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarSystemCreateData : MonoBehaviour
{
    Camera cam2D;
    Camera cam3D;
    List<CelestialBody> bodies;
    GameObject systemObject;

    public SolarSystemCreateData() { }

    public void OnStart(GameObject systemObject)
    {
        this.systemObject = systemObject;
        cam2D = systemObject.GetComponentsInChildren<Camera>()[0];
        cam3D = systemObject.GetComponentsInChildren<Camera>()[1];
    }

    public Camera Get2DCam() { return cam2D; }
    public Camera Get3DCam() { return cam3D; }
}
