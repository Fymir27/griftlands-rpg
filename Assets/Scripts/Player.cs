﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Directions
{
    North,
    East,
    South,
    West
}

public class Player : Actor
{
    [Header("Player Properties")]
    public bool Moving;
    public bool IsTalking;

    // Start is called before the first frame update
    void Start()
    {
        InitActor();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Moving && !IsTalking)
        {
            var hor = Input.GetAxisRaw("Horizontal");
            var ver = Input.GetAxisRaw("Vertical");

            
            if (hor > 0)
            {
                MoveTo(GridPos + Vector3Int.right);
            }
            else if(hor < 0)
            {
                MoveTo(GridPos + Vector3Int.left);
            }
            else if(ver > 0)
            {
                MoveTo(GridPos + Vector3Int.up);
            }
            else if(ver < 0)
            {
                MoveTo(GridPos + Vector3Int.down);
            }
        }

        // Advance dialogue and check if done
        if(IsTalking && Input.GetButtonDown("Jump") && Textbox.Instance.AdvanceDialogue())
        {
            IsTalking = false;
        }
    }

    IEnumerator SetTimeout(float seconds, Action callback)
    {
        yield return new WaitForSeconds(seconds);
        callback();
    }

    void MoveTo(Vector3Int gridPos)
    {
        if (Moving)
            return;

        // probably a wall
        if (World.Instance.IsSolid(gridPos)) 
            return;

        Moving = true;

        switch (World.Instance.GetActor(gridPos))
        {
            case NPC npc:
                IsTalking = true;
                Textbox.Instance.Display(npc, "This is placeholder dialogue!");                
                break;

            case Enemy enemy:                
                enemy.CurHealth--;
                break;

            case null:
                // Tile seems to be empty so lets move there!                
                World.Instance.MoveActorTo(this, gridPos);
                GridPos = gridPos;

                // TODO: anmiation
                transform.position = World.Instance.GridToWorldPos(gridPos);
                break;
        }

        StartCoroutine(SetTimeout(.2f, () => Moving = false));
    }
}
