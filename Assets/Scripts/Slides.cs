using System;
using UnityEngine;
using UnityEngine.UI;

public class Slides : MonoBehaviour
{
    public event Action OnSlideshowEnd;

    [SerializeField]
    string[] content;
    [SerializeField]
    float transitionDuration = 1f;

    [SerializeField]
    Image continueImage;
    [SerializeField]
    Text contentUIText;
    [SerializeField]
    Image backgroundImage;

    bool slideshowStarted = false;
    int contentIndex = 0;   
    float targetAlphaText = 0f;
    float fadeDurationText = 0f;
    float targetAlphaBackground = 0f;
    float fadeDurationBackground = 0f;

    // Update is called once per frame
    void Update()
    {
        if (!slideshowStarted)
        { 
            return; 
        }

        if (Input.GetButtonDown("Submit"))
        {
            NextSlide();
        }   

        if (Input.GetButtonDown("Cancel"))
        {
            End();
        }

        contentUIText.CrossFadeAlpha(targetAlphaText, fadeDurationText, false);
        continueImage.CrossFadeAlpha(targetAlphaBackground, fadeDurationBackground, false);
        backgroundImage.CrossFadeAlpha(targetAlphaBackground, fadeDurationBackground, false);
    }

    public void BeginSlideshow()
    {
        if (content.Length == 0)
        {
            Debug.LogError("No images for slideshow!");
            return;
        }

        slideshowStarted = true;
        
        backgroundImage.enabled = true;
        contentUIText.enabled = true;
        continueImage.enabled = true;

        contentIndex = 0;
        contentUIText.text = content[0];

        // fade in
        fadeDurationText = transitionDuration / 2;
        targetAlphaText = 1f;
        fadeDurationBackground = transitionDuration / 2;
        targetAlphaBackground = 1f;
    }

    void NextSlide()
    {
        if(contentIndex == (content.Length - 1))
        {
            End();
            return;
        }       

        // fade out current image
        fadeDurationText = transitionDuration / 2;
        targetAlphaText = 0f;

        StartCoroutine(DelayedTextFadeIn(transitionDuration / 2, transitionDuration / 2));
    }

    void End()
    {
        fadeDurationText = transitionDuration / 2;
        targetAlphaText = 0f;
        fadeDurationBackground = transitionDuration / 2;
        targetAlphaBackground = 0f;

        if (OnSlideshowEnd != null)
        {
            OnSlideshowEnd.Invoke();
            OnSlideshowEnd = null;
        }
    }

    System.Collections.IEnumerator DelayedTextFadeIn(float delay, float duration)
    {
        yield return new WaitForSeconds(delay);        
        contentUIText.text = content[++contentIndex];
        targetAlphaText = 1f;
        fadeDurationText = duration;
    }
}
