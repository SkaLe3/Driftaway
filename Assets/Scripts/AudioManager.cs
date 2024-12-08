using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager s_Instance;
    public static AudioManager Instance => s_Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            s_Instance = this;
        }
    }

    public void PlaySoundOneShot(AudioClip clip, float volume, float pitch)
    {
        var audioSource = new GameObject("TempAudio").AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.Play();
        Destroy(audioSource.gameObject, clip.length);
    }
}
