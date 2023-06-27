using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarSystemCreateData : MonoBehaviour
{
    GameObject systemObject;
    Camera cam2D;
    Camera cam3D;
    CelestialBodyManager manager;
    CelestialBodyPredictor predictor;

    public SolarSystemCreateData() { }

    public void OnStart(GameObject systemObject)
    {
        this.systemObject = systemObject;
        cam2D = systemObject.GetComponentsInChildren<Camera>()[0];
        cam3D = systemObject.GetComponentsInChildren<Camera>()[1];
        manager = systemObject.GetComponentInChildren<CelestialBodyManager>();
        predictor = systemObject.GetComponentInChildren<CelestialBodyPredictor>();
    }

    public GameObject GetSystemObject() { return systemObject; }
    public Camera Get2DCam() { return cam2D; }
    public Camera Get3DCam() { return cam3D; }
    public CelestialBodyManager GetManager() { return manager; }
    public CelestialBodyPredictor GetPredictor() { return predictor; }
}
