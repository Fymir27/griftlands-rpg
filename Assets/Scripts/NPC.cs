using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : Actor
{
    // Start is called before the first frame update
    void Start()
    {
        InitActor();
        TurnManager.Instance.EnqueueActor(this);
    }

    private void Update()
    {
        if (hpText != null)
            hpText.text = $"HP: {CurHealth}/{MaxHealth}";
    }

    public override void TakeTurn()
    {
        MyTurn = true;
        Debug.Log("NPC TakeTurn(): " + Name);
        World.Instance.SetTimeout(.1f, () => MyTurn = false);
    }
}
