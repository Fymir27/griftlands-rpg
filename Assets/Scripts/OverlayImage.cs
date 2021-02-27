using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class OverlayImage : MonoBehaviour
{
    public static OverlayImage Instance;    

    Image image;

    [SerializeField]
    bool fadingIn = false;
    [SerializeField]
    float duration = 0f;

    private void Awake()
    {
        Instance = this;
        image = GetComponent<Image>();
        var col = image.color;
        col.a = 1f;
        image.color = col;
        FadeOut(1f);
    }

    private void Update()
    {        
        if (fadingIn)
            image.CrossFadeAlpha(1f, duration, false);
        else
            image.CrossFadeAlpha(0f, duration, false);
    }

    public void FadeIn(float duration = 1f)
    {
        fadingIn = true;
        this.duration = duration;
    }

    public void FadeOut(float duration = 1f)
    {
        fadingIn = false;
        this.duration = duration;
    }

    public void SetImage(Sprite sprite)
    {
        image.sprite = sprite;
    }

    public void SetColor(Color color)
    {
        image.color = color;
    }
}
