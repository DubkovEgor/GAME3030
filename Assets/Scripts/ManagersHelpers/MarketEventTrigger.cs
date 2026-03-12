using UnityEngine;

public class MarketEventTrigger : MonoBehaviour
{
    // example
    public void Drought()
    {
        Market.Instance.ApplyExternalShift(Market.Instance.food, +0.35f);
        Market.Instance.ApplyExternalShift(Market.Instance.wood, +0.15f);
    }

    public void ApplyShift(string resourceName, float delta)
    {
        var res = Market.Instance.GetByName(resourceName);
        if (res == null) return;
        Market.Instance.ApplyExternalShift(res, delta);
    }
}