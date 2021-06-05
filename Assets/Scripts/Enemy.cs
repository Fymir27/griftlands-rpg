using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : Actor
{
    [Header("Enemy Properties")]
    [SerializeField]
    int aggroRange = 0;
    [SerializeField]
    int trackRange = 0;
    [SerializeField]
    float walkingSpeed;

    [Header("Live Info")]
    [SerializeField]
    bool aggroed = false;
    [SerializeField]
    bool walking;
    [SerializeField]
    Vector3 walkingTo;
    [SerializeField]
    bool dying = false;
     
    // Start is called before the first frame update
    void Start()
    {
        InitActor();
        TurnManager.Instance.EnqueueActor(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (hpText != null)
            hpText.text = $"HP: {CurHealth}/{MaxHealth}";

        if (characterNameText != null)
            characterNameText.text = Name;

        if (dying)
            return;

        if (CurHealth <= 0)
        {
            dying = true;
            Die();
            return;
        }       

        if(walking)
        {
            if (walkingSpeed > 0)
                transform.position = Vector3.MoveTowards(transform.position, walkingTo, walkingSpeed * Time.deltaTime);
            else
                transform.position = walkingTo;

            if(transform.position == walkingTo)
            {
                walking = false;
                MyTurn = false;
            }
        }
    }

    public override float GetWalkingSpeed()
    {
        return walkingSpeed;
    }

    public override void TakeTurn()
    {
        if (dying)
        {
            MyTurn = false;
            return;
        }

        MyTurn = true;
        //Debug.Log("Enemy TakeTurn(): " + Name);

        var nextStep = NextStepTowards(actor => actor == Player.Instance, aggroed ? trackRange : aggroRange);
        var relStep = nextStep - GridPos;

        var world = World.Instance;

        if(nextStep != GridPos)
        {
            aggroed = true;
            if(world.GetActor(nextStep) == Player.Instance)
            {
                if (animator != null)
                    animator.TriggerAnimation(ActorAnimationState.Attack, relStep);
                Player.Instance.CurHealth -= AttackDamage;               
                MyTurn = false;
            }
            else
            {                
                if (animator != null)
                    animator.TriggerAnimation(ActorAnimationState.Walk, relStep);
                               
                World.Instance.MoveActorTo(this, nextStep);
                walking = true;
                walkingTo = World.Instance.GridToWorldPos(nextStep);
                GridPos = nextStep;
            }           
        }
        else
        {
            aggroed = false;
            MyTurn = false;
        }
    }

    public void ApplyDamage(int amount)
    {
        aggroed = true;
        CurHealth = Mathf.Clamp(CurHealth - amount, 0, MaxHealth);
    }

    public void ApplyHealing(int amount)
    {
        CurHealth = Mathf.Clamp(CurHealth + amount, 0, MaxHealth);
    }
}
