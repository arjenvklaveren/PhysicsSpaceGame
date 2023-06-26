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

    [MenuItem("Window/SolarSystemCreate")]
    static void Init()
    {
        window = GetWindow<SolarSystemCreateWindow>("Solar System Creation");
        window.maxSize = new Vector2(900, 600);
        window.minSize = new Vector2(900, 600);
        isCreate = false;

        systemObject = Instantiate(Resources.Load<GameObject>("Prefabs/NewSystem"));
        systemData.OnStart(systemObject);
        solar2Dview = new SolarSystemView2D(new Rect(5, 5, 600, 600), window);
        object3Dview = new SolarSystemObjectView3D(new Rect(430, 5, 175, 175), window);
        dataListview = new SolarSystemDataListView(new Rect(610, 5, 285, 550), window);

        
    }

    public SolarSystemCreateData GetSystemData()
    {
        return systemData;
    }

    void OnGUI()
    {
        solar2Dview.Draw();
        object3Dview.Draw();
        dataListview.Draw();
        if (GUI.Button(new Rect(615, 565, 135, 30), new GUIContent("Cancel")))
        {
            window.Close();
        }
        if (GUI.Button(new Rect(755, 565, 135, 30), new GUIContent("Create")))
        {
            isCreate = true;
            window.Close();
        }
    }

    private void OnDestroy()
    {
        if(!isCreate) DestroyImmediate(systemObject);
    }
}
