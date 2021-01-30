using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Actor : MonoBehaviour
{
    [Header("Actor Properties")]
    public bool MyTurn;
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

    /** 
     * implementations of this should set "MyTurn" to true while
     * taking their turn and set it back to false once done
     */    
    public abstract void TakeTurn();
}
