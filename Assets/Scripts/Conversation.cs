using System;
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

    public List<Tuple<string, string>> ConversationWithNames = new List<Tuple<string, string>>();

    private void OnEnable()
    {
        foreach(var line in conversation)
        {
            var tokens = line.Split(':');
            if(tokens.Length < 2)
            {
                Debug.LogWarning("Invalid dialogue line: " + line);
                continue;
            }

            string name = tokens[0].Trim(); ;
            string text = String.Join(":", tokens.Skip(1)); // in case there's more than one colon

            ConversationWithNames.Add(Tuple.Create(name, text));
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
