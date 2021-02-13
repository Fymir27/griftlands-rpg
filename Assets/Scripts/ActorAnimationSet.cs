﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ActorAnimationSet : ScriptableObject
{
    [SerializeField]
    CardinalDirection[] ordering;

    [SerializeField]
    Sprite[] animIdle;

    [SerializeField]
    Sprite[] animWalk;

    [SerializeField]
    Sprite[] animAttack;

    [SerializeField]
    Sprite[] animDeath;
    
    public Dictionary<ActorAnimationState, Dictionary<CardinalDirection, Sprite[]>> Sprites;

    private void OnEnable()
    {
        var animsUnsplit = new Dictionary<ActorAnimationState, Sprite[]>()
        {
            { ActorAnimationState.Idle, animIdle },
            { ActorAnimationState.Walk, animWalk },
            { ActorAnimationState.Attack, animAttack },
            { ActorAnimationState.Death, animDeath }
        };

        Sprites = new Dictionary<ActorAnimationState, Dictionary<CardinalDirection, Sprite[]>>();
        foreach (var animState in animsUnsplit.Keys)
        {
            Sprites[animState] = new Dictionary<CardinalDirection, Sprite[]>();
            int offset = 0;
            foreach (var dir in ordering)
            {
                int frameCount = animsUnsplit[animState].Length / 4;
                Sprites[animState][dir] = animsUnsplit[animState].Skip(offset).Take(frameCount).ToArray();
                offset += frameCount;
            }
        }
    }

#if UNITY_EDITOR
    [MenuItem("Assets/Create/ActorAnimationSet", false, 9)]
    public static void Create()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Animation Set", "New Animation Set", "Asset", "Save Animation Set", "Assets/Animations");
        if (path == "")
            return;
        var newAnimationSet = ScriptableObject.CreateInstance<ActorAnimationSet>();            
        AssetDatabase.CreateAsset(newAnimationSet, path);
    }
    
#endif
}
