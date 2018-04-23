using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {

    [HideInInspector]
    public Vector3Int TerrainSizeData;
    [HideInInspector]
    public string _filePath;
    [HideInInspector]
    public bool _fileSelected = false;
    [HideInInspector]
    public bool _terrainSizeSelected = false;
    [HideInInspector]
    public int _resalutionSelekted = -1;
    [HideInInspector]
    public Vector3Int _terrainSizeData;
    [HideInInspector]
    public bool _splitTerrain = false;
    [HideInInspector]
    public bool _addTexture = false;
    [HideInInspector]
    public int _splitCountID;
    [HideInInspector]
    public Texture2D TerTexture;
    [HideInInspector]
    public List<GameObject> _terrainList;
}
