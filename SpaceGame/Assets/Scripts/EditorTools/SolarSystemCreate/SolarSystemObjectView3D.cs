using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SolarSystemObjectView3D : MonoBehaviour
{
    Rect rect;

    float borderSize = 2;

    SolarSystemCreateData systemData;
    Camera cam3D;

    public SolarSystemObjectView3D(Rect rect, SolarSystemCreateData data)
    {
        this.rect = rect;
        systemData = data;
        cam3D = data.Get3DCam();
    }

    public void Draw()
    {
        EditorGUI.DrawRect(rect, Color.grey);
        Rect innerRect = new Rect(rect.x + borderSize, rect.y + borderSize, rect.width - (borderSize * 2), rect.height - (borderSize * 2));
        EditorGUI.DrawRect(innerRect, Color.black);
        Handles.DrawCamera(innerRect, cam3D);
    }
}
