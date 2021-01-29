using System;
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
    public Vector3Int GridPos;

    // Start is called before the first frame update
    void Start()
    {
        GridPos = World.Instance.WorldToGridPos(transform.position);
        // snap to grid
        transform.position = World.Instance.GridToWorldPos(GridPos);
    }

    // Update is called once per frame
    void Update()
    {
        if(!Moving)
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
        
    }

    IEnumerator SetTimeout(float seconds, Action callback)
    {
        yield return new WaitForSeconds(seconds);
        callback();
    }

    void MoveTo(Vector3Int gridPos)
    {       
        if (World.Instance.IsSolid(gridPos)) return;
        Moving = true;
        GridPos = gridPos;
        transform.position = World.Instance.GridToWorldPos(gridPos);
        StartCoroutine(SetTimeout(.3f, () => Moving = false));
    }
}
