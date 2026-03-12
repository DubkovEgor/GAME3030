using UnityEngine;
using System.Collections.Generic;

public class NPCSpawner : MonoBehaviour
{
    public GameObject npcPrefab;
    public float spawnInterval = 10f;
    private float timer = 0f;

    private List<Transform> spawnPoints = new List<Transform>();

    private void Start()
    {
        UpdateSpawnPoints();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            FastSpawn();
        }
            UpdateSpawnPoints();
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            TrySpawnNPC();
        }
    }

    public void UpdateSpawnPoints()
    {
        spawnPoints.Clear();

        foreach (Building b in FindObjectsOfType<Building>())
        {
            Transform point = b.transform.Find("NPCSpawnPoint");
            if (point != null)
                spawnPoints.Add(point);
        }
    }

    private void TrySpawnNPC()
    {
        var eco = EconomyManager.Instance;
        if (eco == null || spawnPoints.Count == 0) return;

        if (eco.currentNPCs < eco.housing)
        {
            Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            Instantiate(npcPrefab, randomPoint.position, Quaternion.identity);
            eco.currentNPCs++;

            eco.NotifyResourcesChanged();
        }
    }
    public void FastSpawn()
    {
        spawnInterval = 0.1f;
    }

}
