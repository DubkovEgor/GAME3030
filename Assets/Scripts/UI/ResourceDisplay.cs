using UnityEngine;
using UnityEngine.UI;

public class ResourceDisplay : MonoBehaviour
{
    [Header("Resources")]
    public Text resourceText;

    [Header("Housing")]
    public Text housingText;

    private int currentPopulation = 0;

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
        UpdateResourcesText();
        UpdateHousingText();
    }
    private void UpdateResourcesText()
    {
        var eco = EconomyManager.Instance;
        resourceText.text = $"Food: {eco.food} ({eco.foodPerHour}/s)   " +
                            $"Wood: {eco.wood} ({eco.woodPerHour}/s)   " +
                            $"Stone: {eco.stone} ({eco.stonePerHour}/s)   " +
                            $"Fuel: {eco.fuel} ({eco.fuelPerHour}/s)   " +
                            $"Iron: {eco.iron} ({eco.ironPerHour}/s)   " +
                            $"Gold: {eco.gold} ({eco.goldPerHour}/s)";
    }
    public void SetCurrentPopulation(int population) 
    {
        currentPopulation = population;
        UpdateHousingText();
    }
    public void UpdateHousingText()
    {
        housingText.text = $"Housing: {EconomyManager.Instance.currentNPCs}/{EconomyManager.Instance.housing}";
    }
    private void OnDestroy()
    {
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnResourcesChanged -= UpdateAllTexts;
    }
}
