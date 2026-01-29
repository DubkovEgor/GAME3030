using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    //public static SoundManager Instance;

    public AudioPlay[] musicSounds, sfxSounds/*, sfxLoopSounds, sfxOnTriggerSounds*/;
    public AudioSource musicSource, sfxSource/*, sfxLoopSource, sfxOnTriggerSource*/;

    private void Awake()
    {
        //if (Instance == null)
        //{
        //    Instance = this;
        //    //DontDestroyOnLoad(gameObject);
        //}
        //
        //else
        //{
        //    Destroy(gameObject);
        //}
    }

    private void Start()
    {
        PlayMusic("Start");
        //SoundManager.Instance.PlayMusic("");
    }

    public void PlayMusic(string name)
    {
        AudioPlay m = Array.Find(musicSounds, x => x.name == name);
        if (m == null)
        {
            Debug.Log("Music not found");
        }

        else
        {
            musicSource.clip = m.clip;
            musicSource.Play();
        }
    }

    public void PlaySFX(string name)
    {
        AudioPlay s = Array.Find(sfxSounds, x => x.name == name);
        if (s == null)
        {
            Debug.Log("Sound not found");
        }

        else
        {
            sfxSource.clip = s.clip;
            sfxSource.Play();
        }
    }
    //public void PlaySFXLoop(string name)
    //{
    //    AudioPlay s = Array.Find(sfxLoopSounds, x => x.name == name);
    //    if (s == null)
    //    {
    //        Debug.Log("Sound not found");
    //    }
    //
    //    else
    //    {
    //        sfxLoopSource.clip = s.clip;
    //        sfxLoopSource.Play();
    //    }
    //}
    //
    //public void PlaySFXOnTrigger(string name)
    //{
    //    AudioPlay s = Array.Find(sfxOnTriggerSounds, x => x.name == name);
    //    if (s == null)
    //    {
    //        Debug.Log("Sound not found");
    //    }
    //
    //    else
    //    {
    //        sfxOnTriggerSource.clip = s.clip;
    //        sfxOnTriggerSource.Play();
    //    }
    //}
    //
    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
    }
    public void ToggleSFX()
    {
        sfxSource.mute = !sfxSource.mute;
        //sfxLoopSource.mute = !sfxLoopSource.mute;
        //sfxOnTriggerSource.mute = !sfxOnTriggerSource.mute;
    }
    public void MusicVolume(float volume)
    {
        musicSource.volume = volume;
    }
    public void SFXVolume(float volume)
    {
        sfxSource.volume = volume;
        //sfxLoopSource.volume = volume;
        //sfxOnTriggerSource.volume = volume;
    }
}
