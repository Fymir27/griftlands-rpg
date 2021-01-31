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
    bool aggroed = false;

    // Start is called before the first frame update
    void Start()
    {
        InitActor();
        TurnManager.Instance.EnqueueActor(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (CurHealth <= 0)
        {            
            Destroy(gameObject);
        }

        if (hpText != null)
            hpText.text = $"HP: {CurHealth}/{MaxHealth}";

        if (characterNameText != null)
            characterNameText.text = Name;
    }

    public override void TakeTurn()
    {
        MyTurn = true;
        Debug.Log("Enemy TakeTurn(): " + Name);

        var nextStep = NextStepTowards(actor => actor == Player.Instance, aggroed ? trackRange : aggroRange);

        var world = World.Instance;

        if(nextStep != GridPos)
        {
            aggroed = true;
            if(world.GetActor(nextStep) == Player.Instance)
            {
                Player.Instance.CurHealth -= AttackDamage;
            }
            else
            {
                World.Instance.MoveActorTo(this, nextStep);
                GridPos = nextStep;

                // TODO: anmiation
                transform.position = World.Instance.GridToWorldPos(nextStep);
            }           
        }
        else
        {
            aggroed = false;
        }
        
       
        World.Instance.SetTimeout(.1f, () => MyTurn = false);
    }
}
