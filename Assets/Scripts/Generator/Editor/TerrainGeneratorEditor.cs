using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor
{
    private TerrainGenerator _terGen;
    private string[] _splitChoices = new[] { "4", "9", "16", "25", "36", "49", "64", "81", "100", "121", "144", "169", "196", "225" };
    private string[] _resalution = new[] { "33×33", "65×65", "129×129", "257×257", "513×513", "1025×1025", "2049×2049", "4097×4097" };
    private float _step;
    private GameObject _terrainOrigin;
    private TerrainData _terrainData;
    private int _hightMapRezaliton = 0;
    SplatPrototype[] terrainTexture = new SplatPrototype[1];
    private int _splitCount;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (_terGen._filePath == null)
            EditorGUILayout.HelpBox("Select file!", MessageType.Warning);
        else
            EditorGUILayout.HelpBox("Selected file path: " + _terGen._filePath, MessageType.Info);


        if (GUILayout.Button("Select terrain file"))
            _terGen._filePath = EditorUtility.OpenFilePanel("Select terrain file", "", "raw");


        if (_terGen._resalutionSelekted < 0)
            EditorGUILayout.HelpBox("Select resalution!", MessageType.Warning);

        EditorGUILayout.HelpBox("Terrain resalution can be only the same as terrain file or smaller", MessageType.None);
        _terGen._resalutionSelekted = EditorGUILayout.Popup("Terain resulation: ", _terGen._resalutionSelekted, _resalution);

        EditorGUILayout.HelpBox("Terrain data: X = Width, Y = Height, Z = Length", MessageType.None);


        if (_terGen._terrainSizeData.x <= 0 || _terGen._terrainSizeData.y <= 0 || _terGen._terrainSizeData.z <= 0)
            EditorGUILayout.HelpBox("Add terrain size!", MessageType.Error);


        _terGen._terrainSizeData = EditorGUILayout.Vector3IntField("Terrain size:", _terGen._terrainSizeData);


        _terGen._splitTerrain = EditorGUILayout.Toggle("Split terrain", _terGen._splitTerrain);

        if (_terGen._splitTerrain)
        {
            _terGen._splitCountID = EditorGUILayout.Popup("Number of pieces", _terGen._splitCountID, _splitChoices);

            if (_terGen._terrainSizeData.x <= _terGen._terrainSizeData.z)
                _step = _terGen._terrainSizeData.z / (_terGen._splitCountID + 2);
            else
                _step = _terGen._terrainSizeData.x / (_terGen._splitCountID + 2);

        }

        _terGen._addTexture = EditorGUILayout.Toggle("Add Texture", _terGen._addTexture);

        if (_terGen._addTexture)

            _terGen.TerTexture = (Texture2D)EditorGUILayout.ObjectField("Texture", _terGen.TerTexture, typeof(Texture2D), false);


        if (GUILayout.Button("Remove Terrain"))
        {
            RemoveTerrain();
        }


        if (GUILayout.Button("Create Terrain"))
        {
            RemoveTerrain();
            CreateTerrain();
            if (_terGen._addTexture)
                Addtexturess(_terrainData);
            if (_terGen._splitTerrain)
                SplitTerrain();

        }

        EditorUtility.SetDirty(_terGen);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }


    private void OnEnable()
    {
        _terGen = (TerrainGenerator)target;

    }

    #region Remove terrain

    private void RemoveTerrain()
    {
        for (int i = _terGen.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(_terGen.transform.GetChild(i).gameObject);
        }
    }

    #endregion

    #region Create terran from file

    public void CreateTerrain()
    {
        _hightMapRezaliton = GetTerrainRezalution(_terGen._splitCountID);
        _terrainData = new TerrainData
        {
            heightmapResolution = _hightMapRezaliton,
            size = _terGen._terrainSizeData
        };

        LoadTerrain(_terGen._filePath, _terrainData);
        _terrainOrigin = Terrain.CreateTerrainGameObject(_terrainData);
        _terrainOrigin.transform.parent = _terGen.transform;

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

    #region Add texture

    public void Addtexturess(TerrainData terrainData)
    {
        if (!_terGen._addTexture) return;
        terrainTexture[0] = new SplatPrototype();
        terrainTexture[0].texture = _terGen.TerTexture;
        terrainData.splatPrototypes = terrainTexture;

    }

    #endregion

    #region Split terrain
    public void SplitTerrain()
    {

        _splitCount = _terGen._splitCountID + 2;

        Terrain _originalTerrain = _terrainOrigin.GetComponent<Terrain>();
        if (_originalTerrain == null) return;

        _step = _originalTerrain.terrainData.size.x / _splitCount;

        for (int x = 0; x < _splitCount; x++)
        {

            for (int z = 0; z < _splitCount; z++)
            {
                float xMin = _originalTerrain.terrainData.size.x / _splitCount * x;
                float xMax = _originalTerrain.terrainData.size.x / _splitCount * (x + 1);
                float zMin = _originalTerrain.terrainData.size.z / _splitCount * z;
                float zMax = _originalTerrain.terrainData.size.z / _splitCount * (z + 1);

                copyTerrain(_originalTerrain, string.Format("{0}{1}_{2}", _originalTerrain.name, x, z), xMin, xMax, zMin, zMax, _hightMapRezaliton, _terrainData.detailResolution, _terrainData.alphamapResolution);
            }
        }

        for (int x = 0; x < _splitCount; x++)
        {
            for (int z = 0; z < _splitCount; z++)
            {

                GameObject center = GameObject.Find(string.Format("{0}{1}_{2}", _originalTerrain.name, x, z));
                Debug.Log(center);
                GameObject left = GameObject.Find(string.Format("{0}{1}_{2}", _originalTerrain.name, x - 1, z));
                GameObject top = GameObject.Find(string.Format("{0}{1}_{2}", _originalTerrain.name, x, z + 1));
                stitchTerrain(center, left, top);
            }

        }

        DestroyImmediate(_terrainOrigin);
    }


    void copyTerrain(Terrain origTerrain, string newName, float xMin, float xMax, float zMin, float zMax, int heightmapResolution, int detailResolution, int alphamapResolution)
    {

        if (xMin < 0 || xMin > xMax || xMax > origTerrain.terrainData.size.x || zMin < 0 || zMin > zMax || zMax > origTerrain.terrainData.size.z) return;

        TerrainData td = new TerrainData();
        GameObject TerGameObeject = Terrain.CreateTerrainGameObject(td);
        Terrain newTerrain = TerGameObeject.GetComponent<Terrain>();

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

        float dimRatio1, dimRatio2;
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

        TerGameObeject.transform.position = new Vector3(origTerrain.transform.position.x + xMin, origTerrain.transform.position.y, origTerrain.transform.position.z + zMin);
        TerGameObeject.name = newName;
        TerGameObeject.transform.parent = _terGen.transform;
        td.size = new Vector3(xMax - xMin, origTerrain.terrainData.size.y, zMax - zMin);

    }

    void stitchTerrain(GameObject center, GameObject left, GameObject top)
    {
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


