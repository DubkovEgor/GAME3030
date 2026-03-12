using UnityEngine;

public class OtherScenePlayAudio : MonoBehaviour
{
    public void PlaySFXByName(string sfxName)
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("SoundManager not found!");
            return;
        }

        if (string.IsNullOrEmpty(sfxName))
        {
            Debug.LogWarning("SFX name is empty!");
            return;
        }

        SoundManager.Instance.PlaySFX(sfxName);
    }
}
