using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconChange : MonoBehaviour
{
    [SerializeField] public Sprite[] sfxSprites;
    [SerializeField] public Image sfxButton;
    [SerializeField] public Sprite[] musicSprites;
    [SerializeField] public Image musicButton;

    public static IconChange Instance;

    private const string SFX_MUTE_KEY = "SFXMuted";
    private const string MUSIC_MUTE_KEY = "MusicMuted";

    private void Awake()
    {
        Instance = this;
    }

    private void OnDisable()
    {
        SaveState();
    }

    private void OnEnable()
    {
        LoadState();
    }

    private void SaveState()
    {
        SaveButtonSpriteState("SFXButton", sfxButton.sprite, sfxSprites);
        SaveButtonSpriteState("MusicButton", musicButton.sprite, musicSprites);

        PlayerPrefs.SetInt(SFX_MUTE_KEY, SoundManager.Instance.sfxSource.mute ? 1 : 0);
        PlayerPrefs.SetInt(MUSIC_MUTE_KEY, SoundManager.Instance.musicSource.mute ? 1 : 0);

        PlayerPrefs.Save();
    }

    private void LoadState()
    {
        LoadButtonSpriteState("SFXButton", sfxButton, sfxSprites);
        LoadButtonSpriteState("MusicButton", musicButton, musicSprites);

        bool sfxMuted = PlayerPrefs.GetInt(SFX_MUTE_KEY, 0) == 1;
        bool musicMuted = PlayerPrefs.GetInt(MUSIC_MUTE_KEY, 0) == 1;

        SoundManager.Instance.sfxSource.mute = sfxMuted;
        SoundManager.Instance.musicSource.mute = musicMuted;

        ApplySFXIconVolume();
        ApplyMusicIconVolume();
    }

    private void ApplySFXIconVolume()
    {
        bool sfxMuted = (sfxButton.sprite == sfxSprites[1]);
        SoundManager.Instance.sfxSource.mute = sfxMuted;
    }

    private void ApplyMusicIconVolume()
    {
        bool musicMuted = (musicButton.sprite == musicSprites[1]);
        SoundManager.Instance.musicSource.mute = musicMuted;
    }


    private void SaveButtonSpriteState(string key, Sprite sprite, Sprite[] sprites)
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprite == sprites[i])
            {
                PlayerPrefs.SetInt(key, i);
                return;
            }
        }
    }

    private void LoadButtonSpriteState(string key, Image button, Sprite[] sprites)
    {
        if (PlayerPrefs.HasKey(key))
        {
            int spriteIndex = PlayerPrefs.GetInt(key);
            if (spriteIndex >= 0 && spriteIndex < sprites.Length)
                button.sprite = sprites[spriteIndex];
        }
    }

    public void ChangeSpriteSFX()
    {
        if (sfxButton.sprite == sfxSprites[0])
            sfxButton.sprite = sfxSprites[1];
        else
            sfxButton.sprite = sfxSprites[0];

        ApplySFXIconVolume();
        SaveState();
    }

    public void ChangeSpriteMusic()
    {
        if (musicButton.sprite == musicSprites[0])
            musicButton.sprite = musicSprites[1];
        else
            musicButton.sprite = musicSprites[0];

        ApplyMusicIconVolume();
        SaveState();
    }
}
