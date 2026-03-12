using System.Collections.Generic;
using UnityEngine;

public enum ResourceType { Food, Wood, Stone, Iron, Gold, Fuel }

public class ResourceGatheringPoint : MonoBehaviour
{
    [Header("Resource")]
    public ResourceType resourceType;
    public int totalAmount;
    public int amountPerGather;
    public int remaining;

    [Header("Access Points (child transforms)")]
    public List<Transform> accessPoints = new();

    [Header("Visuals (Later)")]
    public GameObject depletedVisual;
    public GameObject activeVisual;

    private HashSet<Transform> _occupiedPoints = new();

    public bool HasResources => remaining > 0;
    public bool HasCapacity => _occupiedPoints.Count < accessPoints.Count;

    private void Awake()
    {
        remaining = totalAmount;
    }

    private void Start()
    {
        GatheringPointManager.Instance?.Register(this);
        RefreshVisuals();
    }

    private void OnDestroy()
    {
        GatheringPointManager.Instance?.Unregister(this);
    }
    public Transform ReserveAccessPoint()
    {
        foreach (var point in accessPoints)
        {
            if (!_occupiedPoints.Contains(point))
            {
                _occupiedPoints.Add(point);
                return point;
            }
        }
        return null;
    }

    public void ReleaseAccessPoint(Transform point)
    {
        _occupiedPoints.Remove(point);
    }

    public int TryGather()
    {
        if (remaining <= 0) return 0;

        int taken = Mathf.Min(amountPerGather, remaining);
        remaining -= taken;
        RefreshVisuals();
        return taken;
    }

    private void RefreshVisuals()
    {
        bool active = HasResources;
        if (activeVisual) activeVisual.SetActive(active);
        if (depletedVisual) depletedVisual.SetActive(!active);

        if (!active)
            Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        foreach (var point in accessPoints)
        {
            if (point == null) continue;
            Gizmos.color = _occupiedPoints.Contains(point) ? Color.yellow : Color.green;
            Gizmos.DrawWireSphere(point.position, 0.4f);
            Gizmos.DrawLine(transform.position, point.position);
        }
    }
}