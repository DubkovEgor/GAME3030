using System.Collections.Generic;
using UnityEngine;

public class ResourceDropOff : MonoBehaviour
{
    [Header("Accepted Resources (empty = accepts all)")]
    public List<ResourceType> acceptedTypes = new();

    public bool Accepts(ResourceType type)
    {
        return acceptedTypes.Count == 0 || acceptedTypes.Contains(type);
    }

    public void Deposit(ResourceType type, int amount)
    {
        if (!Accepts(type)) return;

        switch (type)
        {
            case ResourceType.Food: EconomyManager.Instance.food += amount; break;
            case ResourceType.Wood: EconomyManager.Instance.wood += amount; break;
            case ResourceType.Stone: EconomyManager.Instance.stone += amount; break;
            case ResourceType.Iron: EconomyManager.Instance.iron += amount; break;
            case ResourceType.Gold: EconomyManager.Instance.gold += amount; break;
            case ResourceType.Fuel: EconomyManager.Instance.fuel += amount; break;
        }

        EconomyManager.Instance.NotifyResourcesChanged();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.6f);
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}