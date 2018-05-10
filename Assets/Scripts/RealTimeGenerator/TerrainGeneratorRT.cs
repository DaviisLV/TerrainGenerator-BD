using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.Linq;

public class TerrainGeneratorRT : MonoBehaviour
{
    #region  All_Variable
    // The object that moves through the terrain 
    public GameObject Player;

    #region Terain_Properties
    [HideInInspector]
    public Vector3Int _TerrainSizeData;
    [HideInInspector]
    public string _FilePath;
    [HideInInspector]
    public int _ResolutionSelected = -1;

    private GameObject _terrainOrigin;
    private TerrainData _terrainData;
    private int _hightMapRezaliton = 0;
    #endregion

    #region All_Booleans
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
    public int _SplitCount;
    [HideInInspector]
    public int _SplitCountID;
    [HideInInspector]
    #endregion

    #region Runtime_Path_Generation
    public int _RadiusOfGeneration = 0;
    [HideInInspector]
    public float _Step;
    [HideInInspector]
    public List<GameObject> _TerrainList;
    #endregion

    #region Trees_Properties
    [HideInInspector]
    public int _TreesPrefabCount =0;
    [HideInInspector]
    public int _TreesMaxReliefSlope = 45;
    [HideInInspector]
    public int _TreeSpacing = 30;
    [HideInInspector]
    public GameObject[] _Trees = new GameObject[0];

    private TreePrototype[] _treeData;
    #endregion]

    #region Grass_Properties
    [HideInInspector]
    public int _GrassMaxReliefSlope = 40;
    [HideInInspector]
    public Texture2D _Grass;
    [HideInInspector]
    public int _GrassDistance = 400;

    private DetailPrototype[] _detailData;

    #endregion

    #region Texture_Properties
    public SplatPrototype[] _TerrainTexture = new SplatPrototype[1];
    [HideInInspector]
    public Texture2D _TerTexture;

    #endregion

    #endregion

    public void Start()
    {
        //Get terrain resalution
        _hightMapRezaliton = GetTerrainRezalution(_ResolutionSelected);
        //Get split count
        _SplitCount = _SplitCountID + 2;
        CreateProtoTypes();
        CreateTerrain();
        FillTreeInstances(_terrainOrigin.GetComponent<Terrain>());
        FillDetailMap(_terrainOrigin.GetComponent<Terrain>());
        AddTerrainTexture(_terrainData);
        SplitTerrain();
        enableAll();
        SetPlayerPozition();
    }

    private void Update()
    {
        // Check Object movment
        playerMove();
    }

    #region Add_Terrain_Grass_Trees_Textures

    void CreateProtoTypes()
    {
        if (_AddTrees)
        {
            _treeData = new TreePrototype[_Trees.Length];
            for (int i = 0; i < _Trees.Length; i++)
            {
                _treeData[i] = new TreePrototype();
                _treeData[i].prefab = _Trees[i];
            }
        }
        if (_AddTexture)
        {
            _TerrainTexture[0] = new SplatPrototype();
            _TerrainTexture[0].texture = _TerTexture;
        }

        if (_AddGrass)
        {
            _detailData = new DetailPrototype[1];

            _detailData[0] = new DetailPrototype();
            _detailData[0].prototypeTexture = _Grass;
            _detailData[0].renderMode = DetailRenderMode.GrassBillboard;
        }

    }

    void FillTreeInstances(Terrain terrain)

    {
        if (!_AddTrees) return;

        for (int x = 0; x < _TerrainSizeData.z; x += _TreeSpacing)
        {
            for (int z = 0; z < _TerrainSizeData.z; z += _TreeSpacing)
            {

                float unitx = 1.0f / (_TerrainSizeData.x - 1);
                float unitz = 1.0f / (_TerrainSizeData.z - 1);

                float offsetX = UnityEngine.Random.value * unitx * _TreeSpacing;
                float offsetZ = UnityEngine.Random.value * unitz * _TreeSpacing;

                float normX = x * unitx + offsetX;
                float normZ = z * unitz + offsetZ;

                float angle = terrain.terrainData.GetSteepness(normX, normZ);

                if (angle < _TreesMaxReliefSlope)
                {

                    float ht = terrain.terrainData.GetInterpolatedHeight(normX, normZ);

                    if (ht < terrain.terrainData.size.y * 0.4f)
                    {

                        TreeInstance temp = new TreeInstance();
                        temp.position = new Vector3(normX, ht, normZ);
                        temp.prototypeIndex = (int)UnityEngine.Random.Range(0, _Trees.Length);
                        temp.widthScale = 1;
                        temp.heightScale = 1;
                        temp.color = Color.white;
                        temp.lightmapColor = Color.white;

                        terrain.AddTreeInstance(temp);

                    }
                }

            }
        }
    }

