using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GatheringPointManager : MonoBehaviour
{
    public static GatheringPointManager Instance { get; private set; }

    [Header("Gathering Points")]
    public List<ResourceGatheringPoint> _allPoints = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Register(ResourceGatheringPoint point) => _allPoints.Add(point);
    public void Unregister(ResourceGatheringPoint point) => _allPoints.Remove(point);

    public ResourceGatheringPoint FindNearest(ResourceType type, Vector3 from)
    {
        return _allPoints
            .Where(p => p.resourceType == type && p.HasResources && p.HasCapacity)
            .OrderBy(p => Vector3.SqrMagnitude(p.transform.position - from))
            .FirstOrDefault();
    }

    public List<ResourceGatheringPoint> GetAll(ResourceType type)
    {
        return _allPoints
            .Where(p => p.resourceType == type && p.HasResources)
            .ToList();
    }
}