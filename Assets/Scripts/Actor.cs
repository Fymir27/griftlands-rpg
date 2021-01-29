using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    [Header("Actor Properties")]
    public string Name;
    public Vector3Int GridPos;
    public int CurHealth;
    public int MaxHealth;

    /**
     * This function should always be called before anyhting 
     * to initialize the actor on the grid correctly
     */
    protected void InitActor()
    {
        var world = World.Instance;
        GridPos = world.WorldToGridPos(transform.position);
        // snap to grid
        transform.position = world.GridToWorldPos(GridPos);

        world.MoveActorTo(this, GridPos);

        CurHealth = MaxHealth;
    }
}
