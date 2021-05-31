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
    public ActorAnimationSet AnimationSet;

    [SerializeField, Range(1, 30)]
    int sampleRate;    

    [SerializeField]
    ActorAnimationState animationState;

    [SerializeField]
    CardinalDirection direction;

    [SerializeField]
    float secondsUntilNextFrame;

    [SerializeField]
    int frameIndex;

    SpriteRenderer spriteRenderer;

    public Action OnAnimationComplete = null;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();         
    }

    // Update is called once per frame
    void Update()
    {
        secondsUntilNextFrame -= Time.deltaTime;
        if(secondsUntilNextFrame <= 0)
       {
            var anim = AnimationSet.Sprites[animationState][direction];

            // TODO: do we want this?
            if (frameIndex >= anim.Length)
            {
                if (OnAnimationComplete != null)
                {
                    OnAnimationComplete();
                    OnAnimationComplete = null;
                }

                if (animationState == ActorAnimationState.Death)
                {
                    // stay on last frame of death animation
                    frameIndex = anim.Length - 1;
                }
                else
                {
                    animationState = ActorAnimationState.Idle;
                    frameIndex = 0;
                }
            }
           
            spriteRenderer.sprite = anim[frameIndex++];
            secondsUntilNextFrame = 1f / sampleRate;
        }
    }

    public void TriggerAnimation(ActorAnimationState animation, CardinalDirection dir)
    {
        if(!AnimationSet.Sprites.ContainsKey(animation) || !AnimationSet.Sprites[animation].ContainsKey(dir))
        {
            Debug.LogError("No sprites found for animation: " + animation);
            return;
        }    

        if(AnimationSet.Sprites[animation][dir].Length == 0)
        {
            Debug.LogWarning("No sprites found for animation: " + animation);
            return;
        }

        animationState = animation;
        direction = dir;
        frameIndex = 0;
        secondsUntilNextFrame = 0;
    }

    public void TriggerAnimation(ActorAnimationState animation)
    {
        var defaultDir = AnimationSet.GetInitialDirection();
        TriggerAnimation(animation, defaultDir);
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
