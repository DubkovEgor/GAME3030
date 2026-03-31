using UnityEngine;
using UnityEngine.UI;


public class BrightnessController : MonoBehaviour
{
    public Image brightnessOverlay;
    public Slider brightnessSlider;

    private const string BRIGHTNESS_KEY = "Brightness";

    private void OnEnable()
    {
        float savedBrightness = PlayerPrefs.GetFloat(BRIGHTNESS_KEY, 1f);
        savedBrightness = Mathf.Clamp(savedBrightness, 0.2f, 1f);
        brightnessSlider.value = savedBrightness;
        ApplyBrightness(savedBrightness);

        brightnessSlider.onValueChanged.AddListener(ApplyBrightness);
    }

    public void ApplyBrightness(float value)
    {
        value = Mathf.Clamp(value, 0.2f, 1f);

        float overlayAlpha = 1f - value;
        Color c = brightnessOverlay.color;
        c.a = overlayAlpha;
        brightnessOverlay.color = c;

        PlayerPrefs.SetFloat(BRIGHTNESS_KEY, value);
        PlayerPrefs.Save();
    }
}
