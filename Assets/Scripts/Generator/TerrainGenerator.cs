using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {

    #region Terain_Properties
    [HideInInspector]
    public string _FilePath;
    [HideInInspector]
    public int _ResolutionSelected = -1;
    [HideInInspector]
    public Vector3Int _TerrainSizeData;
    [HideInInspector]
    public GameObject _TerrainOrigin;
    [HideInInspector]
    public TerrainData _TerrainData;
    [HideInInspector]
    public int _HightMapRezaliton = 0;
    #endregion

    #region All_Booleans
    [HideInInspector]
    public bool _TerrainSizeSelected = false;    
    [HideInInspector]
    public bool _SplitTerrain = false;
    [HideInInspector]
    public bool _AddTexture = false;
    [HideInInspector]
    public bool _AddTrees = false;
    [HideInInspector]
    public bool _AddGrass = false; 
    #endregion

    #region Terrain_Split
    [HideInInspector]
    public int _SplitCountID;
    [HideInInspector]
    public float _Step;
    #endregion

    #region Trees_Properties  
    [HideInInspector]
    public int _TreesPrefabCount = 0;
    [HideInInspector]
    public int _TreeSpacing = 30;
    [HideInInspector]
    public int _TreesMaxReliefSlope = 45;
    [HideInInspector]
    public GameObject[] _Trees;
    [HideInInspector]
    public GameObject[] _TreesArr;
    [HideInInspector]
    public TreePrototype[] _TreeData;
    #endregion

    #region Grass_Properties 
    [HideInInspector]
    public int _GrassMaxReliefSlope = 40;
    [HideInInspector]
    public Texture2D _Grass;
    [HideInInspector]
    public DetailPrototype[] _DetailData;
    [HideInInspector]
    public int _GrassDistance = 400;
    #endregion

    #region Texture_Properties
    [HideInInspector]
    public Texture2D _TerTexture;  
    [HideInInspector]
    public SplatPrototype[] _TerrainTexture = new SplatPrototype[1];
    #endregion

}
