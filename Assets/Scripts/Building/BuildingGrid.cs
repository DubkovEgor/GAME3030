using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class BuildingsGrid : MonoBehaviour
{
    public Vector2Int GridSize = new Vector2Int(10, 10);

    private Building[,] grid;
    private Building flyingBuilding;
    private Camera mainCamera;
    private Vector3 gridOrigin;
    private int currentRotation = 0; 
    private Building lastBuildingPrefab;
    public List<SpawnGroup> spawnGroups = new List<SpawnGroup>();

    public Text messageText;
    [SerializeField] public Transform spawnedObjectsParent;

    private int gridWidth => GridSize.x;
    private int gridHeight => GridSize.y;
    private float cellSize => 2f;

    private List<Vector2Int> pathWaypoints = new List<Vector2Int>();
    private List<Building> pathGhosts = new List<Building>();
    public static BuildingsGrid Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
        grid = new Building[GridSize.x, GridSize.y];
        mainCamera = Camera.main;

        gridOrigin = transform.position - new Vector3(
            GridSize.x * cellSize * 0.5f,
            0,
            GridSize.y * cellSize * 0.5f
        );
    }

    void Start()
    {
        GenerateRandomMap();
    }
    public void StartPlacingBuilding(Building buildingPrefab)
    {
        if (!EconomyManager.Instance.CanAfford(buildingPrefab.Cost))
        {
            if (messageText != null)
            {
                messageText.text = "Not enough resources!";
                CancelInvoke(nameof(ClearMessage));
                Invoke(nameof(ClearMessage), 2f);
            }
            Debug.Log("Not enough resources to place this building!");
            return;
        }

        if (flyingBuilding != null)
            Destroy(flyingBuilding.gameObject);

        ClearPathGhosts();
        pathWaypoints.Clear();

        Vector3 spawnPos = GetMouseWorldPosition();
        flyingBuilding = Instantiate(buildingPrefab, spawnPos, Quaternion.identity);
        lastBuildingPrefab = buildingPrefab;

        currentRotation = 0;
        ApplyRotation();
    }

    private void Update()
    {
        if (flyingBuilding != null && flyingBuilding.CompareTag("Path"))
            HandlePathPlacement();
        else
            HandleBuilding();
    }


    private void HandleBuilding()
    {
        if (flyingBuilding == null) return;
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            Destroy(flyingBuilding.gameObject);
            flyingBuilding = null;
            return;
        }

        HandleRotation();

        var groundPlane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (!groundPlane.Raycast(ray, out float distance)) return;

        Vector3 worldPos = ray.GetPoint(distance);
        flyingBuilding.transform.position = new Vector3(worldPos.x, 0, worldPos.z);
        float cellSize = flyingBuilding.CellSize;
        Vector2Int size = flyingBuilding.CurrentSize;

        int baseX = Mathf.FloorToInt((worldPos.x - gridOrigin.x) / cellSize);
        int baseY = Mathf.FloorToInt((worldPos.z - gridOrigin.z) / cellSize);


        int offsetX = 0, offsetY = 0;
        switch (currentRotation)
        {
            case 0: offsetX = 0; offsetY = 0; break;
            case 90: offsetX = 0; offsetY = flyingBuilding.CurrentSize.y - 1; break;
            case 180: offsetX = flyingBuilding.CurrentSize.x - 1; offsetY = flyingBuilding.CurrentSize.y - 1; break;
            case 270: offsetX = flyingBuilding.CurrentSize.x - 1; offsetY = 0; break;
        }

        int gridX = baseX - offsetX;
        int gridY = baseY - offsetY;

        Vector3 snappedPos = gridOrigin + new Vector3(gridX * cellSize + size.x * 0.5f * cellSize,
            0,gridY * cellSize + size.y * 0.5f * cellSize);
        flyingBuilding.transform.position = snappedPos;

        bool available = true;
        if (gridX < 0 || gridX + flyingBuilding.CurrentSize.x > GridSize.x) available = false;
        if (gridY < 0 || gridY + flyingBuilding.CurrentSize.y > GridSize.y) available = false;
        if (available && IsPlaceTaken(gridX, gridY)) available = false;

        flyingBuilding.transform.position = GetCellCenter(baseX, baseY);


        flyingBuilding.SetTransparent(available);

        if (available && Input.GetMouseButtonDown(0))
        {
            if (EconomyManager.Instance.SpendResources(flyingBuilding.Cost))
                PlaceFlyingBuilding(gridX, gridY);
        }
    }
    private void HandleRotation()
    {
        if (!Input.GetKey(KeyCode.R)) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) currentRotation = (currentRotation + 90) % 360;
        else if (scroll < 0f) currentRotation = (currentRotation - 90 + 360) % 360;

        ApplyRotation();
    }

    private void ApplyRotation()
    {
        if (flyingBuilding == null) return;

        flyingBuilding.transform.rotation = Quaternion.Euler(0, currentRotation, 0);

        if (currentRotation % 180 == 0)
            flyingBuilding.CurrentSize = flyingBuilding.Size;
        else
            flyingBuilding.CurrentSize = new Vector2Int(flyingBuilding.Size.y, flyingBuilding.Size.x);
    }

    private bool IsPlaceTaken(int x, int y)
    {
        for (int dx = 0; dx < flyingBuilding.CurrentSize.x; dx++)
        {
            for (int dy = 0; dy < flyingBuilding.CurrentSize.y; dy++)
            {
                if (grid[x + dx, y + dy] != null) return true;
            }
        }
        return false;
    }

    private void PlaceFlyingBuilding(int x, int y)
    {
        Building placedBuilding = flyingBuilding;

        for (int dx = 0; dx < flyingBuilding.CurrentSize.x; dx++)
        {
            for (int dy = 0; dy < flyingBuilding.CurrentSize.y; dy++)
            {
                grid[x + dx, y + dy] = flyingBuilding;
            }
        }

        placedBuilding.OnPlaced();
        placedBuilding.SetNormal();
        SoundManager.Instance.PlaySFX("Place");

        flyingBuilding = null;

        StartPlacingBuilding(lastBuildingPrefab);
    }

    private void HandlePathPlacement()
    {
        if (flyingBuilding == null) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(1))
        {
            CancelPathPlacement();
            return;
        }

        var groundPlane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!groundPlane.Raycast(ray, out float distance)) return;

        Vector3 worldPos = ray.GetPoint(distance);
        int mouseX = Mathf.Clamp(Mathf.FloorToInt((worldPos.x - gridOrigin.x) / cellSize), 0, GridSize.x - 1);
        int mouseY = Mathf.Clamp(Mathf.FloorToInt((worldPos.z - gridOrigin.z) / cellSize), 0, GridSize.y - 1);
        Vector2Int mouseCell = new Vector2Int(mouseX, mouseY);

        if (pathWaypoints.Count == 0)
        {
            flyingBuilding.transform.position = GetCellCenter(mouseX, mouseY);
            flyingBuilding.SetTransparent(grid[mouseX, mouseY] == null);

            if (Input.GetMouseButtonDown(0))
            {
                pathWaypoints.Add(mouseCell);               
                flyingBuilding.gameObject.SetActive(false); 
            }

            return;
        }

        List<Vector2Int> allCells = GetAllSegmentCells(pathWaypoints, mouseCell);
        UpdatePathGhosts(allCells);

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (mouseCell != pathWaypoints[pathWaypoints.Count - 1])
                pathWaypoints.Add(mouseCell);
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            PlacePathTiles(allCells);

            pathWaypoints.Clear();
            ClearPathGhosts();

            Building prefab = lastBuildingPrefab;
            Destroy(flyingBuilding.gameObject);
            flyingBuilding = null;
            StartPlacingBuilding(prefab);
        }
    }
    private List<Vector2Int> GetAllSegmentCells(List<Vector2Int> waypoints, Vector2Int liveEnd)
    {
        var result = new List<Vector2Int>();
        var visited = new HashSet<Vector2Int>();

        for (int i = 0; i < waypoints.Count - 1; i++)
            AddSegment(waypoints[i], waypoints[i + 1], result, visited);

        AddSegment(waypoints[waypoints.Count - 1], liveEnd, result, visited);

        return result;
    }
    private void TryAdd(Vector2Int cell, List<Vector2Int> result, HashSet<Vector2Int> visited)
    {
        if (visited.Add(cell))
            result.Add(cell);
    }
    private void AddSegment(Vector2Int a, Vector2Int b, List<Vector2Int> result, HashSet<Vector2Int> visited)
    {
        int dx = b.x - a.x;
        int dy = b.y - a.y;

        if (Mathf.Abs(dx) >= Mathf.Abs(dy))
        {
            int step = dx >= 0 ? 1 : -1;
            for (int x = a.x; x != b.x + step; x += step)
                TryAdd(new Vector2Int(x, a.y), result, visited);
        }
        else
        {
            int step = dy >= 0 ? 1 : -1;
            for (int y = a.y; y != b.y + step; y += step)
                TryAdd(new Vector2Int(a.x, y), result, visited);
        }
    }
    private List<Vector2Int> GetAxisAlignedCells(Vector2Int a, Vector2Int b)
    {
        List<Vector2Int> cells = new List<Vector2Int>();

        int dx = b.x - a.x;
        int dy = b.y - a.y;

        if (Mathf.Abs(dx) >= Mathf.Abs(dy))
        {
            int step = dx >= 0 ? 1 : -1;
            for (int x = a.x; x != b.x + step; x += step)
                cells.Add(new Vector2Int(x, a.y));
        }
        else
        {
            int step = dy >= 0 ? 1 : -1;
            for (int y = a.y; y != b.y + step; y += step)
                cells.Add(new Vector2Int(a.x, y));
        }

        return cells;
    }

    private void UpdatePathGhosts(List<Vector2Int> cells)
    {
        if (pathGhosts.Count != cells.Count)
        {
            ClearPathGhosts();
            for (int i = 0; i < cells.Count; i++)
                pathGhosts.Add(Instantiate(lastBuildingPrefab, Vector3.zero, Quaternion.identity));
        }

        int affordable = GetAffordableTileCount(lastBuildingPrefab.Cost);

        for (int i = 0; i < pathGhosts.Count; i++)
        {
            Vector2Int cell = cells[i];
            bool inBounds = cell.x >= 0 && cell.x < GridSize.x && cell.y >= 0 && cell.y < GridSize.y;
            bool free = inBounds && grid[cell.x, cell.y] == null;
            bool canAfford = i < affordable;

            pathGhosts[i].transform.position = GetCellCenter(cell.x, cell.y);
            pathGhosts[i].SetTransparent(free && canAfford);
        }
    }

    private void PlacePathTiles(List<Vector2Int> cells)
    {
        foreach (var cell in cells)
        {
            if (cell.x < 0 || cell.x >= GridSize.x || cell.y < 0 || cell.y >= GridSize.y) continue;
            if (grid[cell.x, cell.y] != null) continue;
            if (!EconomyManager.Instance.CanAfford(lastBuildingPrefab.Cost)) break;
            if (!EconomyManager.Instance.SpendResources(lastBuildingPrefab.Cost)) break;

            Building tile = Instantiate(lastBuildingPrefab, GetCellCenter(cell.x, cell.y), Quaternion.identity);
            if (spawnedObjectsParent != null) tile.transform.parent = spawnedObjectsParent;
            grid[cell.x, cell.y] = tile;
            tile.OnPlaced();
            tile.SetNormal();
        }

        SoundManager.Instance.PlaySFX("Place");
    }


    private int GetAffordableTileCount(ResourceCost cost)
    {
        int max = int.MaxValue;

        if (cost.gold > 0) max = Mathf.Min(max, EconomyManager.Instance.gold / cost.gold);
        if (cost.wood > 0) max = Mathf.Min(max, EconomyManager.Instance.wood / cost.wood);
        if (cost.stone > 0) max = Mathf.Min(max, EconomyManager.Instance.stone / cost.stone);
        if (cost.food > 0) max = Mathf.Min(max, EconomyManager.Instance.food / cost.food);
        if (cost.iron > 0) max = Mathf.Min(max, EconomyManager.Instance.iron / cost.iron);
        if (cost.fuel > 0) max = Mathf.Min(max, EconomyManager.Instance.fuel / cost.fuel);

        return max == int.MaxValue ? int.MaxValue : max;
    }

    private void CancelPathPlacement()
    {
        pathWaypoints.Clear();
        ClearPathGhosts();

        if (flyingBuilding != null)
        {
            Destroy(flyingBuilding.gameObject);
            flyingBuilding = null;
        }
    }


    private void ClearPathGhosts()
    {
        foreach (var ghost in pathGhosts)
            if (ghost != null) Destroy(ghost.gameObject);

        pathGhosts.Clear();
    }

    public void ClearScene()
    {
        

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.gold = 1000;
            EconomyManager.Instance.stone = 500;
            EconomyManager.Instance.wood = 500;
            EconomyManager.Instance.food = 2500;
            EconomyManager.Instance.fuel = 500;
            EconomyManager.Instance.iron = 500;

            EconomyManager.Instance.goldPerHour = 0;
            EconomyManager.Instance.woodPerHour = 0;
            EconomyManager.Instance.stonePerHour = 0;
            EconomyManager.Instance.foodPerHour = 0;
            EconomyManager.Instance.fuelPerHour = 0;
            EconomyManager.Instance.ironPerHour = 0;

            EconomyManager.Instance.housing = 0;
            EconomyManager.Instance.currentNPCs = 0;
            EconomyManager.Instance.currentIdleNPCs = 0;

            EconomyManager.Instance.woodWorkers = 0;
            EconomyManager.Instance.stoneWorkers = 0;
            EconomyManager.Instance.foodWorkers = 0;
            EconomyManager.Instance.ironWorkers = 0;
            EconomyManager.Instance.goldWorkers = 0;
            EconomyManager.Instance.fuelWorkers = 0;

            EconomyManager.Instance.currentWoodWorkers = 0;
            EconomyManager.Instance.currentStoneWorkers = 0;
            EconomyManager.Instance.currentFoodWorkers = 0;
            EconomyManager.Instance.currentIronWorkers = 0;
            EconomyManager.Instance.currentGoldWorkers = 0;
            EconomyManager.Instance.currentFuelWorkers = 0;

            EconomyManager.Instance.NotifyResourcesChanged();
        }

        foreach (var building in FindObjectsOfType<Building>())
        {
            if (building.CompareTag("Building"))
                Destroy(building.gameObject);
        }

        foreach (var path in GameObject.FindGameObjectsWithTag("Path"))
            Destroy(path);

        foreach (var path in GameObject.FindGameObjectsWithTag("Decoration"))
            Destroy(path);

        foreach (var npc in GameObject.FindGameObjectsWithTag("NPC"))
            Destroy(npc);

        grid = new Building[GridSize.x, GridSize.y];
        flyingBuilding = null;
        pathWaypoints.Clear();
        ClearPathGhosts();
        if (Market.Instance != null)
        {
            foreach (var res in Market.Instance.GetAll())
                res.currentPriceRate = 1f;

            Market.Instance.NotifyRatesChanged();
        }
        if (WeatherSystem.Instance != null)
            WeatherSystem.Instance.ResetToStartDate();
        GenerateRandomMap();
    }
    public Vector3 GetMouseWorldPosition()
    {
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }

    void GenerateRandomMap()
    {
        Dictionary<string, Vector2Int> groupAnchors = new Dictionary<string, Vector2Int>();

        foreach (var group in spawnGroups)
        {
            if (!string.IsNullOrEmpty(group.groupTag) && !groupAnchors.ContainsKey(group.groupTag))
            {
                groupAnchors[group.groupTag] = new Vector2Int(
                    Random.Range(group.groupRadius, gridWidth - group.groupRadius),
                    Random.Range(group.groupRadius, gridHeight - group.groupRadius)
                );
            }
        }

        foreach (var group in spawnGroups)
        {
            Vector2Int? anchor = null;
            if (!string.IsNullOrEmpty(group.groupTag) && groupAnchors.ContainsKey(group.groupTag))
                anchor = groupAnchors[group.groupTag];

            foreach (var entry in group.prefabs)
            {
                int count = Random.Range(entry.minSpawn, entry.maxSpawn + 1);
                for (int i = 0; i < count; i++)
                    TrySpawnRandomPrefab(entry.prefab, anchor, group.groupRadius);
            }
        }
    }

    void TrySpawnRandomPrefab(GameObject prefab, Vector2Int? anchor, int groupRadius)
    {
        Building b = prefab.GetComponent<Building>();
        if (b == null) return;

        Vector2Int size = b.Size;
        int tries = 50;

        while (tries-- > 0)
        {
            int x, y;

            if (anchor.HasValue)
            {
                x = Mathf.Clamp(anchor.Value.x + Random.Range(-groupRadius, groupRadius + 1), 0, gridWidth - size.x);
                y = Mathf.Clamp(anchor.Value.y + Random.Range(-groupRadius, groupRadius + 1), 0, gridHeight - size.y);
            }
            else
            {
                x = Random.Range(0, gridWidth - size.x + 1);
                y = Random.Range(0, gridHeight - size.y + 1);
            }

            if (AreaIsFree(x, y, size))
            {
                GameObject obj = Instantiate(prefab, GetCellCenter(x, y), Quaternion.identity);
                if (spawnedObjectsParent != null) obj.transform.parent = spawnedObjectsParent;

                Building instance = obj.GetComponent<Building>();
                for (int dx = 0; dx < size.x; dx++)
                    for (int dy = 0; dy < size.y; dy++)
                        grid[x + dx, y + dy] = instance;

                instance.OnPlaced();
                return;
            }
        }
    }
    bool AreaIsFree(int x, int y, Vector2Int size)
    {
        for (int dx = 0; dx < size.x; dx++)
            for (int dy = 0; dy < size.y; dy++)
                if (grid[x + dx, y + dy] != null)
                    return false;

        return true;
    }





    private void ClearMessage()
    {
        if (messageText != null)
            messageText.text = "";
    }
    private void OnDrawGizmos()
    {
        if (grid == null) return;

        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                if (grid[x, y] != null)
                    Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
                else
                    Gizmos.color = new Color(1f, 1f, 1f, 0.05f);

                Vector3 cellCenter = GetCellCenter(x, y);
                Gizmos.DrawCube(cellCenter, new Vector3(cellSize, 0.1f, cellSize));
            }
        }
    }
    private Vector3 GetCellCenter(int x, int y)
    {
        return gridOrigin + new Vector3(
            (x + 0.5f) * cellSize,
            0,
            (y + 0.5f) * cellSize
        );
    }
    public (Building[,] grid, Vector3 origin, float cell, Vector2Int size) GetGridData()
    {
        return (grid, gridOrigin, cellSize, GridSize);
    }

    [System.Serializable]
    public class RandomSpawnEntry
    {
        public GameObject prefab;
        public int minSpawn;
        public int maxSpawn;
    }

    [System.Serializable]
    public class SpawnGroup
    {
        public string groupTag;
        public int groupRadius = 3;
        public List<RandomSpawnEntry> prefabs = new List<RandomSpawnEntry>();
    }
}

