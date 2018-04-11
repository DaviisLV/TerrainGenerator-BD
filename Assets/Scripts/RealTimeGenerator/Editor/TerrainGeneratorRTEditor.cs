using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

[CustomEditor(typeof(TerrainGeneratorRT))]
public class TerrainGeneratorRTEditor : Editor
{

    private TerrainGeneratorRT _terGen;
    private string[] _choices = new[] { "4", "9", "16", "25", "36", "49", "64", "81", "100", "121", "144", "169", "196", "225" };
    private string[] _resalution = new[] { "33×33", "65×65", "129×129", "257×257", "513×513", "1025×1025", "2049×2049", "4097×4097" };
    private int _step;
    private bool ttn;
    private object _textura;

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
            _terGen._splitCountID = EditorGUILayout.Popup("Number of pieces", _terGen._splitCountID, _choices);

            if (_terGen._terrainSizeData.x <= _terGen._terrainSizeData.z)
                _step = _terGen._terrainSizeData.z / (_terGen._splitCountID + 2);
            else
                _step = _terGen._terrainSizeData.x / (_terGen._splitCountID + 2);

            _terGen._radiusOfGeneration = EditorGUILayout.IntSlider("Radius of generation:", _terGen._radiusOfGeneration, _step, _step * (_terGen._splitCountID + 2));

        }

        _terGen._addTexture = EditorGUILayout.Toggle("Add Texture", _terGen._addTexture);

        if (_terGen._addTexture)
        {

          //  _terGen._textureCount = EditorGUILayout.IntField("texture count", _terGen._textureCount);
            //for (int i = 0; i > _terGen._textureCount; i++)
            //{
            //    _terGen.TerTexture[i] = (Texture2D)EditorGUILayout.ObjectField("Texture", _terGen.TerTexture[i], typeof(Texture2D), false);
            //}

          //  _terGen.TerTexture[0] = (Texture2D) EditorGUILayout.ObjectField("Texture", _terGen.TerTexture[0], typeof(Texture2D), false);
            _terGen.TerTexture = (Texture2D)EditorGUILayout.ObjectField("Texture", _terGen.TerTexture, typeof(Texture2D), false);
        }
    }



    private void OnEnable()
    {
        _terGen = (TerrainGeneratorRT)target;
    }


}
