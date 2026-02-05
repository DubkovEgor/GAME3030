using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class IconChange : MonoBehaviour
{
    [SerializeField] private Sprite[] sfxSprites;
    [SerializeField] private Image sfxButton;
    [SerializeField] private Sprite[] musicSprites;
    [SerializeField] private Image musicButton;

    public static IconChange Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDisable()
    {
        SaveButtonSpriteState("SFXButton", sfxButton.sprite, sfxSprites);
        SaveButtonSpriteState("MusicButton", musicButton.sprite, musicSprites);
    }

    // Load button sprites when the scene starts
    private void OnEnable()
    {
        LoadButtonSpriteState("SFXButton", sfxButton, sfxSprites);
        LoadButtonSpriteState("MusicButton", musicButton, musicSprites);
    }

    private void SaveButtonSpriteState(string key, Sprite sprite, Sprite[] sprites)
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprite == sprites[i])
            {
                PlayerPrefs.SetInt(key, i);
                PlayerPrefs.Save();
                return;
            }
        }
    }

    // Helper method to load button sprite state
    private void LoadButtonSpriteState(string key, Image button, Sprite[] sprites)
    {
        if (PlayerPrefs.HasKey(key))
        {
            int spriteIndex = PlayerPrefs.GetInt(key);
            if (spriteIndex >= 0 && spriteIndex < sprites.Length)
            {
                button.sprite = sprites[spriteIndex];
            }
        }
    }
    public void ChangeSpriteSFX()
    {
        if (sfxButton.sprite == sfxSprites[0])
        {
            sfxButton.sprite = sfxSprites[1];
            return;
        }

        sfxButton.sprite = sfxSprites[0];
    }
    public void ChangeSpriteMusic()
    {
        if (musicButton.sprite == musicSprites[0])
        {
            musicButton.sprite = musicSprites[1];
            return;
        }

        musicButton.sprite = musicSprites[0];
    }

}
