using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
static class Vector3IntExtensions
{
    public static Vector3Int[] Neighbours(this Vector3Int pos)
    {
        return new Vector3Int[]
        {
            pos + Vector3Int.up,
            pos + Vector3Int.right,
            pos + Vector3Int.down,
            pos + Vector3Int.left,
        };
    }
}