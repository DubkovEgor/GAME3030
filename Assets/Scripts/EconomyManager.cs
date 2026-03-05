using UnityEngine;
using System;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    [Header("Current Resources")]
    public int gold = 1000;
    public int iron = 500;
    public int stone = 500;
    public int wood = 500;
    public int food = 2500;
    public int fuel = 0;
    public int housing = 0;
    public int woodWorkers = 0;
    public int stoneWorkers = 0;
    public int foodWorkers = 0;
    public int ironWorkers = 0;
    public int goldWorkers = 0;
    public int fuelWorkers = 0;
    public int currentNPCs = 0;
    public int currentIdleNPCs = 0;
    public int currentWoodWorkers = 0;
    public int currentStoneWorkers = 0;
    public int currentFoodWorkers = 0;
    public int currentIronWorkers = 0;
    public int currentGoldWorkers = 0;
    public int currentFuelWorkers = 0;

    [Header("Resource Generation Per Hour")]
    public float goldPerHour;
    public float ironPerHour;
    public float stonePerHour;
    public float woodPerHour;
    public float foodPerHour;
    public float fuelPerHour;

    public event Action OnResourcesChanged;
    public event Action OnWin;

    private bool winTriggered = false;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject gameUI;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        var weather = FindFirstObjectByType<WeatherSystem>();
        if (weather != null)
            weather.OnHourPassed += OnHourTick;
        else
            Debug.LogWarning("EconomyManager: WeatherSystem not found.");
    }
    private void OnDestroy()
    {
        var weather = FindFirstObjectByType<WeatherSystem>();
        if (weather != null)
            weather.OnHourPassed -= OnHourTick;
    }

    private void OnHourTick()
    {
        bool changed = false;

        if (goldPerHour != 0) { gold += Mathf.RoundToInt(goldPerHour); changed = true; }
        if (ironPerHour != 0) { iron += Mathf.RoundToInt(ironPerHour); changed = true; }
        if (stonePerHour != 0) { stone += Mathf.RoundToInt(stonePerHour); changed = true; }
        if (woodPerHour != 0) { wood += Mathf.RoundToInt(woodPerHour); changed = true; }
        if (foodPerHour != 0) { food += Mathf.RoundToInt(foodPerHour); changed = true; }
        if (fuelPerHour != 0) { fuel += Mathf.RoundToInt(fuelPerHour); changed = true; }

        if (changed) OnResourcesChanged?.Invoke();
    }
    private void Update()
    {
        
        CheckWinCondition();
    }

    public bool CanAfford(ResourceCost cost)
    {
        return gold >= cost.gold && iron >= cost.iron && stone >= cost.stone && wood >= cost.wood && food >= cost.food && fuel >= cost.fuel;
    }

    public bool SpendResources(ResourceCost cost)
    {
        if (!CanAfford(cost)) return false;

        gold -= cost.gold;
        iron -= cost.iron;
        stone -= cost.stone;
        wood -= cost.wood;
        food -= cost.food;
        fuel -= cost.fuel;

        OnResourcesChanged?.Invoke();
        return true;
    }

    public void AddResources(ResourceCost cost)
    {
        gold += cost.gold;
        iron += cost.iron;
        stone += cost.stone;
        wood += cost.wood;
        food += cost.food;
        fuel += cost.fuel;
        OnResourcesChanged?.Invoke();
    }

    public void Add1000Resources()
    {
        ResourceCost bonus = new ResourceCost(100000, 100000, 100000, 100000, 100000, 100000);
        AddResources(bonus);
    }

    public void NotifyResourcesChanged()
    {
        OnResourcesChanged?.Invoke();
    }
    private void CheckWinCondition()
    {
        if (!winTriggered && currentNPCs >= 1000)
        {
            winTriggered = true;

            if (winPanel != null && gameUI != null)
            {
                winPanel.SetActive(true);
                gameUI.SetActive(false);
                Time.timeScale = 0f;
            }

            Debug.Log("You Win!");
            OnWin?.Invoke();
        }
    }

}

[System.Serializable]
public struct ResourceCost
{
    public int gold;
    public int iron;
    public int stone;
    public int wood;
    public int food;
    public int fuel;

    public ResourceCost(int gold, int iron, int stone, int wood, int food, int fuel)
    {
        this.gold = gold;
        this.iron = iron;
        this.stone = stone;
        this.wood = wood;
        this.food = food;
        this.fuel = fuel;
    }

    public static ResourceCost operator +(ResourceCost a, ResourceCost b)
    {
        return new ResourceCost(a.gold + b.gold, a.iron + b.iron, a.stone + b.stone, a.wood + b.wood, a.food + b.food, a.fuel + b.fuel);
    }
}
