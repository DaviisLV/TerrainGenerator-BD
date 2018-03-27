using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Create Terrain"))
        {
            RemovePreviousTerrain();
            CreateTerrain();
        }

    }

    private TerrainGenerator _terGen;

    private void OnEnable()
    {
        _terGen = (TerrainGenerator)target;
    }

    private void CreateTerrain()
    {

    TerrainData _terrainData = new TerrainData();
      
    _terrainData.size = _terGen.TerrainSize;

    LoadTerrain("C:/Users/Davis/Documents/TerrainGenerator-BD/Assets/GISData/terrain1.raw", _terrainData);
    GameObject _terrain = Terrain.CreateTerrainGameObject(_terrainData);
    _terrain.transform.parent = _terGen.transform;

    }

    private void RemovePreviousTerrain()
    {
        foreach (Transform child in _terGen.transform)
        {
            GameObject.DestroyImmediate(child.gameObject);
        }
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
            Debug.Log("Data louded");
        }
    }
