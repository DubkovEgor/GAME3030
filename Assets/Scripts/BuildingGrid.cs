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
    public List<RandomSpawnEntry> randomSpawnPrefabs = new List<RandomSpawnEntry>();

    public Text messageText;
    [SerializeField] public Transform spawnedObjectsParent;

    private int gridWidth => GridSize.x;
    private int gridHeight => GridSize.y;
    private float cellSize => 2f;

   //  private Vector2Int? pathStartCell = null;
    private Building pathPrefab;
    private List<Vector2Int> previewCells = new List<Vector2Int>();

    private void Awake()
    {
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

        Vector3 spawnPos = GetMouseWorldPosition();
        flyingBuilding = Instantiate(buildingPrefab, spawnPos, Quaternion.identity);
        lastBuildingPrefab = buildingPrefab;

        currentRotation = 0;
        ApplyRotation();
    }

    private void Update()
    {
        HandleBuilding();
        
        //if (flyingBuilding != null && flyingBuilding.CompareTag("Path"))
        //{
        //    pathPrefab = flyingBuilding; // set current path prefab
        //    HandlePathPlacement();
        //}
        //else
        //{
        //    HandleBuilding();
        //}
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



    public void ClearScene()
    {
        foreach (var building in FindObjectsOfType<Building>())
        {
            if (building.CompareTag("Building"))
                Destroy(building.gameObject);
        }

        foreach (var path in GameObject.FindGameObjectsWithTag("Path"))
        {
            Destroy(path);
        }
        foreach (var path in GameObject.FindGameObjectsWithTag("Decoration"))
        {
            Destroy(path);
        }

        foreach (var npc in GameObject.FindGameObjectsWithTag("NPC"))
        {
            Destroy(npc);
        }

        grid = new Building[GridSize.x, GridSize.y];

        flyingBuilding = null;
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.gold = 1000;
            EconomyManager.Instance.stone = 500;
            EconomyManager.Instance.wood = 500;
            EconomyManager.Instance.housing = 0;
            EconomyManager.Instance.currentNPCs = 0;

            EconomyManager.Instance.goldPerHour = 0;
            EconomyManager.Instance.woodPerHour = 0;
            EconomyManager.Instance.stonePerHour = 0;

            EconomyManager.Instance.NotifyResourcesChanged();
        }
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
        foreach (var entry in randomSpawnPrefabs)
        {
            int count = Random.Range(entry.countMinSpawn, entry.countMaxSpawn + 1);

            for (int i = 0; i < count; i++)
            {
                TrySpawnRandomPrefab(entry.prefab);
            }
        }
    }

    void TrySpawnRandomPrefab(GameObject prefab)
    {
        Building b = prefab.GetComponent<Building>();
        if (b == null)
            return;

        Vector2Int size = b.Size;
        int tries = 50;

        while (tries-- > 0)
        {
            int x = Random.Range(0, gridWidth - size.x + 1);
            int y = Random.Range(0, gridHeight - size.y + 1);

            if (AreaIsFree(x, y, size))
            {
                Vector3 pos = GetCellCenter(x, y);



                GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
                if (spawnedObjectsParent != null)
                    obj.transform.parent = spawnedObjectsParent;
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



    //private void HandlePathPlacement()
    //{
    //    if (pathPrefab == null) return;

    //    if (EventSystem.current.IsPointerOverGameObject()) return;

    //    var groundPlane = new Plane(Vector3.up, Vector3.zero);
    //    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
    //    if (!groundPlane.Raycast(ray, out float distance)) return;

    //    Vector3 worldPos = ray.GetPoint(distance);
    //    int x = Mathf.RoundToInt((worldPos.x - gridOrigin.x) / cellSize);
    //    int y = Mathf.RoundToInt((worldPos.z - gridOrigin.z) / cellSize);
    //    Vector2Int currentCell = new Vector2Int(x, y);

    //    // First click: set start point
    //    if (Input.GetMouseButtonDown(0) && pathStartCell == null)
    //    {
    //        pathStartCell = currentCell;
    //        previewCells.Clear();
    //    }
    //    // Second click: place all tiles along line
    //    else if (Input.GetMouseButtonDown(0) && pathStartCell != null)
    //    {
    //        Vector2Int start = pathStartCell.Value;
    //        Vector2Int end = currentCell;

    //        List<Vector2Int> cellsToPlace = GetCellsLine(start, end);

    //        foreach (var cell in cellsToPlace)
    //        {
    //            if (AreaIsFree(cell.x, cell.y, pathPrefab.Size))
    //            {
    //                PlacePathTile(cell.x, cell.y);
    //            }
    //        }

    //        pathStartCell = null;
    //        previewCells.Clear();
    //    }
    //    else if (pathStartCell != null)
    //    {
    //        // Update preview cells along line from start to current mouse
    //        previewCells = GetCellsLine(pathStartCell.Value, currentCell);
    //    }
    //}

    //private void OnDrawGizmosSelected()
    //{
    //    if (previewCells == null) return;

    //    Gizmos.color = new Color(0, 1, 1, 0.5f);
    //    foreach (var cell in previewCells)
    //    {
    //        Vector3 pos = gridOrigin + new Vector3(cell.x * cellSize + cellSize * 0.5f, 0.05f, cell.y * cellSize + cellSize * 0.5f);
    //        Gizmos.DrawCube(pos, new Vector3(cellSize, 0.1f, cellSize));
    //    }
    //}

    //private List<Vector2Int> GetCellsLine(Vector2Int start, Vector2Int end)
    //{
    //    List<Vector2Int> cells = new List<Vector2Int>();

    //    int dx = Mathf.Abs(end.x - start.x);
    //    int dy = Mathf.Abs(end.y - start.y);

    //    int sx = start.x < end.x ? 1 : -1;
    //    int sy = start.y < end.y ? 1 : -1;

    //    int err = dx - dy;
    //    int x = start.x;
    //    int y = start.y;

    //    while (true)
    //    {
    //        cells.Add(new Vector2Int(x, y));
    //        if (x == end.x && y == end.y) break;
    //        int e2 = 2 * err;
    //        if (e2 > -dy) { err -= dy; x += sx; }
    //        if (e2 < dx) { err += dx; y += sy; }
    //    }

    //    return cells;
    //}

    //private void PlacePathTile(int x, int y)
    //{
    //    GameObject obj = Instantiate(pathPrefab.gameObject,
    //        gridOrigin + new Vector3(x * cellSize + cellSize * 0.5f, 0, y * cellSize + cellSize * 0.5f),
    //        Quaternion.identity);

    //    if (spawnedObjectsParent != null)
    //        obj.transform.parent = spawnedObjectsParent;

    //    Building b = obj.GetComponent<Building>();
    //    for (int dx = 0; dx < b.CurrentSize.x; dx++)
    //        for (int dy = 0; dy < b.CurrentSize.y; dy++)
    //            grid[x + dx, y + dy] = b;

    //    b.OnPlaced();
    //}

    private Vector3 GetCellCenter(int x, int y)
    {
        return gridOrigin + new Vector3(
            (x + 0.5f) * cellSize,
            0,
            (y + 0.5f) * cellSize
        );
    }

}

