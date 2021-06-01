using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UiImageAnimator : MonoBehaviour
{
    [SerializeField, Range(1, 30)]
    int sampleRate;

    [SerializeField]
    Sprite[] sprites;

    float secondsUntilNextFrame;
    int frameIndex = 0;

    Image image;

    private void Start()
    {
        image = GetComponent<Image>();
        image.sprite = sprites[0];
        secondsUntilNextFrame = 1f / sampleRate;       
    }

    void Update()
    {
        secondsUntilNextFrame -= Time.deltaTime;
        if (secondsUntilNextFrame <= 0)
        {
            image.sprite = sprites[frameIndex];
            frameIndex = (frameIndex + 1) % sprites.Length;
            secondsUntilNextFrame = 1f / sampleRate;           
        }
    }
}
