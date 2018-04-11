using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {

    [HideInInspector]
    public Vector3Int TerrainSizeData;
    [HideInInspector]
    public string FilePath;
    [HideInInspector]
    public bool _fileSelected = false;
    [HideInInspector]
    public bool _terrainSizeSelected = false;

}
