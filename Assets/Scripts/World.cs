using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class World : MonoBehaviour
{
    public static World Instance { get; private set; }

    Tilemap tilemap;

    private void Awake()
    {
        Instance = this;
        tilemap = GetComponent<Tilemap>();
    }
    
    public Vector3Int WorldToGridPos(Vector3 worldPos)
    {
        return tilemap.WorldToCell(worldPos);
    }
    public Vector3 GridToWorldPos(Vector3Int gridPos)
    {
        return tilemap.GetCellCenterWorld(gridPos);
    }

    public TileObject GetTile(Vector3Int gridPos)
    {
        var instantiatedGO = tilemap.GetInstantiatedObject(gridPos);
        if (instantiatedGO == null)
        {
            Debug.LogError("Tile without GameObject!");
            return null; 
        }

        var tileObject = instantiatedGO.GetComponent<TileObject>();
        if (tileObject == null)
            Debug.LogError("Instantiated tile without TileObject component!");

        return tileObject;
    }

    public bool IsSolid(Vector3Int gridPos)
    {
        var tile = tilemap.GetTile(gridPos) as WorldTile;
        if (tile == null)
            return true;
        return tile.Solid;
    }
}
