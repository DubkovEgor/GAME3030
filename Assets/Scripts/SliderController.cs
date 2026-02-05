using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public Slider musicSlider, sfxSlider;

    public static SliderController Instance;    

    private void Awake()
    {
        Instance = this;
    }
    private void OnDisable()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
    }

    // Load UI settings when the scene starts
    private void OnEnable()
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");

        if (PlayerPrefs.HasKey("SFXVolume"))
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
    }
    public void ToggleMusic()
    {
        SoundManager.Instance.ToggleMusic();
    }

    public void ToggleSFX()
    {
        SoundManager.Instance.ToggleSFX();
    }

    public void MusicVolume()
    {
        SoundManager.Instance.MusicVolume(musicSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        PlayerPrefs.Save();
    }
    public void SFXVolume()
    {
        SoundManager.Instance.SFXVolume(sfxSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
        PlayerPrefs.Save();
    }

    [SerializeField] private SpriteState spriteState1;
    [SerializeField] private Button buttonSound;
    [SerializeField] private SpriteState spriteState2;
    [SerializeField] private Button buttonMusic;
    public void SpriteChangeSFX()
    {

        buttonSound.spriteState = spriteState1;
    }
    public void SpriteChangeMusic()
    {

        buttonMusic.spriteState = spriteState2;
    }

}
