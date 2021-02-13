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
    public bool Breakable;
    public bool Jumpable;
    public int HpChangeStepOn;

#if UNITY_EDITOR
    [MenuItem("Assets/Create/WorldTile", false, 10)]
    public static void CreateWorldTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save World Tile", "New World Tile", "Asset", "Save World Tile", "Assets/Tiles");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<WorldTile>(), path);
    }
#endif
}
