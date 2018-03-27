using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneratorRT : MonoBehaviour {
    [Header("X = Width, Y = Height, Z = Length")]
    public Vector3Int TerrainSize;
    TerrainData _terrainData;
 

    void Start () {
        _terrainData = new TerrainData();
        AddComponents();
	}
	
    private void AddComponents()
    {  
        _terrainData.size = TerrainSize;

        LoadTerrain("C:/Users/Davis/Documents/TerrainGenerator-BD/Assets/GISData/terrain1.raw", _terrainData);
        GameObject _terrain = Terrain.CreateTerrainGameObject(_terrainData);
        _terrain.transform.parent = transform;
      
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
