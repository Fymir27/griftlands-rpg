using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance;
        
    [SerializeField]
    AudioSource backgroundAudio;
    [SerializeField]
    AudioClip backgroundLoop;

    [SerializeField]
    AudioSource sfxAudio;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject); // There can only be one >:D
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
    {
        // start main background loop once intro has finished
        if(!backgroundAudio.isPlaying)
        {
            backgroundAudio.clip = backgroundLoop;
            backgroundAudio.loop = true;
            backgroundAudio.Play();
        }
    }

    public void PlayClip(AudioClip clip)
    {
        sfxAudio.clip = clip;
        sfxAudio.Play();
    }


}
