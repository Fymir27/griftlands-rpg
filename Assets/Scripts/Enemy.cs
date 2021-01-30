using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : Actor
{
    [SerializeField]
    Text hpText = null;

    // Start is called before the first frame update
    void Start()
    {
        InitActor();
        TurnManager.Instance.EnqueueActor(this);
    }

    // Update is called once per frame
    void Update()
    {
        if(hpText != null)
            hpText.text = $"HP: {CurHealth}/{MaxHealth}";
    }

    public override void TakeTurn()
    {
        MyTurn = true;
        Debug.Log("Enemy TakeTurn(): " + Name);
        World.Instance.SetTimeout(.5f, () => MyTurn = false);
    }
}
