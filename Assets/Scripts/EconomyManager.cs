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
    public int currentNPCs = 0;

    [Header("Resource generation per second")]
    public float goldPerSec;
    public float ironPerSec;
    public float stonePerSec;
    public float woodPerSec;
    public float foodPerSec;
    public float fuelPerSec;

    public event Action OnResourcesChanged;
    public event Action OnWin;

    private float goldAccum;
    private float ironAccum;
    private float stoneAccum;
    private float woodAccum;
    private float foodAccum;
    private float fuelAccum;
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

    private void Update()
    {
        float delta = Time.deltaTime;

        goldAccum += goldPerSec * delta;
        ironAccum += ironPerSec * delta;
        stoneAccum += stonePerSec * delta;
        woodAccum += woodPerSec * delta;
        foodAccum += foodPerSec * delta; 
        fuelAccum += fuelPerSec * delta;

        bool changed = false;

        if (goldAccum >= 1f || goldAccum <= -1f)
        {
            int add = Mathf.FloorToInt(goldAccum);
            gold += add;
            goldAccum -= add;
            changed = true;
        }

        if (ironAccum >= 1f || ironAccum <= -1f)
        {
            int add = Mathf.FloorToInt(ironAccum);
            iron += add;
            ironAccum -= add;
            changed = true;
        }

        if (stoneAccum >= 1f || stoneAccum <= -1f)
        {
            int add = Mathf.FloorToInt(stoneAccum);
            stone += add;
            stoneAccum -= add;
            changed = true;
        }

        if (woodAccum >= 1f || woodAccum <= -1f)
        {
            int add = Mathf.FloorToInt(woodAccum);
            wood += add;
            woodAccum -= add;
            changed = true;
        }

        if (foodAccum >= 1f || foodAccum <= -1f)
        {
            int add = Mathf.FloorToInt(foodAccum);
            food += add;
            foodAccum -= add;
            changed = true;
        }
        if (fuelAccum >= 1f || fuelAccum <= -1f)
        {
            int add = Mathf.FloorToInt(fuelAccum);
            fuel += add;
            fuelAccum -= add;
            changed = true;
        }

        if (changed)
            OnResourcesChanged?.Invoke();
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
