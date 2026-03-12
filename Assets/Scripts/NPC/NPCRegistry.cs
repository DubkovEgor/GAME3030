using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NPCRegistry : MonoBehaviour
{
    public static NPCRegistry Instance { get; private set; }

    private readonly List<NPCMove> _all = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Register(NPCMove w) => _all.Add(w);
    public void Unregister(NPCMove w) => _all.Remove(w);

    public List<NPCMove> GetIdle()
        => _all.Where(w => !w.isAssigned).ToList();

    public List<NPCMove> GetWorkers(ResourceType type)
        => _all.Where(w => w.isAssigned && w.assignedResource == type).ToList();

    public void SetWorkerCount(ResourceType type, int count)
    {
        var current = GetWorkers(type);

        if (count > current.Count)
        {
            int needed = count - current.Count;
            var idle = GetIdle().Take(needed).ToList();
            foreach (var npc in idle) npc.AssignJob(type);
        }
        else if (count < current.Count)
        {
            int release = current.Count - count;
            foreach (var npc in current.Take(release)) npc.Unassign();
        }
    }
    public void UnassignAll(ResourceType type)
    {
        foreach (var npc in GetWorkers(type).ToList())
            npc.Unassign();
    }

    public void UnassignAll()
    {
        foreach (var npc in _all.Where(w => w.isAssigned).ToList())
            npc.Unassign();
    }
}