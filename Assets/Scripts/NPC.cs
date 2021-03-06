﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : Actor
{
    [SerializeField]
    Conversation convoJustSal;
    [SerializeField]
    Conversation convoWithRook;
    [SerializeField]
    Conversation convoWithSmith;

    [SerializeField]
    string[] randomQuips;

    [SerializeField]
    bool disappearAfterConversation;
    [SerializeField]
    bool unlocksCharacter;
    [SerializeField]
    PlayerCharacter unlockedCharacter;

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
        MyTurn = false;
    }

    public void Interact()
    {
        Conversation convo = null;

        switch (Player.Instance.CharacterUnlockState)
        {
            case PlayerCharacter.Sal:
                convo = convoJustSal;
                break;
            case PlayerCharacter.Rook:
                convo = convoWithRook;
                break;
            case PlayerCharacter.Smith:
                convo = convoWithSmith;
                break;
        }

        if(convo == null)
        {
            string quip = "...";
            if (randomQuips != null && randomQuips.Length > 0)
            {
                // pick random quip to display
                int quipIndex = Random.Range(0, randomQuips.Length);
                quip = randomQuips[quipIndex];
            }  
            
            Textbox.Instance.Display(this, quip);
        }
        else
        {
            if (disappearAfterConversation)
            {
                Textbox.Instance.ConversationEndedEvent += () => Destroy(gameObject);
            }
            if(unlocksCharacter)
            {
                Textbox.Instance.ConversationEndedEvent += () => Player.Instance.UnlockCharacter(unlockedCharacter); 
            }
            Textbox.Instance.StartConversation(convo);
        }
    }
}
