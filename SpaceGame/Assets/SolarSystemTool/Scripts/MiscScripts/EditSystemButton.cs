using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditSystemButton : MonoBehaviour { }

[CustomEditor(typeof(EditSystemButton))]
public class SystemButton : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Edit System"))
        {
            EditSystemButton component = (EditSystemButton)target;
            if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() == null) SolarSystemCreateWindow.Open(component.gameObject);
        }
    }
}
