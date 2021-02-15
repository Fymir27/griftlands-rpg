using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class OneTimeAnimation : MonoBehaviour
{
    [SerializeField, Range(1, 30)]
    int sampleRate;

    [SerializeField]
    Sprite[] sprites;

    float secondsUntilNextFrame;
    int frameIndex = 0;

    SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprites[frameIndex];
        secondsUntilNextFrame = 1f / sampleRate;       
    }

    void Update()
    {
        secondsUntilNextFrame -= Time.deltaTime;
        if (secondsUntilNextFrame <= 0)
        {
            // my job here is done
            // SELF DESTRUCT SEQUENCE: INITIATED
            if (frameIndex >= sprites.Length)
            {
                Destroy(gameObject);
                return;
            }

            spriteRenderer.sprite = sprites[frameIndex++];           
            secondsUntilNextFrame = 1f / sampleRate;           
        }
    }
}
