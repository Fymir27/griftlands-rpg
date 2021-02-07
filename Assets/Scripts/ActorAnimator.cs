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
    [SerializeField, Range(1, 30)]
    int sampleRate;

    [SerializeField]
    Sprite[] spritesheet;

    [Header("State Info")]

    [SerializeField]
    ActorAnimationState animationState;

    [SerializeField]
    CardinalDirection direction;

    [SerializeField]
    float secondsUntilNextFrame;

    [SerializeField]
    int frameIndex;

    [Header("Spritesheet Definition")]

    [SerializeField]
    int offsetIdle;
    [SerializeField]
    int frameCountIdle;

    [SerializeField]
    int offsetWalk;
    [SerializeField]
    int frameCountWalk;

    [SerializeField]
    int offsetAttack;
    [SerializeField]
    int frameCountAttack;

    [SerializeField]
    int offsetDeath;
    [SerializeField]
    int frameCountDeath;

    Dictionary<ActorAnimationState, Dictionary<CardinalDirection, Sprite[]>> sprites;
    Dictionary<ActorAnimationState, int> frameCounts;

    SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        sprites = new Dictionary<ActorAnimationState, Dictionary<CardinalDirection, Sprite[]>>()
        {
            { ActorAnimationState.Idle, new Dictionary<CardinalDirection, Sprite[]>() },
            { ActorAnimationState.Walk, new Dictionary<CardinalDirection, Sprite[]>() },
            { ActorAnimationState.Attack, new Dictionary<CardinalDirection, Sprite[]>() },
            { ActorAnimationState.Death, new Dictionary<CardinalDirection, Sprite[]>() },
        };

        frameCounts = new Dictionary<ActorAnimationState, int>()
        {

            { ActorAnimationState.Idle, frameCountIdle },
            { ActorAnimationState.Walk, frameCountWalk },
            { ActorAnimationState.Attack, frameCountAttack },
            { ActorAnimationState.Death, frameCountDeath }
        };
        
        for (int dirIndex = 0; dirIndex < 4; dirIndex++)
        {
            var dir = (CardinalDirection)dirIndex;

            var animIdle = new Sprite[frameCountIdle];
            for (int frame = 0; frame < frameCountIdle; frame++)
            {
                animIdle[frame] = spritesheet[offsetIdle + dirIndex * frameCountIdle + frame];
            }
            sprites[ActorAnimationState.Idle][dir] = animIdle;

            var animWalk = new Sprite[frameCountWalk];
            for (int frame = 0; frame < frameCountWalk; frame++)
            {
                animWalk[frame] = spritesheet[offsetWalk + dirIndex * frameCountWalk + frame];
            }
            sprites[ActorAnimationState.Walk][dir] = animWalk;

            var animAttack = new Sprite[frameCountAttack];
            for (int frame = 0; frame < frameCountAttack; frame++)
            {
                animAttack[frame] = spritesheet[offsetAttack + dirIndex * frameCountAttack + frame];
            }
            sprites[ActorAnimationState.Attack][dir] = animAttack;

            var animDeath = new Sprite[frameCountDeath];
            for (int frame = 0; frame < frameCountDeath; frame++)
            {
                animDeath[frame] = spritesheet[offsetDeath + dirIndex * frameCountDeath + frame];
            }
            sprites[ActorAnimationState.Death][dir] = animDeath;
        }
    }

    // Update is called once per frame
    void Update()
    {
        secondsUntilNextFrame -= Time.deltaTime;
        if(secondsUntilNextFrame <= 0)
        {
            Sprite newFrame = sprites[animationState][direction][frameIndex];
            spriteRenderer.sprite = newFrame;
            frameIndex = (frameIndex + 1) % frameCounts[animationState];

            // TODO: do we want this?
            if (frameIndex == 0)
            {
                if (animationState == ActorAnimationState.Death)
                {
                    // stay on last frame of death animation
                    frameIndex = frameCounts[animationState] - 1;
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
    }
}
