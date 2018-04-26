using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {

    #region Terain_Properties
    [HideInInspector]
    public Vector3Int TerrainSizeData;
    [HideInInspector]
    public string _filePath;
    [HideInInspector]
    public int _resolutionSelected = -1;
    [HideInInspector]
    public Vector3Int _terrainSizeData;
    [HideInInspector]
    public GameObject _terrainOrigin;
    [HideInInspector]
    public TerrainData _terrainData;
    [HideInInspector]
    public int _hightMapRezaliton = 0;
    #endregion

    #region All_Booleans
    [HideInInspector]
    public bool _terrainSizeSelected = false;    
    [HideInInspector]
    public bool _splitTerrain = false;
    [HideInInspector]
    public bool _addTexture = false;
    [HideInInspector]
    public bool _addTrees = false;
    [HideInInspector]
    public bool _addGrass = false; 
    #endregion

    #region Terrain_Split
    [HideInInspector]
    public int _splitCountID;
    [HideInInspector]
    public float _step;
    #endregion

    #region Trees_Properties  
    [HideInInspector]
    public int _treesPrefabCount = 0;
    [HideInInspector]
    public int _treeSpacing = 30;
    [HideInInspector]
    public int _treesMaxReliefSlope = 45;
    [HideInInspector]
    public GameObject[] Trees;
    public GameObject[] _treesArr;
    public TreePrototype[] _treeData;
    #endregion

    #region Grass_Properties 
    [HideInInspector]
    public int _grassMaxReliefSlope = 40;
    [HideInInspector]
    public Texture2D Grass;
    [HideInInspector]
    public DetailPrototype[] _detailData;
    #endregion

    #region Texture_Properties
    [HideInInspector]
    public Texture2D TerTexture;
    [HideInInspector]
    public SplatPrototype[] terrainTexture = new SplatPrototype[1];
    #endregion
}
