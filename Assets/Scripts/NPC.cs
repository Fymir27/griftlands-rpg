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

        if (characterNameText != null)
            characterNameText.text = Name;
    }

    public override void TakeTurn()
    {
        MyTurn = true;
        Debug.Log("NPC TakeTurn(): " + Name);
        World.Instance.SetTimeout(.1f, () => MyTurn = false);
    }

    public void Interact()
    {
        // TODO: dialogue system
        Textbox.Instance.Display(this, "This is placeholder dialogue!");
    }
}
