using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SolarSystemDataListView
{
    Rect rect;
    float borderSize = 2;
    SolarSystemCreateData systemData;

    CelestialBody selectedBody;

    GUIStyle textFieldStyle = new GUIStyle();
    GUIStyle labelStyleRight = new GUIStyle();
    GUIStyle labelStyleLeft = new GUIStyle();

    public SolarSystemDataListView(Rect rect, SolarSystemCreateData data)
    {
        this.rect = rect;
        systemData = data;
        SetGUIStyles();
    }

    void SetGUIStyles()
    {
        textFieldStyle.normal.textColor = Color.white;
        textFieldStyle.fontSize = 15;
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(1, 1, Color.red);
        tex.Apply();
        textFieldStyle.normal.background = tex;
        textFieldStyle.alignment = TextAnchor.MiddleCenter;

        labelStyleRight.fontSize = 13;
        //labelStyleRight.normal.background = tex;
        labelStyleRight.normal.textColor = Color.white;
        labelStyleRight.alignment = TextAnchor.UpperRight;

        labelStyleLeft.fontSize = 13;
        //labelStyleLeft.normal.background = tex;
        labelStyleLeft.normal.textColor = Color.white;
        labelStyleLeft.alignment = TextAnchor.UpperLeft;
    }

    public void GuiUpdate()
    {
        EditorGUI.DrawRect(rect, Color.grey);
        EditorGUI.DrawRect(new Rect(rect.x + borderSize, rect.y + borderSize, rect.width - (borderSize * 2), rect.height - (borderSize * 2)), Color.black);
        selectedBody = systemData.GetSelectedBody();
        DrawUniverseEditor();
        DrawBodyEditor();
        DrawExtraEditor();
    }

    public void DrawUniverseEditor()
    {
        Rect fitRect = new Rect(rect.x + borderSize, rect.y + borderSize, rect.width - (borderSize * 2), 35);
        systemData.GetSystemObject().transform.name = EditorGUI.DelayedTextField(fitRect, systemData.GetSystemObject().transform.name, textFieldStyle);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y + fitRect.height, rect.width, 1), Color.gray);

        EditorGUI.LabelField(new Rect(rect.x + 15, 50, 70, 20), "G: ", labelStyleLeft);
        GUI.HorizontalSlider(new Rect(rect.x + 45, 50, 198, 20), 1, 0.1f, 10);
        EditorGUI.LabelField(new Rect(rect.x + 200, 50, 70, 20), "1", labelStyleRight);

        EditorGUI.LabelField(new Rect(rect.x + 15, 70, 70, 20), "Sim speed: ", labelStyleLeft);
        GUI.HorizontalSlider(new Rect(rect.x + 100, 70, 145, 20), 1, 0.1f, 10);
        EditorGUI.LabelField(new Rect(rect.x + 200, 70, 70, 20), "1", labelStyleRight);

        EditorGUI.DrawRect(new Rect(rect.x + 20, 105, 245, 1), Color.gray);
    }
    public void DrawBodyEditor()
    {
        Rect fitRect = new Rect(rect.x + borderSize, rect.y + borderSize + 100, rect.width - (borderSize * 2), 35);
        systemData.GetSelectedBody().transform.name = EditorGUI.DelayedTextField(fitRect, systemData.GetSelectedBody().transform.name, textFieldStyle);

        EditorGUI.LabelField(new Rect(rect.x + 15, 145, 70, 20), "Mass: ", labelStyleLeft);
        selectedBody.mass = (int)GUI.HorizontalSlider(new Rect(rect.x + 75, 145, 135, 20), selectedBody.mass, 100, 50000);
        EditorGUI.LabelField(new Rect(rect.x + 200, 145, 70, 20), selectedBody.mass.ToString(), labelStyleRight);

        EditorGUI.LabelField(new Rect(rect.x + 15, 165, 70, 20), "Scale: ", labelStyleLeft);
        selectedBody.scaleMass = (int)GUI.HorizontalSlider(new Rect(rect.x + 75, 165, 155, 20), selectedBody.scaleMass, 1, 100);
        EditorGUI.LabelField(new Rect(rect.x + 200, 165, 70, 20), selectedBody.scaleMass.ToString(), labelStyleRight);

        selectedBody.SetSize();


    }
    public void DrawExtraEditor()
    {

    }
}
//EditorGUI.DrawRect(new Rect(rect.x + 10, rect.y + fitRect.height + 10, 265, 1), Color.white);