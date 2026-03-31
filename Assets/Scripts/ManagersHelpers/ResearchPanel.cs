using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResearchPanel : MonoBehaviour
{
    public static ResearchPanel Instance { get; private set; }

    [Header("Gold")]
    public int startingGold = 500;
    private int _gold;

    [Header("UI")]
    public TextMeshProUGUI goldText;
    public Button closeButton;

    private List<ResearchButton> _buttons = new List<ResearchButton>();

    private void Awake()
    {
        Instance = this;
        _gold = startingGold;
        GetComponentsInChildren(true, _buttons);
    }

    private void Start()
    {
        RefreshAll();
        gameObject.SetActive(false);
    }


    public void RefreshAll()
    {
        if (goldText) goldText.text = $"Gold: {_gold}";
        foreach (var btn in _buttons) btn.Refresh();
    }

    public bool CanAfford(int cost) => _gold >= cost;

    public void SpendGold(int amount)
    {
        _gold -= amount;
        if (goldText) goldText.text = $"Gold: {_gold}";
    }

    public void AddGold(int amount)
    {
        _gold += amount;
        RefreshAll();
    }

    public void ResetResearch()
    {
        foreach (var btn in _buttons)
            PlayerPrefs.DeleteKey($"research_{btn.gameObject.name}");

        PlayerPrefs.Save();
        RefreshAll();
    }
}