    void FillDetailMap(Terrain terrain)
    {
        if (!_AddGrass) return;

        int[,] grassMap = new int[(int)terrain.terrainData.size.x, (int)terrain.terrainData.size.z];

        for (int x = 0; x < terrain.terrainData.size.x; x++)
        {
            for (int z = 0; z < terrain.terrainData.size.z; z++)
            {
                float unitx = 1.0f / (_TerrainSizeData.x - 1);
                float unitz = 1.0f / (_TerrainSizeData.z - 1);

                float offsetX = UnityEngine.Random.value * unitx;
                float offsetZ = UnityEngine.Random.value * unitz;

                float normX = x * unitx + offsetX;
                float normZ = z * unitz + offsetZ;

                float angle = terrain.terrainData.GetSteepness(normX, normZ);

                if (angle < _GrassMaxReliefSlope)
                    grassMap[z, x] = 1;


            }
        }
        terrain.detailObjectDistance = _GrassDistance;
        terrain.terrainData.SetDetailResolution(512, 8);
        terrain.terrainData.SetDetailLayer(0, 0, 0, grassMap);



    }

    public void AddTerrainTexture(TerrainData terrainData)
    {
        if (!_AddTexture) return;
        terrainData.splatPrototypes = _TerrainTexture;

    }
    #endregion

