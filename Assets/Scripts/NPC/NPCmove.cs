using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum NPCState
{
    Idle,
    Walking,
    GatheringWood,
    GatheringStone,
    GatheringGold,
    GatheringIron,
    GatheringFuel,
    GatheringBerries,
    GatheringCrops,
    GatheringMeat,
    CarryingResources
}

[RequireComponent(typeof(Collider))]
public class NPCMove : MonoBehaviour
{
    [Header("Movement")]
    public float baseMoveSpeed = 2f;
    public float moveSpeed = 2f;
    public float turnSpeed = 8f;
    public float minMoveTime = 1.0f;
    public float maxMoveTime = 4.0f;
    public float minIdleTime = 0.5f;
    public float maxIdleTime = 2.0f;
    public bool useRigidbody = false;

    [Header("Gathering")]
    public float gatherDuration = 2f;
    public float arrivalRadius = 1.5f;

    [Header("Carry Props")]
    public GameObject carryWood;
    public GameObject carryStone;
    public GameObject carryGold;
    public GameObject carryIron;
    public GameObject carryFuel;
    public GameObject carryBerries;
    public GameObject carryCrops;
    public GameObject carryMeat;

    [Header("Tools")]
    public GameObject toolWood;
    public GameObject toolStone;
    public GameObject toolGold;
    public GameObject toolIron;
    public GameObject toolFuel;
    public GameObject toolBerries;
    public GameObject toolCrops;
    public GameObject toolMeat;

    public bool isAssigned { get; private set; } = false;
    public ResourceType assignedResource { get; private set; }

    private NPCState _state = NPCState.Walking;
    public NPCState State => _state;

    private Rigidbody rb;
    private Animator animatorRef;
    private static readonly int AnimStateHash = Animator.StringToHash("State");

    private Vector3 moveDirection;
    private bool isMoving = false;
    private float pathMultiplier = 1f;
    private Coroutine stateCoroutine;

    private ResourceGatheringPoint targetPoint;
    private Transform targetAccessPoint;
    private ResourceDropOff targetDropOff;
    [SerializeField] private int carryAmount;

    [Header("Debug Info")]
    [SerializeField] private NPCState debugState;
    [SerializeField] private bool debugAssigned;
    [SerializeField] private ResourceType debugResource;

    [Header("Separation")]
    public float separationRadius = 1.2f;
    public float separationForce = 2f;

    private List<Vector3> _currentPath = new List<Vector3>();
    private Building[,] _cachedGrid;
    private Vector3 _cachedOrigin;
    private float _cachedCellSize;
    private Vector2Int _cachedGridSize;
    private bool _gridCached = false;

    private void Awake()
    {
        animatorRef = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("NPC");

        int npcLayer = LayerMask.NameToLayer("NPC");
        int decorationLayer = LayerMask.NameToLayer("Decoration");
        Physics.IgnoreLayerCollision(npcLayer, npcLayer, true);
        Physics.IgnoreLayerCollision(npcLayer, decorationLayer, true);

        if (useRigidbody)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
        }

        NPCRegistry.Instance?.Register(this);
        EconomyManager.Instance.currentIdleNPCs++;
        EconomyManager.Instance.NotifyResourcesChanged();

