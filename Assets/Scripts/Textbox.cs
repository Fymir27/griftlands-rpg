using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Textbox : MonoBehaviour
{
    public static Textbox Instance;

    public delegate void ConversationEndedCallback();
    public event ConversationEndedCallback ConversationEndedEvent;

    [SerializeField]
    Text characterName = null;
    [SerializeField]
    Text dialogue = null;
    [SerializeField]
    Conversation curConversation;
    [SerializeField]
    int conversationIndex;

    private void Awake()
    {
        Instance = this;
        var bg = GetComponent<Image>();
        if (bg)
            bg.enabled = true;
        gameObject.SetActive(false);
    }

    public void Display(Actor actor, string dialogueContent)
    {
        // TODO: big portrait?
        characterName.text = actor.Name;
        dialogue.text = dialogueContent;
        gameObject.SetActive(true);
    }

    public void Display(WorldObject thing, string dialogueContent)
    {
        characterName.text = thing.Name;
        dialogue.text = dialogueContent;
        gameObject.SetActive(true);
    }

    public void StartConversation(Conversation convo)
    {
        curConversation = convo;
        conversationIndex = -1;
        AdvanceDialogue();
    }

    /**
     * returns true when dialogue has ended
     */
    public bool AdvanceDialogue()
    {   
        if(curConversation != null)
        {
            if(++conversationIndex < curConversation.ConversationWithNames.Count)
            {
                gameObject.SetActive(true);
                var line = curConversation.ConversationWithNames[conversationIndex];
                characterName.text = line.Item1;
                dialogue.text = line.Item2;
                return false;
            }
        }

        curConversation = null;
        gameObject.SetActive(false);
        ConversationEndedEvent?.Invoke();
        ConversationEndedEvent = null;
        return true;
    }

    public void AbortDialogue()
    {
        curConversation = null;
        gameObject.SetActive(false);
    }
}
