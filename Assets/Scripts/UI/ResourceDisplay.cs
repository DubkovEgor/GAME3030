using UnityEngine;
using UnityEngine.UI;

public class ResourceDisplay : MonoBehaviour
{
    [Header("Resources")]
    public Text resourceText;

    [Header("Housing")]
    public Text housingText;


    private void Start()
    {
        if (EconomyManager.Instance != null)
        {
            UpdateResourcesText();
            UpdateHousingText();
            EconomyManager.Instance.OnResourcesChanged += UpdateAllTexts;
        }
    }

    private void UpdateAllTexts()
    {
        if (resourceText != null) UpdateResourcesText();
        if (housingText != null) UpdateHousingText();
    }

    private void UpdateResourcesText()
    {
        if (resourceText == null) return;
        var eco = EconomyManager.Instance;
        resourceText.text = $"Food: {eco.food} ({eco.foodPerHour}/s)   " +
                            $"Wood: {eco.wood} ({eco.woodPerHour}/s)   " +
                            $"Stone: {eco.stone} ({eco.stonePerHour}/s)   " +
                            $"Fuel: {eco.fuel} ({eco.fuelPerHour}/s)   " +
                            $"Iron: {eco.iron} ({eco.ironPerHour}/s)   " +
                            $"Gold: {eco.gold} ({eco.goldPerHour}/s)";
    }
    public void UpdateHousingText()
    {
        if (housingText == null) return;
        housingText.text = $"Housing: {EconomyManager.Instance.currentNPCs}/{EconomyManager.Instance.housing}";
    }
    private void OnDestroy()
    {
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnResourcesChanged -= UpdateAllTexts;
    }
}
