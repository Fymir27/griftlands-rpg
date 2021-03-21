using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class World : MonoBehaviour
{
    [SerializeField]
    WorldTile floorTile;
    public static World Instance { get; private set; }

    Tilemap tilemap;
    Dictionary<Vector3Int, Actor> actors = new Dictionary<Vector3Int, Actor>();
    Dictionary<Vector3Int, WorldObject> objects = new Dictionary<Vector3Int, WorldObject>();
    Dictionary<Vector3Int, Conversation> conversations = new Dictionary<Vector3Int, Conversation>();
    Dictionary<Vector3Int, string> sceneTriggers = new Dictionary<Vector3Int, string>();

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
  
    public Vector3Int MouseGridPos()
    {
        var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return WorldToGridPos(mouseWorldPos);
    }

    public bool IsSolid(Vector3Int gridPos)
    {
        var tile = tilemap.GetTile(gridPos) as WorldTile;       
        if (tile == null)
            return true;
        var thing = GetObject(gridPos);       
        return tile.Solid || (thing != null && thing.Solid);
    }

    public bool IsBreakable(Vector3Int gridPos)
    {
        var tile = tilemap.GetTile(gridPos) as WorldTile;
        if (tile == null)
            return true;
        return tile.Breakable;
    }

    public bool IsJumpable(Vector3Int gridPos)
    {
        var tile = tilemap.GetTile(gridPos) as WorldTile;
        if (tile == null)
            return true;
        return tile.Jumpable;
    }

    public void BreakTile(Vector3Int gridPos)
    {
        var tile = tilemap.GetTile(gridPos) as WorldTile;
        if (tile == null)
        {
            Debug.LogError("Trying to break nonexistent tile! " + gridPos);
            return;
        }
        if(!tile.Breakable)
        {
            Debug.LogError("Trying to break non breakable tile! " + gridPos);
            return;
        }
        tilemap.SetTile(gridPos, floorTile);
    }

    public void SetSceneTrigger(string sceneName, Vector3Int gridPos)
    {
        if(sceneTriggers.ContainsKey(gridPos))
        {
            Debug.LogError("Two scene triggers on same tile! " + gridPos);
            return;
        }
        sceneTriggers[gridPos] = sceneName;
    }

    public void SetConversation(Conversation convo, Vector3Int gridPos)
    {
        if(conversations.ContainsKey(gridPos))
        {
            Debug.LogWarning("Overwriting conversation at " + gridPos);
        }
        conversations[gridPos] = convo;
    }

    public Conversation GetConversation(Vector3Int gridPos)
    {
        conversations.TryGetValue(gridPos, out Conversation conversation);
        return conversation;
    }

    public void SetObject(WorldObject thing, Vector3Int gridPos)
    {
        if (objects.ContainsKey(gridPos))
        {
            Debug.LogError(gridPos + " is already occupied! Failed to set object " + thing.Name);
            return;
        }
        objects[gridPos] = thing;
    }

    public WorldObject GetObject(Vector3Int gridPos)
    {
        objects.TryGetValue(gridPos, out WorldObject thing);
        return thing;
    }

    public void MoveActorTo(Actor actor, Vector3Int gridPos)
    {
        if(actors.ContainsKey(gridPos))
        {
            Debug.LogError(gridPos + " is already occupied! Failed to move actor " + actor.Name);
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
            spriteRenderer.sortingOrder = -gridPos.y;
        }

        TriggerStepOn(actor, gridPos);
    }

    public void TriggerStepOn(Actor actor, Vector3Int gridPos)
    {
        var tile = tilemap.GetTile(gridPos) as WorldTile;
        if (tile != null)
        {
            actor.CurHealth = Mathf.Clamp(actor.CurHealth + tile.HpChangeStepOn, 0, actor.MaxHealth);
        }

        if(actor == Player.Instance && sceneTriggers.ContainsKey(gridPos))
        {
            var overlay = OverlayImage.Instance;
            if(overlay)
            {
                overlay.SetColor(Color.black);
                overlay.FadeIn(1f);
                SetTimeout(1f, () => SceneManager.LoadScene(sceneTriggers[gridPos]));
            }
            else
            {
                SceneManager.LoadScene(sceneTriggers[gridPos]);
            }            
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

    /**
     * Bresenhams line drawing algorithm from wikipedia
     */
    public List<Vector3Int> LineOfSight(Vector3Int from, Vector3Int to, int range = int.MaxValue, bool stopAtSolid = true, bool stopAtJumpable = true)
    {
        Vector3Int delta = to - from;
        int distX = Mathf.Abs(delta.x);
        int distY = Mathf.Abs(delta.y);
        int signX = delta.x < 0 ? -1 : 1;
        int signY = delta.y < 0 ? -1 : 1;

        Vector3Int parallelStep;
        Vector3Int diagonalStep = new Vector3Int(signX, signY, 0);
        int deltaFast;
        int deltaSlow;

        if(distX > distY)
        {
            parallelStep = new Vector3Int(signX, 0, 0);            
            deltaFast = distX;
            deltaSlow = distY;
        }
        else
        {
            parallelStep = new Vector3Int(0, signY, 0);
            deltaFast = distY;
            deltaSlow = distX;
        }

        var curPos = from;
        var result = new List<Vector3Int>() { from };
        int error = deltaFast / 2;

        for (int coordFast = 0; coordFast < deltaFast; coordFast++)
        {
            error -= deltaSlow;
            if(error < 0)
            {
                error += deltaFast;
                curPos += diagonalStep;
            }
            else
            {
                curPos += parallelStep;
            }

            if (stopAtSolid && IsSolid(curPos))
            {
                if (stopAtJumpable || !IsJumpable(curPos))
                {
                    break;
                }
            }

            if ((curPos - from).magnitude > range)
                break;

            result.Add(curPos);
        }

        return result;
    }
}
