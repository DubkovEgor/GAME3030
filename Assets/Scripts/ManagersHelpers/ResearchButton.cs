using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResearchButton : MonoBehaviour
{
    [Header("Upgrade Info")]
    public string upgradeName = "Upgrade Name";
    [TextArea(2, 3)]
    public string description = "What this upgrade does.";
    public int cost = 100;

    [Header("Unlock Condition")]
    [Tooltip("Leave empty to be available from the start")]
    public ResearchButton requiredUpgrade;

    [Header("UI")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public Image background;
    public GameObject lockedOverlay;
    public GameObject purchasedOverlay;

    [Header("Colors")]
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f);
    public Color availableColor = Color.white;
    public Color purchasedColor = new Color(0.4f, 1f, 0.5f);

    private string SaveKey => $"research_{gameObject.name}";
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
    }

    private void Start() => Refresh();

    public void Refresh()
    {
        bool purchased = IsPurchased();
        bool locked = !purchased && !IsUnlocked();
        bool canAfford = ResearchPanel.Instance.CanAfford(cost);

        if (nameText) nameText.text = upgradeName;
        if (costText) costText.text = purchased ? "Purchased" : $"{cost} Gold";

        if (lockedOverlay) lockedOverlay.SetActive(locked);
        if (purchasedOverlay) purchasedOverlay.SetActive(purchased);

        _button.interactable = !purchased && !locked && canAfford;

        if (background)
            background.color = purchased ? purchasedColor
                             : locked ? lockedColor
                                         : availableColor;
    }

    private void OnClick()
    {
        ResearchPanel.Instance.SpendGold(cost);
        PlayerPrefs.SetInt(SaveKey, 1);
        PlayerPrefs.Save();
        ResearchPanel.Instance.RefreshAll();
    }

    public bool IsPurchased() => PlayerPrefs.GetInt(SaveKey, 0) == 1;

    public bool IsUnlocked()
    {
        if (requiredUpgrade == null) return true;
        return requiredUpgrade.IsPurchased();
    }
}