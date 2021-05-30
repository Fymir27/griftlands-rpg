using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningBar : MonoBehaviour
{
    public static WarningBar Instance;

    Color color 
    {
        get => icon.color;
        set
        {
            icon.color = value;
            value.a = backgroundAlpha;
            background.color = value;
        }
    }

    [Header("Properties")]            
    [SerializeField, Range(0f, 1f)]
    float fadeInDuration = .2f;
    [SerializeField, Range(0f, 1f)]
    float fadeOutDuration = .2f;


    [Header("References")]
    [SerializeField]
    Text uiText;
    [SerializeField]
    Image background;
    [SerializeField]
    Image icon;

    // internals
    Color defaultColor;    
    float backgroundAlpha = .7f;
    float currentFadeDuration = 0f;
    float targetAlpha = 0f;
    Coroutine fadeOutTimeout;

    private void Awake()
    {
        Instance = this;
        defaultColor = icon.color;
        backgroundAlpha = background.color.a;
    }

    private void Update()
    {
        background.CrossFadeAlpha(targetAlpha, currentFadeDuration, false);
        icon.CrossFadeAlpha(targetAlpha, currentFadeDuration, false);
        uiText.CrossFadeAlpha(targetAlpha, currentFadeDuration, false);
    }  

    public void Display(string message, Color? color = null, float duration = 1f)
    {
        this.color = color ?? defaultColor;
        uiText.text = message;

        // start fading in
        targetAlpha = 1f;
        currentFadeDuration = fadeInDuration;
        
        if (fadeOutTimeout != null)
            StopCoroutine(fadeOutTimeout);

        // wait and fade out again
        fadeOutTimeout = StartCoroutine(DelayedFadeOut(duration + fadeInDuration));
    }
    
    IEnumerator DelayedFadeOut(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        targetAlpha = 0f;
        currentFadeDuration = fadeOutDuration;
    }
}
