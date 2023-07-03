using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SolarSystemCreateWindow : EditorWindow
{
    static SolarSystemCreateWindow window;
    static SolarSystemCreateData systemData = new SolarSystemCreateData();

    static SolarSystemView2D solar2Dview;
    static SolarSystemObjectView3D object3Dview;
    static SolarSystemDataListView dataListview;

    static GameObject systemObject;
    static bool isCreate;
    static bool isNew;

    [MenuItem("Window/SolarSystemCreate")]
    static void Init()
    {
        systemObject = Instantiate(Resources.Load<GameObject>("Prefabs/System"));
        systemObject.transform.name = "New solar system";
        isCreate = false;
        isNew = true;
        SetData();
    }

    public static void Open(GameObject system)
    {
        systemObject = system;
        Instantiate(Resources.Load<GameObject>("Prefabs/2Dcam"), system.transform);
        Instantiate(Resources.Load<GameObject>("Prefabs/3Dcam"), system.transform);
        isCreate = false;
        isNew = false;
        SetData();
    }

    static void SetData()
    {
        window = GetWindow<SolarSystemCreateWindow>("Solar System Creation");
        window.maxSize = new Vector2(900, 600);
        window.minSize = new Vector2(900, 600);

        systemData.OnStart(systemObject);
        solar2Dview = new SolarSystemView2D(new Rect(5, 5, 600, 600), systemData);
        object3Dview = new SolarSystemObjectView3D(new Rect(430, 5, 175, 175), systemData);
        dataListview = new SolarSystemDataListView(new Rect(610, 5, 285, 550), systemData);
    }

    void OnGUI()
    {
        if (window == null) { this.Close(); }

        dataListview.GuiUpdate();
        solar2Dview.GuiUpdate();
        object3Dview.GuiUpdate();

        if (GUI.Button(new Rect(615, 565, 135, 30), new GUIContent("Cancel"))) { window.Close(); }
        if (GUI.Button(new Rect(755, 565, 135, 30), new GUIContent("Done"))) { CloseWindow(); }
        window.Repaint();
    }

    void AddRenderScript()
    {
        Camera mainCam = Camera.main;
        if (!mainCam.gameObject.GetComponent<OnRenderEvent>())
        {
            mainCam.gameObject.AddComponent<OnRenderEvent>();
            mainCam.nearClipPlane = 1;
            mainCam.farClipPlane = 20000;
        }
    }

    void CloseWindow()
    {
        isCreate = true;
        DestroyImmediate(systemData.Get2DCam().gameObject);
        DestroyImmediate(systemData.Get3DCam().gameObject);
        AddRenderScript();
        window.Close();
    }

    private void OnDestroy()
    {
        if (!isCreate && isNew) DestroyImmediate(systemObject);
        if (!isCreate && !isNew) CloseWindow();
    }
}
