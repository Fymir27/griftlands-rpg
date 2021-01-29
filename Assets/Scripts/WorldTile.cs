using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WorldTile : Tile
{
    public bool Solid;

#if UNITY_EDITOR
    // The following is a helper that adds a menu item to create a RoadTile Asset
    [MenuItem("Assets/Create/WorldTile", false, 10)]
    public static void CreateRoadTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save World Tile", "New World Tile", "Asset", "Save World Tile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<WorldTile>(), path);
    }
#endif
}
