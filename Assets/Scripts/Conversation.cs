﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Conversation : ScriptableObject
{
    [SerializeField]
    string[] conversation;

    private List<Tuple<string, string>> conversationWithNames = new List<Tuple<string, string>>();

    // Lazy init property because Awake() and OnEnable() aren't called reliably
    // during Play Mode in Editor
    public List<Tuple<string, string>> ConversationWithNames { 
        get  
        {
            if (conversationWithNames.Count == 0)
            {
                Init();                
            } 
            return conversationWithNames;
        }         
    }

    private void Init()
    {
        if (conversation.Length == 0)
        {
            conversationWithNames.Add(Tuple.Create("", $"MISSING DIALOGUE {name}"));
            return;
        }

        foreach (var line in conversation)
        {
            var tokens = line.Split(':');
            if (tokens.Length == 1)
            {
                conversationWithNames.Add(Tuple.Create("", line));
            }
            else
            {
                string name = tokens[0].Trim(); ;
                string text = String.Join(":", tokens.Skip(1)); // in case there's more than one colon
                conversationWithNames.Add(Tuple.Create(name, text));
            }
        }     
    }

    private void OnEnable()
    {
        if (conversation.Length > 0 && conversation[0] == "DEBUG")
        {
            foreach (var line in conversation)
            {
                Debug.Log(line);
            }
        }       
    }

    private void OnValidate()
    {   
        for (int i = 0; i < conversation.Length; i++)
        {
            // make sure line doesn't start or end with ":" as that breaks the asset file (YAML) because unity doesn't escape it
            conversation[i] = System.Text.RegularExpressions.Regex.Replace(conversation[i], @"(^[\s:]*)|([:\s]*$)", "");
        }
    }

#if UNITY_EDITOR
    [MenuItem("Assets/Create/Conversation", false, 10)]
    public static void CreateConversation()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Conversation", "New Conversation", "Asset", "Save Conversation", "Assets/Conversations");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<Conversation>(), path);
    }
#endif
}
