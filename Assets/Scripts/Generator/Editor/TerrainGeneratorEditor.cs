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
    private int _splitCount;





    public int m_detailObjectDistance = 400; //The distance at which details will no longer be drawn


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        #region Select_Terrain_File

        if (_terGen._filePath == null)
            EditorGUILayout.HelpBox("Select file!", MessageType.Warning);
        else
            EditorGUILayout.HelpBox("Selected file path: " + _terGen._filePath, MessageType.Info);


        if (GUILayout.Button("Select terrain file"))
            _terGen._filePath = EditorUtility.OpenFilePanel("Select terrain file", "", "raw");
        #endregion

        #region Set_Terrain_Properties

        if (_terGen._resolutionSelected < 0)
            EditorGUILayout.HelpBox("Select resalution!", MessageType.Warning);

        EditorGUILayout.HelpBox("Terrain resalution can be only the same as terrain file or smaller", MessageType.None);
        _terGen._resolutionSelected = EditorGUILayout.Popup("Terain resulation: ", _terGen._resolutionSelected, _resalution);

        EditorGUILayout.HelpBox("Terrain data: X = Width, Y = Height, Z = Length", MessageType.None);


        if (_terGen._terrainSizeData.x <= 0 || _terGen._terrainSizeData.y <= 0 || _terGen._terrainSizeData.z <= 0)
            EditorGUILayout.HelpBox("Add terrain size!", MessageType.Error);


        _terGen._terrainSizeData = EditorGUILayout.Vector3IntField("Terrain size:", _terGen._terrainSizeData);
        #endregion

        #region Split_Terrain
        _terGen._splitTerrain = EditorGUILayout.Toggle("Split terrain", _terGen._splitTerrain);

        if (_terGen._splitTerrain)
        {
            _terGen._splitCountID = EditorGUILayout.Popup("Number of pieces", _terGen._splitCountID, _splitChoices);

            if (_terGen._terrainSizeData.x <= _terGen._terrainSizeData.z)
                _terGen._step = _terGen._terrainSizeData.z / (_terGen._splitCountID + 2);
            else
               _terGen._step = _terGen._terrainSizeData.x / (_terGen._splitCountID + 2);

        }
        #endregion

        #region Add_Texture

        _terGen._addTexture = EditorGUILayout.Toggle("Add Texture", _terGen._addTexture);

        if (_terGen._addTexture)

            _terGen.TerTexture = (Texture2D)EditorGUILayout.ObjectField("Texture", _terGen.TerTexture, typeof(Texture2D), false);
        #endregion

        #region Add_Trees

        _terGen._addTrees = EditorGUILayout.Toggle("Add Trees", _terGen._addTrees);
        if (_terGen._addTrees)
        {
            if(_terGen.Trees== null)
            _terGen.Trees = new GameObject[0];
            _terGen._treeSpacing = EditorGUILayout.IntSlider("Trees spacing", _terGen._treeSpacing, 1, _terGen._terrainSizeData.x / 2);
            _terGen._treesMaxReliefSlope = EditorGUILayout.IntSlider("Max anngel for tree gen", _terGen._treesMaxReliefSlope, 0, 90);
            _terGen._treesPrefabCount = EditorGUILayout.IntSlider("Trees prefab count", _terGen._treesPrefabCount, 1, 24);

            if (_terGen._treesPrefabCount != _terGen.Trees.Length)
                EditorGUILayout.HelpBox("Press button to recauclate Trees prefab count ", MessageType.Info);

            if (GUILayout.Button("Recauculate prefab count", GUILayout.ExpandWidth(false)))
            {

                if (_terGen._treesPrefabCount == _terGen.Trees.Length) return;
                _terGen._treesArr = new GameObject[_terGen._treesPrefabCount];
                _terGen.Trees = _terGen._treesArr;


            }

            for (int i = 0; i < _terGen.Trees.Length; i++)
            {

                _terGen.Trees[i] = (GameObject)EditorGUILayout.ObjectField("Tree prefab " + (i + 1), _terGen.Trees[i], typeof(GameObject), false);
                if (_terGen.Trees[i] == null)
                    EditorGUILayout.HelpBox("Select tree prefab ", MessageType.Error);

            }

        }
        #endregion

        #region Add_Grass

        _terGen._addGrass = EditorGUILayout.Toggle("Add Grass", _terGen._addGrass);
        if (_terGen._addGrass)
        {

            _terGen._grassMaxReliefSlope = EditorGUILayout.IntSlider("Max angel for grass gen", _terGen._grassMaxReliefSlope, 0, 90);
            _terGen.Grass = (Texture2D)EditorGUILayout.ObjectField("Grass texture", _terGen.Grass, typeof(Texture2D), false);
            EditorGUILayout.HelpBox("Distance at which details will no longer be drawn", MessageType.None);
            _terGen._grassDistance = EditorGUILayout.IntField("Distance", _terGen._grassDistance);

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
            if (_terGen._addTexture)
                Addtexturess(_terGen._terrainData);
            if (_terGen._addTrees)
                FillTreeInstances(_terGen._terrainOrigin.GetComponent<Terrain>());
            if (_terGen._addGrass)
                FillDetailMap(_terGen._terrainOrigin.GetComponent<Terrain>());

            if (_terGen._splitTerrain)
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
        if (_terGen._addTrees)
        {
            _terGen._treeData = new TreePrototype[_terGen.Trees.Length];
            for (int i = 0; i < _terGen.Trees.Length; i++)
            {
               
                _terGen._treeData[i] = new TreePrototype();
                _terGen._treeData[i].prefab = _terGen.Trees[i];

            }
        }
        if (_terGen._addTexture)
        {
           _terGen.terrainTexture[0] = new SplatPrototype();
           _terGen.terrainTexture[0].texture = _terGen.TerTexture;
        }



        if (_terGen._addGrass)
        {
            _terGen._detailData = new DetailPrototype[1];

            _terGen._detailData[0] = new DetailPrototype();
            _terGen._detailData[0].prototypeTexture = _terGen.Grass;
            _terGen._detailData[0].renderMode = DetailRenderMode.GrassBillboard;
        }

    }

    void FillTreeInstances(Terrain terrain)

    {
        if (!_terGen._addTrees) return;

        for (int x = 0; x < _terGen._terrainSizeData.z; x += _terGen._treeSpacing)
        {
            for (int z = 0; z < _terGen._terrainSizeData.z; z += _terGen._treeSpacing)
            {

                float unitx = 1.0f / (_terGen._terrainSizeData.x - 1);
                float unitz = 1.0f / (_terGen._terrainSizeData.z - 1);

                float offsetX = UnityEngine.Random.value * unitx * _terGen._treeSpacing;
                float offsetZ = UnityEngine.Random.value * unitz * _terGen._treeSpacing;

                float normX = x * unitx + offsetX;
                float normZ = z * unitz + offsetZ;

                float angle = terrain.terrainData.GetSteepness(normX, normZ);

                if (angle < _terGen._treesMaxReliefSlope)
                {

                    float ht = terrain.terrainData.GetInterpolatedHeight(normX, normZ);

                    if (ht < terrain.terrainData.size.y * 0.4f)
                    {

                        TreeInstance temp = new TreeInstance();
                        temp.position = new Vector3(normX, ht, normZ);
                        temp.prototypeIndex = (int)UnityEngine.Random.Range(0, _terGen.Trees.Length);
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
       
        if (!_terGen._addGrass) return;

        int[,] detailMap0 = new int[(int)terrain.terrainData.size.x, (int)terrain.terrainData.size.z];

        for (int x = 0; x < terrain.terrainData.size.x; x++)
        {
            for (int z = 0; z < terrain.terrainData.size.z; z++)
            {
                float unitx = 1.0f / (_terGen._terrainSizeData.x - 1);
                float unitz = 1.0f / (_terGen._terrainSizeData.z - 1);

                float offsetX = UnityEngine.Random.value * unitx;
                float offsetZ = UnityEngine.Random.value * unitz;

                float normX = x * unitx + offsetX;
                float normZ = z * unitz + offsetZ;

                float angle = terrain.terrainData.GetSteepness(normX, normZ);

                if (angle < _terGen._treesMaxReliefSlope)

                    detailMap0[z, x] = 1;
     

            }
        }

        terrain.detailObjectDistance = _terGen._grassDistance;
        terrain.terrainData.SetDetailResolution(512, 8);
        terrain.terrainData.SetDetailLayer(0, 0, 0, detailMap0);



    }

    public void Addtexturess(TerrainData terrainData)
    {
        if (!_terGen._addTexture) return;
        terrainData.splatPrototypes = _terGen.terrainTexture;

        //float[,,] map = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainTexture.Length];

        // terrainData.SetAlphamaps(0, 0, map);

        // // Splatmap data is stored internally as a 3d array of floats, so declare a new empty array ready for your custom splatmap data:
        // float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];
        // Debug.Log(terrainData.alphamapHeight);
        // for (int y = 0; y < terrainData.alphamapHeight; y++)
        // {
        //     for (int x = 0; x < terrainData.alphamapWidth; x++)
        //     {
        //         // Normalise x/y coordinates to range 0-1 
        //         float y_01 = (float)y / (float)terrainData.alphamapHeight;
        //         float x_01 = (float)x / (float)terrainData.alphamapWidth;

        //         // Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
        //         float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapHeight), Mathf.RoundToInt(x_01 * terrainData.heightmapWidth));

        //         // Calculate the normal of the terrain (note this is in normalised coordinates relative to the overall terrain dimensions)
        //         Vector3 normal = terrainData.GetInterpolatedNormal(y_01, x_01);

        //         // Calculate the steepness of the terrain
        //         float steepness = terrainData.GetSteepness(y_01, x_01);

        //         // Setup an array to record the mix of texture weights at this point
        //         float[] splatWeights = new float[terrainData.alphamapLayers];
        //         Debug.Log(terrainData.alphamapLayers);
        //         // CHANGE THE RULES BELOW TO SET THE WEIGHTS OF EACH TEXTURE ON WHATEVER RULES YOU WANT

        //         // Texture[0] has constant influence
        //         splatWeights[0] = 1;// Mathf.Clamp01((terrainData.heightmapHeight + height));

        //         // Texture[1] is stronger at lower altitudes
        //        // splatWeights[1] = Mathf.Clamp01((terrainData.heightmapHeight - height));

        //         //// Texture[2] stronger on flatter terrain
        //         //// Note "steepness" is unbounded, so we "normalise" it by dividing by the extent of heightmap height and scale factor
        //         //// Subtract result from 1.0 to give greater weighting to flat surfaces
        //         //splatWeights[2] = 1.0f - Mathf.Clamp01(steepness * steepness / (terrainData.heightmapHeight / 5.0f));

        //         //// Texture[3] increases with height but only on surfaces facing positive Z axis 
        //         //splatWeights[3] = height * Mathf.Clamp01(normal.z);

        //         // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
        //         float z = splatWeights.Sum();

        //         // Loop through each terrain texture
        //         for (int i = 0; i < terrainData.alphamapLayers; i++)
        //         {

        //             // Normalize so that sum of all texture weights = 1
        //             splatWeights[i] /= z;

        //             // Assign this point to the splatmap array
        //             splatmapData[x, y, i] = splatWeights[i];
        //         }
        //     }
        // }

        // // Finally assign the new splatmap to the terrainData:
        // terrainData.SetAlphamaps(0, 0, splatmapData);
    }
    #endregion

    #region Create terran from file

    public void CreateTerrain()
    {
        _terGen._hightMapRezaliton = GetTerrainRezalution(_terGen._splitCountID);
        _terGen._terrainData = new TerrainData
        {
            heightmapResolution = _terGen._hightMapRezaliton,
            size = _terGen._terrainSizeData,
            treePrototypes = _terGen._treeData,
            detailPrototypes = _terGen._detailData
        };

        LoadTerrain(_terGen._filePath, _terGen._terrainData);
        _terGen._terrainOrigin = Terrain.CreateTerrainGameObject(_terGen._terrainData);
        _terGen._terrainOrigin.transform.parent = _terGen.transform;

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

        _splitCount = _terGen._splitCountID + 2;

        Terrain _originalTerrain = _terGen._terrainOrigin.GetComponent<Terrain>();
        if (_originalTerrain == null) return;

        _terGen._step = _originalTerrain.terrainData.size.x / _splitCount;

        for (int x = 0; x < _splitCount; x++)
        {

            for (int z = 0; z < _splitCount; z++)
            {
                float xMin = _originalTerrain.terrainData.size.x / _splitCount * x;
                float xMax = _originalTerrain.terrainData.size.x / _splitCount * (x + 1);
                float zMin = _originalTerrain.terrainData.size.z / _splitCount * z;
                float zMax = _originalTerrain.terrainData.size.z / _splitCount * (z + 1);

                copyTerrain(_originalTerrain, string.Format("{0}{1}_{2}", _originalTerrain.name, x, z), xMin, xMax, zMin, zMax, _terGen._hightMapRezaliton, _terGen._terrainData.detailResolution, _terGen._terrainData.alphamapResolution);
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

        DestroyImmediate(_terGen._terrainOrigin);
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


