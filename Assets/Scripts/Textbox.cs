using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Textbox : MonoBehaviour
{
    public static Textbox Instance;

    [SerializeField]
    Text characterName;
    [SerializeField]
    Text dialogue;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Display(Actor actor, string dialogueContent)
    {
        characterName.text = actor.Name;
        dialogue.text = dialogueContent;
        gameObject.SetActive(true);
    }

    /**
     * returns true when dialogue has ended
     */
    public bool AdvanceDialogue()
    {        
        gameObject.SetActive(false);
        return true;
    }
}