        EnterState(NPCState.Walking);
    }

    private void OnDestroy()
    {
        NPCRegistry.Instance?.Unregister(this);

        EconomyManager.Instance.currentNPCs--;
        if (isAssigned)
            ReleaseFromJob();
        else
            EconomyManager.Instance.currentIdleNPCs--;

        EconomyManager.Instance.NotifyResourcesChanged();
    }

    private void Update()
    {
        if (_state == NPCState.Walking || _state == NPCState.CarryingResources)
        {
            float targetSpeed = baseMoveSpeed * pathMultiplier;
            moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, Time.unscaledDeltaTime * 3f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PathTile tile = other.GetComponent<PathTile>();
        if (tile != null)
            pathMultiplier = tile.speedMultiplier;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PathTile>() != null)
            pathMultiplier = 1f;
    }

    private void RefreshGridCache()
    {
        (_cachedGrid, _cachedOrigin, _cachedCellSize, _cachedGridSize) =
            BuildingsGrid.Instance.GetGridData();
        _gridCached = true;
    }

    public void AssignJob(ResourceType type)
    {
        if (isAssigned) ReleaseFromJob();

        assignedResource = type;
        isAssigned = true;
        debugAssigned = true;
        debugResource = type;

        EconomyManager.Instance.currentIdleNPCs--;
        AddCurrentWorker(type, 1);
        EconomyManager.Instance.NotifyResourcesChanged();

        EnterState(GatherStateForType(type));
    }

    public void Unassign()
    {
        if (!isAssigned) return;
        ReleaseFromJob();
        EnterState(NPCState.Walking);
    }

    private void EnterState(NPCState next)
    {
        if (stateCoroutine != null) StopCoroutine(stateCoroutine);
        if (next != NPCState.CarryingResources)
            HideAllCarryProps();

        _state = next;
        debugState = next;

        bool isGathering = next == NPCState.GatheringWood || next == NPCState.GatheringStone ||
                           next == NPCState.GatheringGold || next == NPCState.GatheringIron ||
                           next == NPCState.GatheringFuel || next == NPCState.GatheringBerries ||
                           next == NPCState.GatheringCrops || next == NPCState.GatheringMeat;

        if (isGathering) ShowTool(assignedResource);
        else HideAllTools();

        if (!isGathering) UpdateAnimator();

        switch (next)
        {
            case NPCState.Idle:
                break;

            case NPCState.Walking:
                stateCoroutine = StartCoroutine(WanderLoop());
                break;

            case NPCState.GatheringWood:
            case NPCState.GatheringStone:
            case NPCState.GatheringGold:
            case NPCState.GatheringIron:
            case NPCState.GatheringFuel:
            case NPCState.GatheringBerries:
            case NPCState.GatheringCrops:
            case NPCState.GatheringMeat:
                stateCoroutine = StartCoroutine(GatherRoutine());
                break;

            case NPCState.CarryingResources:
                ShowCarryProp(assignedResource);
                stateCoroutine = StartCoroutine(CarryRoutine());
                break;
        }
    }

    private IEnumerator WanderLoop()
    {
        while (true)
        {
            moveDirection = RandomDirectionXZ();
            isMoving = true;
            _state = NPCState.Walking;
            debugState = NPCState.Walking;
            UpdateAnimator();

            float moveTime = Random.Range(minMoveTime, maxMoveTime);
            float t = 0f;
            while (t < moveTime)
            {
                t += Time.deltaTime;
                StepMove();
                ApplySeparation();
                yield return null;
            }

            isMoving = false;
            _state = NPCState.Idle;
            debugState = NPCState.Idle;
            UpdateAnimator();

            while (moveSpeed > 0.05f)
            {
                moveSpeed = Mathf.Lerp(moveSpeed, 0f, Time.deltaTime * 8f);
                yield return null;
            }
            moveSpeed = 0f;

            yield return new WaitForSeconds(Random.Range(minIdleTime, maxIdleTime));
        }
    }
    private IEnumerator GatherRoutine()
    {
        HideAllTools();
        _state = NPCState.Walking;
        debugState = NPCState.Walking;
        UpdateAnimator();

        targetPoint = GatheringPointManager.Instance.FindNearest(assignedResource, transform.position);

        if (targetPoint == null)
        {
            SetIdleAndWait();
            yield return new WaitForSeconds(3f);
            if (isAssigned) EnterState(GatherStateForType(assignedResource));
            yield break;
        }

        targetAccessPoint = targetPoint.ReserveAccessPoint();
        if (targetAccessPoint == null)
        {
            SetIdleAndWait();
            yield return new WaitForSeconds(2f);
            if (isAssigned) EnterState(GatherStateForType(assignedResource));
            yield break;
        }

        yield return StartCoroutine(WalkTo(targetAccessPoint.position));

        if (targetPoint == null || !targetPoint.gameObject.activeInHierarchy)
        {
            if (targetAccessPoint != null && targetPoint != null)
                targetPoint.ReleaseAccessPoint(targetAccessPoint);
            targetAccessPoint = null;
            targetPoint = null;
            if (isAssigned) EnterState(GatherStateForType(assignedResource));
            yield break;
        }

        ShowTool(assignedResource);
        _state = GatherStateForType(assignedResource);
        debugState = _state;
        UpdateAnimator();
        yield return new WaitForSeconds(gatherDuration);

        HideAllTools();

        carryAmount = targetPoint.TryGather();
        targetPoint.ReleaseAccessPoint(targetAccessPoint);
        targetAccessPoint = null;

        if (carryAmount > 0)
            EnterState(NPCState.CarryingResources);
        else if (isAssigned)
            EnterState(GatherStateForType(assignedResource));
    }

    private IEnumerator CarryRoutine()
    {
        targetDropOff = FindNearestDropOff(assignedResource);

        if (targetDropOff == null)
        {
            yield return new WaitForSeconds(3f);
            if (isAssigned) EnterState(GatherStateForType(assignedResource));
            yield break;
        }

        yield return StartCoroutine(WalkTo(targetDropOff.transform.position));

        targetDropOff.Deposit(assignedResource, carryAmount);
        carryAmount = 0;
        HideAllCarryProps();

        if (isAssigned) EnterState(GatherStateForType(assignedResource));
        else EnterState(NPCState.Walking);
    }
    private IEnumerator WalkTo(Vector3 destination)
    {
        RefreshGridCache();

        _currentPath = AStarPathfinder.FindPath(
            transform.position, destination,
            _cachedGrid, _cachedOrigin, _cachedCellSize, _cachedGridSize);

        if (_currentPath == null || _currentPath.Count == 0)
        {
            yield return StartCoroutine(WalkStraightTo(destination));
            yield break;
        }

        yield return StartCoroutine(WalkPath());
    }

    private IEnumerator WalkPath()
    {
        int waypointIndex = 0;

        while (waypointIndex < _currentPath.Count)
        {
            Vector3 target = new Vector3(
                _currentPath[waypointIndex].x,
                transform.position.y,
                _currentPath[waypointIndex].z);

            if (Vector3.Distance(transform.position, target) <= arrivalRadius)
            {
                waypointIndex++;
                continue;
            }

            if (IsWaypointBlocked(target))
            {
                Vector3 finalDestination = new Vector3(
                    _currentPath[_currentPath.Count - 1].x,
                    transform.position.y,
                    _currentPath[_currentPath.Count - 1].z);

                RefreshGridCache();
                _currentPath = AStarPathfinder.FindPath(
                    transform.position, finalDestination,
                    _cachedGrid, _cachedOrigin, _cachedCellSize, _cachedGridSize);

                if (_currentPath == null || _currentPath.Count == 0)
                {
                    yield return StartCoroutine(WalkStraightTo(finalDestination));
                    yield break;
                }

                waypointIndex = 0;
                continue;
            }

            Vector3 dir = (target - transform.position);
            dir.y = 0f;
            dir.Normalize();

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir, Vector3.up),
                Time.deltaTime * turnSpeed);

            float step = moveSpeed * Time.deltaTime;
            float remaining = Vector3.Distance(transform.position, target);
            if (step > remaining) step = remaining;

            if (useRigidbody && rb != null)
                rb.MovePosition(rb.position + dir * step);
            else
                transform.position += dir * step;

            ApplySeparation();
            yield return null;
        }
    }

    private bool IsWaypointBlocked(Vector3 worldPos)
    {
        if (!_gridCached) return false;

        int gx = Mathf.Clamp(
            Mathf.FloorToInt((worldPos.x - _cachedOrigin.x) / _cachedCellSize),
            0, _cachedGridSize.x - 1);
        int gz = Mathf.Clamp(
            Mathf.FloorToInt((worldPos.z - _cachedOrigin.z) / _cachedCellSize),
            0, _cachedGridSize.y - 1);

        return _cachedGrid[gx, gz] != null;
    }

    private IEnumerator WalkStraightTo(Vector3 destination)
    {
        destination.y = transform.position.y;

        while (Vector3.Distance(transform.position, destination) > arrivalRadius)
        {
            Vector3 dir = (destination - transform.position);
            dir.y = 0f;
            dir.Normalize();

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir, Vector3.up),
                Time.deltaTime * turnSpeed);

            float step = moveSpeed * Time.deltaTime;
            float remaining = Vector3.Distance(transform.position, destination);
            if (step > remaining) step = remaining;

            if (useRigidbody && rb != null)
                rb.MovePosition(rb.position + dir * step);
            else
                transform.position += dir * step;

            ApplySeparation();
            yield return null;
        }
    }

    private void StepMove()
    {
        Vector3 flatDir = new Vector3(moveDirection.x, 0f, moveDirection.z).normalized;
        if (flatDir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(flatDir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);

        Vector3 motion = flatDir * moveSpeed * Time.deltaTime;
        if (useRigidbody && rb != null)
            rb.MovePosition(rb.position + motion);
        else
            transform.position += motion;
    }

    private Vector3 RandomDirectionXZ()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_state != NPCState.Walking) return;

        if (collision.contactCount > 0)
        {
            Vector3 normal = collision.contacts[0].normal;
            Vector3 reflected = Vector3.Reflect(moveDirection.normalized, normal).normalized;

            float jitter = Random.Range(-20f, 20f) * Mathf.Deg2Rad;
            reflected = Quaternion.Euler(0f, Mathf.Rad2Deg * jitter, 0f) * reflected;
            moveDirection = reflected;

            if (!isMoving)
            {
                StopAllCoroutines();
                StartCoroutine(QuickEscapeAndResume());
            }
        }
    }

    private IEnumerator QuickEscapeAndResume()
    {
        isMoving = true;
        float t = 0f;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            StepMove();
            yield return null;
        }
        isMoving = false;
        yield return new WaitForSeconds(Random.Range(minIdleTime, maxIdleTime));
        EnterState(NPCState.Walking);
    }

    private void SetIdleAndWait()
    {
        _state = NPCState.Idle;
        debugState = NPCState.Idle;
        UpdateAnimator();
    }

    private void ReleaseFromJob()
    {
        if (targetAccessPoint != null && targetPoint != null)
        {
            targetPoint.ReleaseAccessPoint(targetAccessPoint);
            targetAccessPoint = null;
        }

        HideAllTools();
        AddCurrentWorker(assignedResource, -1);
        EconomyManager.Instance.currentIdleNPCs++;
        isAssigned = false;
        debugAssigned = false;
    }

    private ResourceDropOff FindNearestDropOff(ResourceType type)
    {
        var all = FindObjectsByType<ResourceDropOff>(FindObjectsSortMode.None);
        ResourceDropOff nearest = null;
        float bestDist = float.MaxValue;

        foreach (var d in all)
        {
            if (!d.Accepts(type)) continue;
            float dist = Vector3.SqrMagnitude(d.transform.position - transform.position);
            if (dist < bestDist) { bestDist = dist; nearest = d; }
        }
        return nearest;
    }

    private NPCState GatherStateForType(ResourceType type) => type switch
    {
        ResourceType.Wood => NPCState.GatheringWood,
        ResourceType.Stone => NPCState.GatheringStone,
        ResourceType.Gold => NPCState.GatheringGold,
        ResourceType.Iron => NPCState.GatheringIron,
        ResourceType.Fuel => NPCState.GatheringFuel,
        ResourceType.Food => NPCState.GatheringBerries,
        _ => NPCState.GatheringWood
    };
    private void UpdateAnimator()
    {
        if (animatorRef == null) return;

        int animValue = _state switch
        {
            NPCState.Idle => 0,
            NPCState.Walking => 1,
            NPCState.CarryingResources => 2,
            NPCState.GatheringWood => 3,
            NPCState.GatheringStone => 4,
            NPCState.GatheringIron => 5,
            NPCState.GatheringGold => 6,
            NPCState.GatheringFuel => 7,
            NPCState.GatheringBerries => 8,
            NPCState.GatheringCrops => 9,
            NPCState.GatheringMeat => 10,
            _ => 0
        };

        animatorRef.SetInteger(AnimStateHash, animValue);
    }

    private void HideAllCarryProps()
    {
        if (carryWood) carryWood.SetActive(false);
        if (carryStone) carryStone.SetActive(false);
        if (carryGold) carryGold.SetActive(false);
        if (carryIron) carryIron.SetActive(false);
        if (carryFuel) carryFuel.SetActive(false);
        if (carryBerries) carryBerries.SetActive(false);
        if (carryCrops) carryCrops.SetActive(false);
        if (carryMeat) carryMeat.SetActive(false);
    }

    private void ShowCarryProp(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Wood: if (carryWood) carryWood.SetActive(true); break;
            case ResourceType.Stone: if (carryStone) carryStone.SetActive(true); break;
            case ResourceType.Gold: if (carryGold) carryGold.SetActive(true); break;
            case ResourceType.Iron: if (carryIron) carryIron.SetActive(true); break;
            case ResourceType.Fuel: if (carryFuel) carryFuel.SetActive(true); break;
            case ResourceType.Food: if (carryBerries) carryBerries.SetActive(true); break;
        }
    }

    private void HideAllTools()
    {
        if (toolWood) toolWood.SetActive(false);
        if (toolStone) toolStone.SetActive(false);
        // if (toolGold) toolGold.SetActive(false);
        // if (toolIron) toolIron.SetActive(false);
        // if (toolFuel) toolFuel.SetActive(false);
        // if (toolBerries) toolBerries.SetActive(false);
        // if (toolCrops) toolCrops.SetActive(false);
        // if (toolMeat) toolMeat.SetActive(false);
    }

    private void ShowTool(ResourceType type)
    {
        HideAllTools();
        switch (type)
        {
            case ResourceType.Wood: if (toolWood) toolWood.SetActive(true); break;
            case ResourceType.Stone: if (toolStone) toolStone.SetActive(true); break;
                // case ResourceType.Gold: if (toolGold) toolGold.SetActive(true); break;
                // case ResourceType.Iron: if (toolIron) toolIron.SetActive(true); break;
                // case ResourceType.Fuel: if (toolFuel) toolFuel.SetActive(true); break;
                // case ResourceType.Food: if (toolBerries) toolBerries.SetActive(true); break;
        }
    }
    private void AddCurrentWorker(ResourceType type, int delta)
    {
        var em = EconomyManager.Instance;
        switch (type)
        {
            case ResourceType.Wood: em.currentWoodWorkers += delta; break;
            case ResourceType.Stone: em.currentStoneWorkers += delta; break;
            case ResourceType.Gold: em.currentGoldWorkers += delta; break;
            case ResourceType.Iron: em.currentIronWorkers += delta; break;
            case ResourceType.Fuel: em.currentFuelWorkers += delta; break;
            case ResourceType.Food: em.currentFoodWorkers += delta; break;
        }
    }

    private void ApplySeparation()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, separationRadius);

        foreach (var col in nearby)
        {
            if (col.gameObject == gameObject) continue;
            if (!col.TryGetComponent<NPCMove>(out _)) continue;

            Vector3 away = transform.position - col.transform.position;
            away.y = 0f;

            if (away.sqrMagnitude < 0.001f)
                away = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));

            away.Normalize();

            float strength = 1f - (away.magnitude / separationRadius);
            transform.position += away * separationForce * strength * Time.deltaTime;
        }
    }
}