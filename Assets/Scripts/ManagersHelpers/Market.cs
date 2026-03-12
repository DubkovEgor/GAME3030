using UnityEngine;

[System.Serializable]
public class MarketResource
{
    public string name;
    public float baseValue;
    [Range(0.2f, 2f)] public float currentPriceRate = 1f;

    public float EffectiveValue => baseValue * currentPriceRate;
    public int RatePercent => Mathf.RoundToInt(currentPriceRate * 100);
    public string RateLabel => $"{RatePercent}%";

    public Color RateColor()
    {
        if (currentPriceRate >= 0.9f) return Color.green;
        if (currentPriceRate >= 0.65f) return Color.yellow;
        return Color.red;
    }
}

public class Market : MonoBehaviour
{
    public static Market Instance { get; private set; }

    [Header("Resources")]
    public MarketResource food = new MarketResource { name = "Food", baseValue = 1.0f };
    public MarketResource wood = new MarketResource { name = "Wood", baseValue = 1.5f };
    public MarketResource stone = new MarketResource { name = "Stone", baseValue = 1.5f };
    public MarketResource fuel = new MarketResource { name = "Fuel", baseValue = 2.0f };
    public MarketResource iron = new MarketResource { name = "Iron", baseValue = 3.0f };
    public MarketResource gold = new MarketResource { name = "Gold", baseValue = 4.0f };

    [Header("Economy Tuning")]
    public float sellPressure = 0.012f;
    public float naturalRecovery = 0.04f;
    public float recoveryInterval = 8f;

    [Header("Market Fee")]
    [Range(0f, 0.5f)] public float feeFraction = 0.05f;

    public event System.Action OnRatesChanged;

    MarketResource[] _all;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _all = new[] { food, wood, stone, fuel, iron, gold };
        InvokeRepeating(nameof(NaturalRecoveryTick), recoveryInterval, recoveryInterval);
    }

    public int Preview(MarketResource give, MarketResource get, int amount)
    {
        if (give == null || get == null || give == get || amount <= 0) return 0;
        float ratio = give.EffectiveValue / get.EffectiveValue;
        return Mathf.FloorToInt(amount * ratio * (1f - feeFraction));
    }

    public bool ExecuteTrade(MarketResource give, MarketResource get, int amount)
    {
        int receive = Preview(give, get, amount);
        if (receive <= 0) return false;

        EconomyManager eco = EconomyManager.Instance;

        if (!HasStock(give, amount, eco)) return false;

        RemoveResource(give, amount, eco);
        AddResource(get, receive, eco);

        give.currentPriceRate = Mathf.Clamp(give.currentPriceRate - (amount / 100f) * sellPressure, 0.2f, 2f);
        get.currentPriceRate = Mathf.Clamp(get.currentPriceRate + (receive / 100f) * sellPressure, 0.2f, 2f);

        eco.NotifyResourcesChanged();
        OnRatesChanged?.Invoke();
        return true;
    }

    public void ApplyExternalShift(MarketResource res, float delta)
    {
        res.currentPriceRate = Mathf.Clamp(res.currentPriceRate + delta, 0.2f, 2f);
        OnRatesChanged?.Invoke();
    }

    public MarketResource GetByName(string resName)
    {
        foreach (var r in _all)
            if (r.name.ToLower() == resName.ToLower()) return r;
        return null;
    }

    public MarketResource[] GetAll() => _all;

    bool HasStock(MarketResource res, int amount, EconomyManager eco)
    {
        if (res == food) return eco.food >= amount;
        if (res == wood) return eco.wood >= amount;
        if (res == stone) return eco.stone >= amount;
        if (res == fuel) return eco.fuel >= amount;
        if (res == iron) return eco.iron >= amount;
        if (res == gold) return eco.gold >= amount;
        return false;
    }

    void RemoveResource(MarketResource res, int amount, EconomyManager eco)
    {
        if (res == food) eco.food -= amount;
        if (res == wood) eco.wood -= amount;
        if (res == stone) eco.stone -= amount;
        if (res == fuel) eco.fuel -= amount;
        if (res == iron) eco.iron -= amount;
        if (res == gold) eco.gold -= amount;
    }

    void AddResource(MarketResource res, int amount, EconomyManager eco)
    {
        if (res == food) eco.food += amount;
        if (res == wood) eco.wood += amount;
        if (res == stone) eco.stone += amount;
        if (res == fuel) eco.fuel += amount;
        if (res == iron) eco.iron += amount;
        if (res == gold) eco.gold += amount;
    }

    void NaturalRecoveryTick()
    {
        bool changed = false;
        foreach (var r in _all)
        {
            float gap = 1f - r.currentPriceRate;
            if (Mathf.Abs(gap) > 0.01f)
            {
                r.currentPriceRate += gap * naturalRecovery;
                changed = true;
            }
        }
        if (changed) OnRatesChanged?.Invoke();
    }

    public void NotifyRatesChanged()
    {
        OnRatesChanged?.Invoke();
    }
}