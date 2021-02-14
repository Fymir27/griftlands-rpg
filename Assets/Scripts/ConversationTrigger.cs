using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConversationTrigger : MonoBehaviour
{
    [SerializeField]
    Conversation conversation;

    private void Start()
    {
        var spriteRender = GetComponent<SpriteRenderer>();
        if (spriteRender != null)
            spriteRender.enabled = false;
        var gridPos = World.Instance.WorldToGridPos(transform.position);
        World.Instance.SetConversation(conversation, gridPos);
    }
}
