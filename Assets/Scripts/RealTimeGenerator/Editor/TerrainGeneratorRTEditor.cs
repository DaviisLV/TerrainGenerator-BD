using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(TerrainGeneratorRT))]
public class TerrainGeneratorRTEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
       // AddComponents();
    }
    private TerrainGeneratorRT _terGen;

    private void OnEnable()
    {
        _terGen = (TerrainGeneratorRT)target;

    }

  
}