    #region Set_Player_Start_Position
    //Place Player object in the midle of terrain
    private void SetPlayerPozition()
    {
        RaycastHit hit;
        Ray ray = new Ray(new Vector3(_TerrainSizeData.x / 2, _TerrainSizeData.y, _TerrainSizeData.z / 2) + Vector3.up * 100, Vector3.down);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (hit.collider != null)
            {
                Player.transform.position = new Vector3(_TerrainSizeData.x / 2, hit.point.y + 2, _TerrainSizeData.z / 2);
            }
        }
    }
    #endregion

    #region Split_Terrain_Generation

    private void playerMove()
    {
        if (!_SplitTerrain) return;
        foreach (GameObject g in _TerrainList)
        {

            Vector3 centre = new Vector3(g.transform.position.x + (_Step / 2), 0, g.transform.position.z + (_Step / 2));

            float distance = Vector3.Distance(new Vector3(Player.transform.position.x, 0, Player.transform.position.z), centre);

            if (distance < _RadiusOfGeneration)
                g.SetActive(true);
            if (distance > _RadiusOfGeneration * 2)
                if (g.active)
                    g.SetActive(false);
        }
    }

    private void enableAll()
    {
        if (!_SplitTerrain) return;
        foreach (GameObject g in _TerrainList)
        {
            g.SetActive(false);
        }
    }
    #endregion

    #region Create_Terran_From_File

    public void CreateTerrain()
    {
        //Create terrain data and generate terrain
        _terrainData = new TerrainData
        {
            heightmapResolution = _hightMapRezaliton,
            size = _TerrainSizeData,
            treePrototypes = _treeData,
            detailPrototypes = _detailData


        };

        LoadTerrain(_FilePath, _terrainData);
        _terrainOrigin = Terrain.CreateTerrainGameObject(_terrainData);
        _terrainOrigin.transform.parent = transform;
    }

    void LoadTerrain(string aFileName, TerrainData aTerrain)
    {
        int h = aTerrain.heightmapHeight;
        int w = aTerrain.heightmapWidth;
        float[,] data = new float[h, w];
        using (var file = System.IO.File.OpenRead(aFileName))
        using (var reader = new System.IO.BinaryReader(file))
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float v = (float)reader.ReadUInt16() / 0xFFFF;
                    data[y, x] = v;
                }
            }
        }
        aTerrain.SetHeights(0, 0, data);
    }
    //Resaluton from editor script
    private int GetTerrainRezalution(int arrayIndex)
    {
        switch (arrayIndex)
        {
            case 0:
                return 33;
            case 1:
                return 65;
            case 2:
                return 129;
            case 3:
                return 257;
            case 4:
                return 513;
            case 5:
                return 1025;
            case 6:
                return 2049;
            case 7:
                return 4097;
            default:
                return 0;
        }
    }

    #endregion

    #region Split_Terrain
    
    public void SplitTerrain()
    {
        if (!_SplitTerrain) return;

        Terrain _originalTerrain = _terrainOrigin.GetComponent<Terrain>();
        if (_originalTerrain == null) return;

        _Step = _originalTerrain.terrainData.size.x / _SplitCount;
        //splite terrain
        for (int x = 0; x < _SplitCount; x++)
        {

            for (int z = 0; z < _SplitCount; z++)
            {
                float xMin = _originalTerrain.terrainData.size.x / _SplitCount * x;
                float xMax = _originalTerrain.terrainData.size.x / _SplitCount * (x + 1);
                float zMin = _originalTerrain.terrainData.size.z / _SplitCount * z;
                float zMax = _originalTerrain.terrainData.size.z / _SplitCount * (z + 1);
                copyTerrain(_originalTerrain, string.Format("{0}{1}_{2}", _originalTerrain.name, x, z), xMin, xMax, zMin, zMax, _hightMapRezaliton, _terrainData.detailResolution, _terrainData.alphamapResolution);
            }
        }
        //Put terrain beck together
        for (int x = 0; x < _SplitCount; x++)
        {
            for (int z = 0; z < _SplitCount; z++)
            {

                GameObject center = GameObject.Find(string.Format("{0}{1}_{2}", _originalTerrain.name, x, z));
                GameObject left = GameObject.Find(string.Format("{0}{1}_{2}", _originalTerrain.name, x - 1, z));
                GameObject top = GameObject.Find(string.Format("{0}{1}_{2}", _originalTerrain.name, x, z + 1));
                stitchTerrain(center, left, top);
            }

        }
        //Destroy original
        Destroy(_terrainOrigin);
    }


    void copyTerrain(Terrain origTerrain, string newName, float xMin, float xMax, float zMin, float zMax, int heightmapResolution, int detailResolution, int alphamapResolution)
    {

        if (xMin < 0 || xMin > xMax || xMax > origTerrain.terrainData.size.x || zMin < 0 || zMin > zMax || zMax > origTerrain.terrainData.size.z) return;

        TerrainData td = new TerrainData();
        GameObject TerGameObeject = Terrain.CreateTerrainGameObject(td);
        Terrain newTerrain = TerGameObeject.GetComponent<Terrain>();

        // Copy over all values
        newTerrain.bakeLightProbesForTrees = origTerrain.bakeLightProbesForTrees;
        newTerrain.basemapDistance = origTerrain.basemapDistance;
        newTerrain.castShadows = origTerrain.castShadows;
        newTerrain.collectDetailPatches = origTerrain.collectDetailPatches;
        newTerrain.detailObjectDensity = origTerrain.detailObjectDensity;
        newTerrain.detailObjectDistance = origTerrain.detailObjectDistance;
        newTerrain.drawHeightmap = origTerrain.drawHeightmap;
        newTerrain.drawTreesAndFoliage = origTerrain.drawTreesAndFoliage;
        newTerrain.editorRenderFlags = origTerrain.editorRenderFlags;
        newTerrain.heightmapMaximumLOD = origTerrain.heightmapMaximumLOD;
        newTerrain.heightmapPixelError = origTerrain.heightmapPixelError;
        newTerrain.legacyShininess = origTerrain.legacyShininess;
        newTerrain.legacySpecular = origTerrain.legacySpecular;
        newTerrain.lightmapIndex = origTerrain.lightmapIndex;
        newTerrain.lightmapScaleOffset = origTerrain.lightmapScaleOffset;
        newTerrain.materialTemplate = origTerrain.materialTemplate;
        newTerrain.materialType = origTerrain.materialType;
        newTerrain.realtimeLightmapIndex = origTerrain.realtimeLightmapIndex;
        newTerrain.realtimeLightmapScaleOffset = origTerrain.realtimeLightmapScaleOffset;
        newTerrain.reflectionProbeUsage = origTerrain.reflectionProbeUsage;
        newTerrain.treeBillboardDistance = origTerrain.treeBillboardDistance;
        newTerrain.treeCrossFadeLength = origTerrain.treeCrossFadeLength;
        newTerrain.treeDistance = origTerrain.treeDistance;
        newTerrain.treeMaximumFullLODCount = origTerrain.treeMaximumFullLODCount;

        td.splatPrototypes = origTerrain.terrainData.splatPrototypes;
        td.treePrototypes = origTerrain.terrainData.treePrototypes;
        td.detailPrototypes = origTerrain.terrainData.detailPrototypes;

        float xMinNorm = xMin / origTerrain.terrainData.size.x;
        float xMaxNorm = xMax / origTerrain.terrainData.size.x;
        float zMinNorm = zMin / origTerrain.terrainData.size.z;
        float zMaxNorm = zMax / origTerrain.terrainData.size.z;

        float dimRatio1, dimRatio2;

        //Set height for split terrain part
        td.heightmapResolution = heightmapResolution;
        float[,] newHeights = new float[heightmapResolution, heightmapResolution];
        dimRatio1 = (xMax - xMin) / heightmapResolution;
        dimRatio2 = (zMax - zMin) / heightmapResolution;
        for (int i = 0; i < heightmapResolution; i++)
        {
            for (int j = 0; j < heightmapResolution; j++)
            {
                newHeights[j, i] = origTerrain.SampleHeight(new Vector3(xMin + (i * dimRatio1), 0, zMin + (j * dimRatio2))) / origTerrain.terrainData.size.y;
            }
        }
        td.SetHeightsDelayLOD(0, 0, newHeights);

        td.SetDetailResolution(detailResolution, 8);

        td.alphamapResolution = alphamapResolution;

        // Set tree for split terrain part
        for (int i = 0; i < origTerrain.terrainData.treeInstanceCount; i++)
        {
            TreeInstance ti = origTerrain.terrainData.treeInstances[i];
            if (ti.position.x < xMinNorm || ti.position.x >= xMaxNorm)
                continue;
            if (ti.position.z < zMinNorm || ti.position.z >= zMaxNorm)
                continue;
            ti.position = new Vector3(((ti.position.x * origTerrain.terrainData.size.x) - xMin) / (xMax - xMin), ti.position.y, ((ti.position.z * origTerrain.terrainData.size.z) - zMin) / (zMax - zMin));
            newTerrain.AddTreeInstance(ti);
        }
        //Set grass for split terrain part
        for (int layer = 0; layer < origTerrain.terrainData.detailPrototypes.Length; layer++)
        {
            int[,] detailLayer = origTerrain.terrainData.GetDetailLayer(
                    Mathf.FloorToInt(xMinNorm * origTerrain.terrainData.detailWidth),
                    Mathf.FloorToInt(zMinNorm * origTerrain.terrainData.detailHeight),
                    Mathf.FloorToInt((xMaxNorm - xMinNorm) * origTerrain.terrainData.detailWidth),
                    Mathf.FloorToInt((zMaxNorm - zMinNorm) * origTerrain.terrainData.detailHeight),
                    layer);
            int[,] newDetailLayer = new int[detailResolution, detailResolution];
            dimRatio1 = (float)detailLayer.GetLength(0) / detailResolution;
            dimRatio2 = (float)detailLayer.GetLength(1) / detailResolution;
            for (int i = 0; i < newDetailLayer.GetLength(0); i++)
            {
                for (int j = 0; j < newDetailLayer.GetLength(1); j++)
                {
                    newDetailLayer[i, j] = detailLayer[Mathf.FloorToInt(i * dimRatio1), Mathf.FloorToInt(j * dimRatio2)];
                }
            }
            td.SetDetailLayer(0, 0, layer, newDetailLayer);
        }

        TerGameObeject.transform.position = new Vector3(origTerrain.transform.position.x + xMin, origTerrain.transform.position.y, origTerrain.transform.position.z + zMin);
        TerGameObeject.name = newName;

        TerGameObeject.transform.parent = transform;

        td.size = new Vector3(xMax - xMin, origTerrain.terrainData.size.y, zMax - zMin);

        _TerrainList.Add(TerGameObeject);

    }


    void stitchTerrain(GameObject center, GameObject left, GameObject top)
    {
        //Connect the relief areas created in the correct order
        if (center == null)
            return;
        Terrain centerTerrain = center.GetComponent<Terrain>();
        float[,] centerHeights = centerTerrain.terrainData.GetHeights(0, 0, centerTerrain.terrainData.heightmapWidth, centerTerrain.terrainData.heightmapHeight);
        if (top != null)
        {
            Terrain topTerrain = top.GetComponent<Terrain>();
            float[,] topHeights = topTerrain.terrainData.GetHeights(0, 0, topTerrain.terrainData.heightmapWidth, topTerrain.terrainData.heightmapHeight);
            if (topHeights.GetLength(0) != centerHeights.GetLength(0))
            {
                Debug.Log("Terrain sizes must be equal");
                return;
            }
            for (int i = 0; i < centerHeights.GetLength(1); i++)
            {
                centerHeights[centerHeights.GetLength(0) - 1, i] = topHeights[0, i];
            }
        }
        if (left != null)
        {
            Terrain leftTerrain = left.GetComponent<Terrain>();
            float[,] leftHeights = leftTerrain.terrainData.GetHeights(0, 0, leftTerrain.terrainData.heightmapWidth, leftTerrain.terrainData.heightmapHeight);
            if (leftHeights.GetLength(0) != centerHeights.GetLength(0))
            {
                Debug.Log("Terrain sizes must be equal");
                return;
            }
            for (int i = 0; i < centerHeights.GetLength(0); i++)
            {
                centerHeights[i, 0] = leftHeights[i, leftHeights.GetLength(1) - 1];
            }
        }
        centerTerrain.terrainData.SetHeights(0, 0, centerHeights);
    }
    #endregion
}

