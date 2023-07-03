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
        tex.SetPixel(1, 1, Color.black);
        tex.Apply();
        textFieldStyle.normal.background = tex;
        textFieldStyle.alignment = TextAnchor.MiddleCenter;
        textFieldStyle.fontStyle = FontStyle.Italic;

        labelStyleRight.fontSize = 13;
        labelStyleRight.normal.textColor = Color.white;
        labelStyleRight.alignment = TextAnchor.UpperRight;

        labelStyleLeft.fontSize = 13;
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
        int timeStepsMult = systemData.GetPredictor().GetTimeSteps() / 5000;
        timeStepsMult = (int)GUI.HorizontalSlider(new Rect(rect.x + 100, 70, 145, 20), timeStepsMult, 1, 5);
        systemData.GetPredictor().SetTimeSteps(timeStepsMult * 5000);
        EditorGUI.LabelField(new Rect(rect.x + 200, 70, 70, 20), timeStepsMult.ToString(), labelStyleRight);

        EditorGUI.DrawRect(new Rect(rect.x + 20, 105, 245, 1), Color.gray);
    }
    public void DrawBodyEditor()
    {
        Rect fitRect = new Rect(rect.x + borderSize, rect.y + borderSize + 100, rect.width - (borderSize * 2), 35);
        systemData.GetSelectedBody().transform.name = EditorGUI.DelayedTextField(fitRect, systemData.GetSelectedBody().transform.name, textFieldStyle);

        EditorGUI.LabelField(new Rect(rect.x + 15, 145, 70, 20), "Mass: ", labelStyleLeft);
        selectedBody.mass = (int)GUI.HorizontalSlider(new Rect(rect.x + 75, 145, 125, 20), selectedBody.mass, 100, 1000000);
        selectedBody.mass = (int)SliderStep((float)selectedBody.mass, 100.0f, 100.0f);
        EditorGUI.LabelField(new Rect(rect.x + 200, 145, 70, 20), selectedBody.mass.ToString(), labelStyleRight);

        EditorGUI.LabelField(new Rect(rect.x + 15, 165, 70, 20), "Scale: ", labelStyleLeft);
        selectedBody.scaleOffset = (int)GUI.HorizontalSlider(new Rect(rect.x + 75, 165, 160, 20), selectedBody.scaleOffset, -10, 10);
        EditorGUI.LabelField(new Rect(rect.x + 200, 165, 70, 20), selectedBody.scaleOffset.ToString(), labelStyleRight);

        selectedBody.SetSize();

        Vector3 startVelocity = selectedBody.GetInitialVelocity();

        float angle = Vector3.Angle(Vector3.forward, startVelocity) * Mathf.Sign(Vector3.Dot(Vector3.up, Vector3.Cross(Vector3.forward, startVelocity)));
        float speed = startVelocity.magnitude;

        EditorGUI.LabelField(new Rect(rect.x + 15, 185, 70, 20), "Angle: ", labelStyleLeft);
        angle = GUI.HorizontalSlider(new Rect(rect.x + 75, 185, 165, 20), angle, -180, 180);
        EditorGUI.LabelField(new Rect(rect.x + 200, 185, 70, 20), ((int)angle).ToString(), labelStyleRight);

        EditorGUI.LabelField(new Rect(rect.x + 15, 205, 70, 20), "Speed: ", labelStyleLeft);
        speed = GUI.HorizontalSlider(new Rect(rect.x + 75, 205, 150, 20), speed, 1, 1000);
        EditorGUI.LabelField(new Rect(rect.x + 200, 205, 70, 20), ((int)speed).ToString(), labelStyleRight);

        Vector3 newVelocity = Quaternion.Euler(0, angle, 0) * Vector3.forward * speed;
        selectedBody.SetInitialVelocity(newVelocity);
    }

    public void DrawExtraEditor()
    {
        int offsetValue = 0;

        if(systemData.GetSelectedBody().GetComponentInChildren<BodyRings>())
        {
            BodyRings rings = systemData.GetSelectedBody().GetComponentInChildren<BodyRings>();

            EditorGUI.DrawRect(new Rect(rect.x + 20, 235, 245, 1), Color.gray);

            EditorGUI.LabelField(new Rect(rect.x + 15, 245, 70, 20), "Ring width: ", labelStyleLeft);
            rings.ringWidth = GUI.HorizontalSlider(new Rect(rect.x + 100, 245, 130, 20), rings.ringWidth, 0.25f, 4.0f);
            rings.ringWidth = SliderStep(rings.ringWidth, 0.25f, 0.25f);
            EditorGUI.LabelField(new Rect(rect.x + 200, 245, 70, 20), rings.ringWidth.ToString(), labelStyleRight);

            EditorGUI.LabelField(new Rect(rect.x + 15, 265, 70, 20), "Ring offset: ", labelStyleLeft);
            rings.ringOffset = GUI.HorizontalSlider(new Rect(rect.x + 100, 265, 135, 20), rings.ringOffset, 0.0f, 2.0f);
            rings.ringOffset = SliderStep(rings.ringOffset, 0.0f, 0.25f);
            EditorGUI.LabelField(new Rect(rect.x + 200, 265, 70, 20), rings.ringOffset.ToString(), labelStyleRight);

            EditorGUI.LabelField(new Rect(rect.x + 15, 285, 70, 20), "Ring tilt: ", labelStyleLeft);
            rings.tilt = (int)GUI.HorizontalSlider(new Rect(rect.x + 100, 285, 135, 20), rings.tilt, -180, 180);
            EditorGUI.LabelField(new Rect(rect.x + 200, 285, 70, 20), rings.tilt.ToString(), labelStyleRight);

            if (GUI.Button(new Rect(rect.x + 30, 310, 225, 20), new GUIContent("Edit ring texture"))) { RingTextureDrawWindow.Open(rings); }

            rings.OnEditRing();

            offsetValue = 110;
        }

        EditorGUI.DrawRect(new Rect(rect.x + 20, 235 + offsetValue, 245, 1), Color.gray);
        if (GUI.Button(new Rect(rect.x + 15, 250 + offsetValue, 70, 20), new GUIContent("Focus")))
        {
            float yPos = systemData.Get2DCam().transform.localPosition.y - systemData.Get3DCam().transform.localPosition.y;
            systemData.Get2DCam().transform.localPosition = systemData.Get3DCam().transform.localPosition + new Vector3(0,yPos,0);
        }

        if (GUI.Button(new Rect(rect.x + 108, 250 + offsetValue, 70, 20), new GUIContent("Relative"))) { systemData.GetPredictor().SetRelativeBody(systemData.GetSelectedBody()); }
        if (GUI.Button(new Rect(rect.x + 200, 250 + offsetValue, 70, 20), new GUIContent("Delete")))
        {
            if (systemData.GetManager().bodies.Count == 0) return;
            systemData.GetManager().RemoveBody(systemData.GetSelectedBody());
            systemData.SetSelectedBody(systemData.GetManager().bodies[0]);
        }
    }

    private float SliderStep(float value, float min, float step)
    {
        if (step == 0) return value;
        float newValue = min + Mathf.Round((value - min) / step) * step;
        return (float)newValue;
    }
}
