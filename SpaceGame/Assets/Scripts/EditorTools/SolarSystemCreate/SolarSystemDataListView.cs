using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SolarSystemDataListView
{
    Rect rect;
    float borderSize = 2;
    SolarSystemCreateWindow window;

    public SolarSystemDataListView(Rect rect, SolarSystemCreateWindow window)
    {
        this.rect = rect;
        this.window = window;
    }
    public void Draw()
    {
        EditorGUI.DrawRect(rect, Color.grey);
        EditorGUI.DrawRect(new Rect(rect.x + borderSize, rect.y + borderSize, rect.width - (borderSize * 2), rect.height - (borderSize * 2)), Color.black);
    }
}
