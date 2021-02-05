using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Guns : MonoBehaviour
{
    [SerializeField]
    LineRenderer[] gunTrails;
    [SerializeField]
    Gradient trailColor;
    [SerializeField, Range(.03f, 3f)]
    float trailWidth;

    private void Start()
    {
        foreach (var trail in gunTrails)
        {
            trail.positionCount = 2;            
            trail.startWidth = trailWidth;                    
            trail.endWidth = trailWidth;
            trail.colorGradient = trailColor;
            trail.enabled = false;
        }
    }

    public void Shoot(Vector3 target, float seconds)
    {
        foreach(var trail in gunTrails)
        {   
            trail.SetPosition(0, trail.transform.position);
            trail.SetPosition(1, target);
            trail.enabled = true;
        }

        World.Instance.SetTimeout(seconds, () =>
        {            
            foreach (var trail in gunTrails)
            {
                trail.enabled = false;                
            }
        });
    }
}
