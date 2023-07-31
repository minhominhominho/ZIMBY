using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class RunTimeMapGeneratorManager : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MapGenerator initializer = (MapGenerator)target;
        if (GUILayout.Button("LoadFiles"))
        {
            initializer.LoadFiles();
        }
    }
}
#endif