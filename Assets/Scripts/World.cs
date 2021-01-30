using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class World : MonoBehaviour
{
    public static World Instance { get; private set; }

    Tilemap tilemap;
    Dictionary<Vector3Int, Actor> actors = new Dictionary<Vector3Int, Actor>();

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
  
    public bool IsSolid(Vector3Int gridPos)
    {
        var tile = tilemap.GetTile(gridPos) as WorldTile;
        if (tile == null)
            return true;
        return tile.Solid;
    }

    public void MoveActorTo(Actor actor, Vector3Int gridPos)
    {
        if(actors.ContainsKey(gridPos))
        {
            Debug.LogError(gridPos + " is already occupied! Failed to move actor " + actor.name);
            return;
        }
        // remove actor from old position
        if(actors.ContainsKey(actor.GridPos))
        {
            actors.Remove(actor.GridPos);
        }
        actors[gridPos] = actor;

        var spriteRenderer = actor.gameObject.GetComponent<SpriteRenderer>();
        if(spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 100 - gridPos.y;
        }
    }

    public void RemoveActor(Actor actor)
    {
        actors.Remove(actor.GridPos);
    }

    public Actor GetActor(Vector3Int gridPos)
    {
        actors.TryGetValue(gridPos, out Actor a);
        return a;
    }

    IEnumerator DelayedAction(float seconds, System.Action callback)
    {
        yield return new WaitForSeconds(seconds);
        callback();
    }
    public void SetTimeout(float seconds, System.Action callback)
    {
        StartCoroutine(DelayedAction(seconds, callback));
    }
}
