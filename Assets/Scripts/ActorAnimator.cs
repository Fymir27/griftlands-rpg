using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

public enum CardinalDirection
{
    North,
    West,
    South,
    East
}

public enum ActorAnimationState
{
    Idle,
    Walk,
    Attack,
    Death
}

public class ActorAnimator : MonoBehaviour
{
    [Header("State Info")]

    [SerializeField, Range(1, 30)]
    int sampleRate;
    [SerializeField]
    CardinalDirection[] ordering;

    [SerializeField]
    ActorAnimationState animationState;

    [SerializeField]
    CardinalDirection direction;

    [SerializeField]
    float secondsUntilNextFrame;

    [SerializeField]
    int frameIndex;

    [Header("Spritesheet")]

    [SerializeField]
    Sprite[] animIdle;

    [SerializeField]
    Sprite[] animWalk;

    [SerializeField]
    Sprite[] animAttack;

    [SerializeField]
    Sprite[] animDeath;

    Dictionary<ActorAnimationState, Dictionary<CardinalDirection, Sprite[]>> sprites;

    SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        var animsUnsplit = new Dictionary<ActorAnimationState, Sprite[]>()
        {
            { ActorAnimationState.Idle, animIdle },
            { ActorAnimationState.Walk, animWalk },
            { ActorAnimationState.Attack, animAttack },
            { ActorAnimationState.Death, animDeath }
        };

        sprites = new Dictionary<ActorAnimationState, Dictionary<CardinalDirection, Sprite[]>>();
        foreach (var animState in animsUnsplit.Keys)
        {
            sprites[animState] = new Dictionary<CardinalDirection, Sprite[]>();
            int offset = 0;
            foreach (var dir in ordering)
            {                   
                int frameCount = animsUnsplit[animState].Length / 4;
                sprites[animState][dir] = animsUnsplit[animState].Skip(offset).Take(frameCount).ToArray();
                offset += frameCount;
            }
        }        
    }

    // Update is called once per frame
    void Update()
    {
        secondsUntilNextFrame -= Time.deltaTime;
        if(secondsUntilNextFrame <= 0)
        {
            var anim = sprites[animationState][direction];            
            spriteRenderer.sprite = anim[frameIndex];
            frameIndex = (frameIndex + 1) % anim.Length;

            // TODO: do we want this?
            if (frameIndex == 0)
            {
                if (animationState == ActorAnimationState.Death)
                {
                    // stay on last frame of death animation
                    frameIndex = anim.Length - 1;
                }
                else
                {
                    animationState = ActorAnimationState.Idle;
                }
            }

            secondsUntilNextFrame = 1f / sampleRate;
        }
    }

    public void TriggerAnimation(ActorAnimationState animation, CardinalDirection dir)
    {
        animationState = animation;
        direction = dir;
        frameIndex = 0;
        secondsUntilNextFrame = 0;
    }

    public void TriggerAnimation(ActorAnimationState animation, Vector3Int vectorDir)
    {
        CardinalDirection dir;
        if (vectorDir.x > 0)
            dir = CardinalDirection.East;
        else if (vectorDir.x < 0)
            dir = CardinalDirection.West;
        else if (vectorDir.y > 0)
            dir = CardinalDirection.North;
        else
            dir = CardinalDirection.South;

        TriggerAnimation(animation, dir);
    }
}
