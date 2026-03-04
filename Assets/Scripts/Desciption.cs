using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Description : MonoBehaviour
{
    public TextMeshProUGUI descriptionText;
    public Camera cam;

    private Building lastBuilding;
    private UIButtonInfo lastButton;

    private void Update()
    {
        if (CheckUIHover())
            return;

        CheckWorldHover();
    }

    private bool CheckUIHover()
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        foreach (var result in results)
        {
            UIButtonInfo uiButton = result.gameObject.GetComponent<UIButtonInfo>();

            if (uiButton != null)
            {
                if (uiButton != lastButton)
                {
                    lastButton = uiButton;
                    lastBuilding = null;

                    descriptionText.text = BuildTextForButton(uiButton.buildingPrefab);
                }
                return true;
            }
        }

        lastButton = null;
        return false;
    }

    private void CheckWorldHover()
    {
        if (cam == null) cam = Camera.main;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 2000f))
        {
            Building b = hit.collider.GetComponentInParent<Building>();

            if (b != null)
            {
                if (b != lastBuilding)
                {
                    lastButton = null;
                    lastBuilding = b;

                    descriptionText.text = BuildTextForWorld(b);
                }
                return;
            }
        }

        lastBuilding = null;
        descriptionText.text = "";
    }

    private string BuildTextForButton(Building building)
    {
        string cleanName = building.name.Replace("(Clone)", "").Trim();
        string text = $"<b>{cleanName}</b>\n";

        string costLine = "";
        if (building.Cost.food != 0) costLine += $"Food: {building.Cost.food}. ";
        if (building.Cost.wood != 0) costLine += $"Wood: {building.Cost.wood}. ";
        if (building.Cost.stone != 0) costLine += $"Stone: {building.Cost.stone}. ";
        if (building.Cost.iron != 0) costLine += $"Iron: {building.Cost.iron}. ";
        if (building.Cost.gold != 0) costLine += $"Gold: {building.Cost.gold}. ";

        if (!string.IsNullOrEmpty(costLine))
            text += $"<b>Cost:</b> {costLine.Trim()}\n";

        if (building.HousingProvided != 0)
            text += $"Housing: {building.HousingProvided}\n";

        string genLine = "";
        if (building.Generation.food != 0) genLine += $"Food: {FormatPerSec(building.Generation.food)}. ";
        if (building.Generation.wood != 0) genLine += $"Wood: {FormatPerSec(building.Generation.wood)}. ";
        if (building.Generation.stone != 0) genLine += $"Stone: {FormatPerSec(building.Generation.stone)}. ";
        if (building.Generation.iron != 0) genLine += $"Iron: {FormatPerSec(building.Generation.iron)}. ";
        if (building.Generation.gold != 0) genLine += $"Gold: {FormatPerSec(building.Generation.gold)}. ";

        if (!string.IsNullOrEmpty(genLine))
            text += $"<b>Generation:</b> {genLine.Trim()}\n";

        return text;
    }
    private string BuildTextForWorld(Building building)
    {
        string cleanName = building.name.Replace("(Clone)", "").Trim();
        string text = $"<b>{cleanName}</b>\n";

        if (building.HousingProvided != 0)
            text += $"Housing: {building.HousingProvided}\n";

        string genLine = "";
        if (building.Generation.food != 0) genLine += $"Food: {FormatPerSec(building.Generation.food)} ";
        if (building.Generation.wood != 0) genLine += $"Wood: {FormatPerSec(building.Generation.wood)} ";
        if (building.Generation.stone != 0) genLine += $"Stone: {FormatPerSec(building.Generation.stone)} ";
        if (building.Generation.iron != 0) genLine += $"Iron: {FormatPerSec(building.Generation.iron)} ";
        if (building.Generation.gold != 0) genLine += $"Gold: {FormatPerSec(building.Generation.gold)} ";

        if (!string.IsNullOrEmpty(genLine))
            text += $"<b>Generation:</b> {genLine.Trim()}\n";

        return text;
    }

    private string FormatPerSec(int value)
    {
        if (value == 0) return null;
        return $"{(value > 0 ? "+" : "")}{value}/sec";
    }

}
