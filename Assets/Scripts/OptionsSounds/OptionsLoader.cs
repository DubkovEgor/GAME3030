using UnityEngine;

public class OptionsLoader : MonoBehaviour
{
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string CAMERA_SPEED_KEY = "CameraSpeed";
    private const string BRIGHTNESS_KEY = "Brightness";
    private const string MUSIC_ICON_KEY = "MusicButton";
    private const string SFX_ICON_KEY = "SFXButton";

    private void Awake()
    {
        ApplySavedOptions();
    }

    public void ApplySavedOptions()
    {
        if (SoundManager.Instance != null)
        {
            if (PlayerPrefs.HasKey(MUSIC_VOLUME_KEY))
                SoundManager.Instance.MusicVolume(PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f));

            if (PlayerPrefs.HasKey(SFX_VOLUME_KEY))
                SoundManager.Instance.SFXVolume(PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f));
        }

        if (PlayerPrefs.HasKey(CAMERA_SPEED_KEY))
        {
            float speed = PlayerPrefs.GetFloat(CAMERA_SPEED_KEY, 1f);
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                var cameraMove = mainCam.GetComponent<CameraMove>();
                if (cameraMove != null)
                    cameraMove.cameraXYspeed = speed;
            }
        }

        if (PlayerPrefs.HasKey(BRIGHTNESS_KEY))
        {
            float brightness = PlayerPrefs.GetFloat(BRIGHTNESS_KEY, 1f);
            BrightnessController brightnessController = FindObjectOfType<BrightnessController>();
            if (brightnessController != null)
                brightnessController.ApplyBrightness(brightness);
        }
        IconChange iconChange = IconChange.Instance;
        if (iconChange != null)
        {
            if (PlayerPrefs.HasKey(MUSIC_ICON_KEY))
            {
                int musicIndex = PlayerPrefs.GetInt(MUSIC_ICON_KEY, 0);
                if (musicIndex >= 0 && musicIndex < iconChange.musicSprites.Length)
                    iconChange.musicButton.sprite = iconChange.musicSprites[musicIndex];
            }

            if (PlayerPrefs.HasKey(SFX_ICON_KEY))
            {
                int sfxIndex = PlayerPrefs.GetInt(SFX_ICON_KEY, 0);
                if (sfxIndex >= 0 && sfxIndex < iconChange.sfxSprites.Length)
                    iconChange.sfxButton.sprite = iconChange.sfxSprites[sfxIndex];
            }
        }
    }
}
