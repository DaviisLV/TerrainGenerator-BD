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
    private GameObject[] _treesArr;

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
            _terGen._SplitCountID = EditorGUILayout.Popup("Number of pieces", _terGen._SplitCountID, _choices);

            if (_terGen._TerrainSizeData.x <= _terGen._TerrainSizeData.z)
                _step = _terGen._TerrainSizeData.z / (_terGen._SplitCountID + 2);
            else
                _step = _terGen._TerrainSizeData.x / (_terGen._SplitCountID + 2);

            _terGen._RadiusOfGeneration = EditorGUILayout.IntSlider("Radius of generation:", _terGen._RadiusOfGeneration, _step, _step * (_terGen._SplitCountID + 2));

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
            _terGen._TreeSpacing = EditorGUILayout.IntSlider("Trees spacing", _terGen._TreeSpacing, 1, _terGen._TerrainSizeData.x / 2);
            _terGen._TreesMaxReliefSlope = EditorGUILayout.IntSlider("Max anngel for tree gen", _terGen._TreesMaxReliefSlope, 0, 90);
            _terGen._TreesPrefabCount = EditorGUILayout.IntSlider("Trees prefab count", _terGen._TreesPrefabCount, 1, 24);

            if (_terGen._TreesPrefabCount != _terGen._Trees.Length)
                EditorGUILayout.HelpBox("Press button to recauclate Trees prefab count ", MessageType.Info);

            if (GUILayout.Button("Recauculate prefab count", GUILayout.ExpandWidth(false)))
            {

                if (_terGen._TreesPrefabCount == _terGen._Trees.Length) return;
                _treesArr = new GameObject[_terGen._TreesPrefabCount];
                _terGen._Trees = _treesArr;


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

    }

    private void OnEnable()
    {
        _terGen = (TerrainGeneratorRT)target;
    }

}
