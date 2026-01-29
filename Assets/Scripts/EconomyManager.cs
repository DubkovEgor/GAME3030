using UnityEngine;
using System;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    [Header("Current Resources")]
    public int gold = 1000;
    public int stone = 500;
    public int wood = 500;
    public int housing = 0;
    public int currentNPCs = 0;

    [Header("Resource generation per second")]
    public float goldPerSec;
    public float stonePerSec;
    public float woodPerSec;

    public event Action OnResourcesChanged;
    public event Action OnWin;

    private float goldAccum;
    private float stoneAccum;
    private float woodAccum;
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
        stoneAccum += stonePerSec * delta;
        woodAccum += woodPerSec * delta;

        bool changed = false;

        if (goldAccum >= 1f || goldAccum <= -1f)
        {
            int add = Mathf.FloorToInt(goldAccum);
            gold += add;
            goldAccum -= add;
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

        if (changed)
            OnResourcesChanged?.Invoke();
        CheckWinCondition();
    }

    public bool CanAfford(ResourceCost cost)
    {
        return gold >= cost.gold && stone >= cost.stone && wood >= cost.wood;
    }

    public bool SpendResources(ResourceCost cost)
    {
        if (!CanAfford(cost)) return false;

        gold -= cost.gold;
        stone -= cost.stone;
        wood -= cost.wood;
        OnResourcesChanged?.Invoke();
        return true;
    }

    public void AddResources(ResourceCost cost)
    {
        gold += cost.gold;
        stone += cost.stone;
        wood += cost.wood;
        OnResourcesChanged?.Invoke();
    }

    public void Add1000Resources()
    {
        ResourceCost bonus = new ResourceCost(1000, 1000, 1000);
        AddResources(bonus);
    }

    public void NotifyResourcesChanged()
    {
        OnResourcesChanged?.Invoke();
    }
    private void CheckWinCondition()
    {
        if (!winTriggered && currentNPCs >= 150)
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

    //public void IncreaseDifficulty()
    //{
    //    GameObject[] allBuildings = GameObject.FindGameObjectsWithTag("Building");
    //    foreach (var obj in allBuildings)
    //    {
    //        Building building = obj.GetComponent<Building>();
    //        if (building != null && building.HousingProvided > 0)
    //        {
    //            building.Cost.gold *= 2;
    //            building.Cost.stone *= 2;
    //            building.Cost.wood *= 2;
    //        }
    //    }
    //}

}

[System.Serializable]
public struct ResourceCost
{
    public int gold;
    public int stone;
    public int wood;

    public ResourceCost(int gold, int stone, int wood)
    {
        this.gold = gold;
        this.stone = stone;
        this.wood = wood;
    }

    public static ResourceCost operator +(ResourceCost a, ResourceCost b)
    {
        return new ResourceCost(a.gold + b.gold, a.stone + b.stone, a.wood + b.wood);
    }
}
