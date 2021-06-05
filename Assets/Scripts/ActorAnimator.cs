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

[RequireComponent(typeof(Actor))]
public class ActorAnimator : MonoBehaviour
{
    public Action OnAnimationComplete = null;
    public ActorAnimationSet AnimationSet;

    [SerializeField]
    bool AdaptAnimationSpeedToActorWalkingSpeed = true;

    [SerializeField, Range(1f, 30f)]
    float sampleRate = 4f;    

    [SerializeField]
    ActorAnimationState animationState;

    [SerializeField]
    CardinalDirection direction;

    [SerializeField]
    float secondsUntilNextFrame;

    [SerializeField]
    int frameIndex;

    SpriteRenderer spriteRenderer;
    Actor actor;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        actor = GetComponent<Actor>();
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
                    anim = AnimationSet.Sprites[animationState][direction];
                }                
            }
            
            spriteRenderer.sprite = anim[frameIndex++];

            if (AdaptAnimationSpeedToActorWalkingSpeed && animationState == ActorAnimationState.Walk)
            {
                secondsUntilNextFrame = World.Instance.GetGridSize() / (actor.GetWalkingSpeed() * anim.Length) + Time.deltaTime * 2f;                
            }
            else
            {
                secondsUntilNextFrame = 1f / sampleRate;
            }           
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
