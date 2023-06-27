using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SolarSystemDataListView
{
    Rect rect;
    float borderSize = 2;
    SolarSystemCreateData systemData;

    public SolarSystemDataListView(Rect rect, SolarSystemCreateData data)
    {
        this.rect = rect;
    }
    public void Draw()
    {
        EditorGUI.DrawRect(rect, Color.grey);
        EditorGUI.DrawRect(new Rect(rect.x + borderSize, rect.y + borderSize, rect.width - (borderSize * 2), rect.height - (borderSize * 2)), Color.black);
    }
}
