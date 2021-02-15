using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Actor : MonoBehaviour
{
    [Header("Actor Properties")]
    public bool MyTurn;
    public string Name;
    public Vector3Int GridPos;
    public int CurHealth;
    public int MaxHealth;
    public int AttackDamage;

    [Header("Actor References")]
    [SerializeField]
    protected Text hpText = null;
    [SerializeField]
    protected Text characterNameText = null;
    [SerializeField]
    protected ActorAnimator animator;

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

    protected Vector3Int NextStepTowards(Func<Actor, bool> criterium, int range)
    {
        var prev = new Dictionary<Vector3Int, Vector3Int>()
        {
            { GridPos, GridPos }
        };

        /*
        var distances = new Dictionary<Vector3Int, int>()
        {
            { GridPos, 0 }
        };
        */

        var queue = new Queue<Vector3Int>();
        queue.Enqueue(GridPos);

        Vector3Int target = GridPos;
        while(queue.Count > 0)
        {
            var curPos = queue.Dequeue();
                        
            foreach(var neighbourPos in curPos.Neighbours())
            {
                // ignore solid and visited tiles
                if (World.Instance.IsSolid(neighbourPos) || prev.ContainsKey(neighbourPos))
                    continue;

                prev[neighbourPos] = curPos;
                //distances[neighbourPos] = distances[curPos] + 1;                

                var neighbouringActor = World.Instance.GetActor(neighbourPos);
                if (neighbouringActor == null && (neighbourPos - GridPos).magnitude <= range)
                {
                    queue.Enqueue(neighbourPos);
                }
                if (criterium(neighbouringActor))
                {
                    Debug.Log("Target found! " + neighbourPos);
                    target = neighbourPos;
                    break;
                }               
            }            
        }

        // nothing found
        if(target == GridPos)
        {
            return GridPos;
        }

        Vector3Int nextStep = target;
        while (prev[nextStep] != GridPos)
            nextStep = prev[nextStep];

        return nextStep;
    }

    public virtual void Die()
    {
        animator.TriggerAnimation(ActorAnimationState.Death);
        animator.OnAnimationComplete += () => Destroy(gameObject);
    }

    private void OnDestroy()
    {
        TurnManager.Instance.RemoveActor(this);
        World.Instance.RemoveActor(this);
    }
}
