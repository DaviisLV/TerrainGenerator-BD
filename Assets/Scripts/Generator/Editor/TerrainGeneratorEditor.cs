using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.Linq;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor
{
    private TerrainGenerator _terGen;
    private string[] _splitChoices = new[] { "4", "9", "16", "25", "36", "49", "64", "81", "100", "121", "144", "169", "196", "225" };
    private string[] _resalution = new[] { "33×33", "65×65", "129×129", "257×257", "513×513", "1025×1025", "2049×2049", "4097×4097" };
    private int _splitCount;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        #region Select_Terrain_File

        if (_terGen._FilePath == null)
            EditorGUILayout.HelpBox("Select file!", MessageType.Warning);
        else
            EditorGUILayout.HelpBox("Selected file path: " + _terGen._FilePath, MessageType.Info);


        if (GUILayout.Button("Select terrain file"))
            _terGen._FilePath = EditorUtility.OpenFilePanel("Select terrain file", "", "raw");
        #endregion

        #region Set_Terrain_Properties

        if (_terGen._ResolutionSelected < 0)
            EditorGUILayout.HelpBox("Select resalution!", MessageType.Warning);

        EditorGUILayout.HelpBox("Terrain resalution can be only the same as terrain file or smaller", MessageType.None);
        _terGen._ResolutionSelected = EditorGUILayout.Popup("Terain resulation: ", _terGen._ResolutionSelected, _resalution);

        EditorGUILayout.HelpBox("Terrain data: X = Width, Y = Height, Z = Length", MessageType.None);


        if (_terGen._TerrainSizeData.x <= 0 || _terGen._TerrainSizeData.y <= 0 || _terGen._TerrainSizeData.z <= 0)
            EditorGUILayout.HelpBox("Add terrain size!", MessageType.Error);


        _terGen._TerrainSizeData = EditorGUILayout.Vector3IntField("Terrain size:", _terGen._TerrainSizeData);
        #endregion

        #region Split_Terrain
        _terGen._SplitTerrain = EditorGUILayout.Toggle("Split terrain", _terGen._SplitTerrain);

        if (_terGen._SplitTerrain)
        {
            _terGen._SplitCountID = EditorGUILayout.Popup("Number of pieces", _terGen._SplitCountID, _splitChoices);

            if (_terGen._TerrainSizeData.x <= _terGen._TerrainSizeData.z)
                _terGen._Step = _terGen._TerrainSizeData.z / (_terGen._SplitCountID + 2);
            else
               _terGen._Step = _terGen._TerrainSizeData.x / (_terGen._SplitCountID + 2);

        }
        #endregion

        #region Add_Texture

        _terGen._AddTexture = EditorGUILayout.Toggle("Add Texture", _terGen._AddTexture);

        if (_terGen._AddTexture)

            _terGen._TerTexture = (Texture2D)EditorGUILayout.ObjectField("Texture", _terGen._TerTexture, typeof(Texture2D), false);
        #endregion

        #region Add_Trees

        _terGen._AddTrees = EditorGUILayout.Toggle("Add Trees", _terGen._AddTrees);
        if (_terGen._AddTrees)
        {
            if(_terGen._Trees== null)
            _terGen._Trees = new GameObject[0];
            _terGen._TreeSpacing = EditorGUILayout.IntSlider("Trees spacing", _terGen._TreeSpacing, 1, _terGen._TerrainSizeData.x / 2);
            _terGen._TreesMaxReliefSlope = EditorGUILayout.IntSlider("Max anngel for tree gen", _terGen._TreesMaxReliefSlope, 0, 90);
            _terGen._TreesPrefabCount = EditorGUILayout.IntSlider("Trees prefab count", _terGen._TreesPrefabCount, 1, 24);

            if (_terGen._TreesPrefabCount != _terGen._Trees.Length)
                EditorGUILayout.HelpBox("Press button to recauclate Trees prefab count ", MessageType.Info);

            if (GUILayout.Button("Recauculate prefab count", GUILayout.ExpandWidth(false)))
            {

                if (_terGen._TreesPrefabCount == _terGen._Trees.Length) return;
                _terGen._TreesArr = new GameObject[_terGen._TreesPrefabCount];
                _terGen._Trees = _terGen._TreesArr;


            }

            for (int i = 0; i < _terGen._Trees.Length; i++)
            {

                _terGen._Trees[i] = (GameObject)EditorGUILayout.ObjectField("Tree prefab " + (i + 1), _terGen._Trees[i], typeof(GameObject), false);
                if (_terGen._Trees[i] == null)
                    EditorGUILayout.HelpBox("Select tree prefab ", MessageType.Error);

            }

        }
        #endregion

        #region Add_Grass

        _terGen._AddGrass = EditorGUILayout.Toggle("Add Grass", _terGen._AddGrass);
        if (_terGen._AddGrass)
        {

            _terGen._GrassMaxReliefSlope = EditorGUILayout.IntSlider("Max angel for grass gen", _terGen._GrassMaxReliefSlope, 0, 90);
            _terGen._Grass = (Texture2D)EditorGUILayout.ObjectField("Grass texture", _terGen._Grass, typeof(Texture2D), false);
            EditorGUILayout.HelpBox("Distance at which details will no longer be drawn", MessageType.None);
            _terGen._GrassDistance = EditorGUILayout.IntField("Distance", _terGen._GrassDistance);

        }
        #endregion

        #region Buttons

        if (GUILayout.Button("Remove Terrain"))
        {
            RemoveTerrain();
        }

        if (GUILayout.Button("Create Terrain"))
        {
            RemoveTerrain();
            CreateProtoTypes();
            CreateTerrain();
            if (_terGen._AddTexture)
                Addtexturess(_terGen._TerrainData);
            if (_terGen._AddTrees)
                FillTreeInstances(_terGen._TerrainOrigin.GetComponent<Terrain>());
            if (_terGen._AddGrass)
                FillDetailMap(_terGen._TerrainOrigin.GetComponent<Terrain>());

            if (_terGen._SplitTerrain)
                SplitTerrain();

        }
        #endregion

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

    #region Add_Terrain_Grass_Trees_Textures

    void CreateProtoTypes()
    {
        if (_terGen._AddTrees)
        {
            _terGen._TreeData = new TreePrototype[_terGen._Trees.Length];
            for (int i = 0; i < _terGen._Trees.Length; i++)
            {
               
                _terGen._TreeData[i] = new TreePrototype();
                _terGen._TreeData[i].prefab = _terGen._Trees[i];

            }
        }
        if (_terGen._AddTexture)
        {
           _terGen._TerrainTexture[0] = new SplatPrototype();
           _terGen._TerrainTexture[0].texture = _terGen._TerTexture;

        }



        if (_terGen._AddGrass)
        {
            _terGen._DetailData = new DetailPrototype[1];

            _terGen._DetailData[0] = new DetailPrototype();
            _terGen._DetailData[0].prototypeTexture = _terGen._Grass;
            _terGen._DetailData[0].renderMode = DetailRenderMode.GrassBillboard;
        }

    }

    void FillTreeInstances(Terrain terrain)

    {
        if (!_terGen._AddTrees) return;

        for (int x = 0; x < _terGen._TerrainSizeData.z; x += _terGen._TreeSpacing)
        {
            for (int z = 0; z < _terGen._TerrainSizeData.z; z += _terGen._TreeSpacing)
            {

                float unitx = 1.0f / (_terGen._TerrainSizeData.x - 1);
                float unitz = 1.0f / (_terGen._TerrainSizeData.z - 1);

                float offsetX = UnityEngine.Random.value * unitx * _terGen._TreeSpacing;
                float offsetZ = UnityEngine.Random.value * unitz * _terGen._TreeSpacing;

                float normX = x * unitx + offsetX;
                float normZ = z * unitz + offsetZ;

                float angle = terrain.terrainData.GetSteepness(normX, normZ);

                if (angle < _terGen._TreesMaxReliefSlope)
                {

                    float ht = terrain.terrainData.GetInterpolatedHeight(normX, normZ);

                    if (ht < terrain.terrainData.size.y * 0.4f)
                    {

                        TreeInstance temp = new TreeInstance();
                        temp.position = new Vector3(normX, ht, normZ);
                        temp.prototypeIndex = (int)UnityEngine.Random.Range(0, _terGen._Trees.Length);
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
       
        if (!_terGen._AddGrass) return;

        int[,] detailMap0 = new int[(int)terrain.terrainData.size.x, (int)terrain.terrainData.size.z];

        for (int x = 0; x < terrain.terrainData.size.x; x++)
        {
            for (int z = 0; z < terrain.terrainData.size.z; z++)
            {
                float unitx = 1.0f / (_terGen._TerrainSizeData.x - 1);
                float unitz = 1.0f / (_terGen._TerrainSizeData.z - 1);

                float offsetX = UnityEngine.Random.value * unitx;
                float offsetZ = UnityEngine.Random.value * unitz;

                float normX = x * unitx + offsetX;
                float normZ = z * unitz + offsetZ;

                float angle = terrain.terrainData.GetSteepness(normX, normZ);

                if (angle < _terGen._TreesMaxReliefSlope)

                    detailMap0[z, x] = 1;
     

            }
        }

        terrain.detailObjectDistance = _terGen._GrassDistance;
        terrain.terrainData.SetDetailResolution(512, 8);
        terrain.terrainData.SetDetailLayer(0, 0, 0, detailMap0);



    }

    public void Addtexturess(TerrainData terrainData)
    {
        if (!_terGen._AddTexture) return;

        terrainData.splatPrototypes = _terGen._TerrainTexture;

    }
    #endregion

    #region Create terran from file

    public void CreateTerrain()
    {
        _terGen._HightMapRezaliton = GetTerrainRezalution(_terGen._SplitCountID);
        _terGen._TerrainData = new TerrainData
        {
            heightmapResolution = _terGen._HightMapRezaliton,
            size = _terGen._TerrainSizeData,
            treePrototypes = _terGen._TreeData,
            detailPrototypes = _terGen._DetailData
        };

        LoadTerrain(_terGen._FilePath, _terGen._TerrainData);
        _terGen._TerrainOrigin = Terrain.CreateTerrainGameObject(_terGen._TerrainData);
        _terGen._TerrainOrigin.transform.parent = _terGen.transform;

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

    #region Split terrain
    public void SplitTerrain()
    {

        _splitCount = _terGen._SplitCountID + 2;

        Terrain _originalTerrain = _terGen._TerrainOrigin.GetComponent<Terrain>();
        if (_originalTerrain == null) return;

        _terGen._Step = _originalTerrain.terrainData.size.x / _splitCount;

        for (int x = 0; x < _splitCount; x++)
        {

            for (int z = 0; z < _splitCount; z++)
            {
                float xMin = _originalTerrain.terrainData.size.x / _splitCount * x;
                float xMax = _originalTerrain.terrainData.size.x / _splitCount * (x + 1);
                float zMin = _originalTerrain.terrainData.size.z / _splitCount * z;
                float zMax = _originalTerrain.terrainData.size.z / _splitCount * (z + 1);

                copyTerrain(_originalTerrain, string.Format("{0}{1}_{2}", _originalTerrain.name, x, z), xMin, xMax, zMin, zMax, _terGen._HightMapRezaliton, _terGen._TerrainData.detailResolution, _terGen._TerrainData.alphamapResolution);
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

        DestroyImmediate(_terGen._TerrainOrigin);
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

        float xMinNorm = xMin / origTerrain.terrainData.size.x;
        float xMaxNorm = xMax / origTerrain.terrainData.size.x;
        float zMinNorm = zMin / origTerrain.terrainData.size.z;
        float zMaxNorm = zMax / origTerrain.terrainData.size.z;
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


        // Tree
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
        //grass
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


