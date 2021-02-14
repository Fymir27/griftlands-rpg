using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeTrigger : MonoBehaviour
{
    [SerializeField]
    string scene;

    private void Start()
    {
        var spriteRender = GetComponent<SpriteRenderer>();
        if (spriteRender != null)
            spriteRender.enabled = false;
        var gridPos = World.Instance.WorldToGridPos(transform.position);
        World.Instance.SetSceneTrigger(scene, gridPos);
    }
}
