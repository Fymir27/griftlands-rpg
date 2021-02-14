using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance;

    [SerializeField]
    AudioSource backgroundAudio;

    [SerializeField]
    AudioSource sfxAudio;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayClip(AudioClip clip)
    {
        sfxAudio.clip = clip;
        sfxAudio.Play();
    }


}
