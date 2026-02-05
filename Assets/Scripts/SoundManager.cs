using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class AudioPlay
{
    public string name;
    public AudioClip clip;
}
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;


    [Header("Audio Clips")]

    public AudioPlay[] sfxSounds;
    public AudioPlay[] sfxLoopSounds;
    public AudioPlay[] musicSounds;
    public AudioPlay[] musicAwakeSounds;



    [Header("Audio Sources")]

    public AudioSource sfxSource;
    // SoundManager.Instance.Play("name");
    public AudioSource sfxLoopSource;
    // SoundManager.Instance.PlaySFXLoop("name");
    // SoundManager.Instance.StopSFXLoop();
    public AudioSource musicSource;
    // SoundManager.Instance.PlayMusic("name");
    public AudioSource musicAwakeSource;
    // SoundManager.Instance.PlayMusic("name");

    [Header("Current Volumes")]
    [Range(0f, 1f)]
    public float musicVolume = 1f;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;



    [Header("Fade Settings")]
    public float fadeDuration = 1f;

    private Dictionary<string, AudioPlay> soundLookup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            soundLookup = new Dictionary<string, AudioPlay>();

            AddSoundsToLookup(sfxSounds);
            AddSoundsToLookup(sfxLoopSounds);
            AddSoundsToLookup(musicSounds);
            AddSoundsToLookup(musicAwakeSounds);

            PlayMusicAwake("MenuMusicAwake");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void AddSoundsToLookup(AudioPlay[] soundsArray)
    {
        if (soundsArray == null) return;

        foreach (var sound in soundsArray)
        {
            if (sound == null || sound.clip == null || string.IsNullOrEmpty(sound.name))
                continue;

            if (!soundLookup.ContainsKey(sound.name))
            {
                soundLookup.Add(sound.name, sound);
            }
            else
            {
                Debug.LogWarning("Duplicate sound name: " + sound.name);
            }
        }
    }


    public void MusicVolume(float volume)
    {
        musicVolume = volume; // save current user volume
        musicSource.volume = volume;
        musicAwakeSource.volume = volume;
    }

    public void SFXVolume(float volume)
    {
        sfxVolume = volume; // save current user volume
        sfxSource.volume = volume;
        sfxLoopSource.volume = volume;
    }

    public void PlaySFX(string soundName)
    {
        if (soundLookup.TryGetValue(soundName, out AudioPlay sound))
        {
            sfxSource.PlayOneShot(sound.clip);
        }
        else
        {
            Debug.LogWarning("SoundManager: Sound not found: " + soundName);
        }
    }

    public void PlaySFXLoop(string soundName)
    {
        if (!soundLookup.TryGetValue(soundName, out AudioPlay sound))
        {
            Debug.LogWarning("SoundManager: Sound not found: " + soundName);
            return;
        }

        sfxLoopSource.clip = sound.clip;
        sfxLoopSource.loop = true;
        sfxLoopSource.Play();
    }

    public void StopSFXLoop()
    {
        if (sfxLoopSource.isPlaying)
            sfxLoopSource.Stop();
    }

    public void PlayMusic(string soundName)
    {
        if (string.IsNullOrEmpty(soundName))
            return;

        // Check if the input contains multiple options
        string chosenName = soundName;

        if (soundName.Contains("/"))
        {
            string[] options = soundName.Split('/');
            chosenName = options[UnityEngine.Random.Range(0, options.Length)];
        }

        // Play the chosen music
        if (soundLookup.TryGetValue(chosenName, out AudioPlay sound))
        {
            StartCoroutine(FadeToNewMusic(sound));
        }
        else
        {
            Debug.LogWarning("SoundManager: Music not found: " + chosenName);
        }
    }


    public void PlayMusicAwake(string soundName)
    {
        if (soundLookup.TryGetValue(soundName, out AudioPlay sound))
        {
            musicAwakeSource.loop = true;
            musicAwakeSource.clip = sound.clip;
            musicAwakeSource.Play();
        }
        else
        {
            Debug.LogWarning("SoundManager: Music not found: " + soundName);
        }
    }

    public void StopMusicAwake()
    {
        if (musicAwakeSource.isPlaying)
            musicAwakeSource.Stop();
    }

    private IEnumerator FadeToNewMusic(AudioPlay newSound)
    {
        yield return StartCoroutine(FadeOutMusic());

        musicSource.clip = newSound.clip;
        musicSource.loop = true;
        musicSource.Play();

        yield return StartCoroutine(FadeInMusic(musicVolume));
    }


    private IEnumerator FadeOutMusic()
    {
        float startVolume = musicSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = 0;
    }

    private IEnumerator FadeInMusic(float targetVolume)
    {
        targetVolume = musicVolume; // use the user-set volume
        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(0, targetVolume, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }



    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
        musicAwakeSource.mute = !musicAwakeSource.mute;
    }

    public void ToggleSFX()
    {
        sfxSource.mute = !sfxSource.mute;
        sfxLoopSource.mute = !sfxLoopSource.mute;
    }


    public bool sfxLookupContains(string name)
    {
        return soundLookup.ContainsKey(name);
    }
}
