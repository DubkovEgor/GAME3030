using UnityEngine;
using System.Collections;
using System.Net.NetworkInformation;
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

    public bool isAssigned { get; private set; } = false;
    public ResourceType assignedResource { get; private set; }


    private NPCState _state = NPCState.Walking;
    public NPCState State => _state;

    private Rigidbody rb;
    private Animator _animator;
    private static readonly int AnimState = Animator.StringToHash("State");

    private Vector3 moveDirection;
    private bool isMoving = false;
    private bool isOnPath = false;
    private Coroutine _stateCoroutine;

    private ResourceGatheringPoint _targetPoint;
    private Transform _targetAccessPoint;
    private ResourceDropOff _targetDropOff;
    [SerializeField] private int _carryAmount;

    [Header("Debug Info")]
    [SerializeField] private NPCState _debugState;
    [SerializeField] private bool _debugAssigned;
    [SerializeField] private ResourceType _debugResource;
    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
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
        if (_state == NPCState.Walking)
        {
            float targetSpeed = isOnPath ? 4f : 2f;
            moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, Time.deltaTime * 5f);
        }
    }
    public void AssignJob(ResourceType type)
    {
        if (isAssigned) ReleaseFromJob();

        assignedResource = type;
        isAssigned = true;
        _debugAssigned = true;
        _debugResource = type;

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
        if (_stateCoroutine != null) StopCoroutine(_stateCoroutine);
        HideAllCarryProps();

        _state = next;
        _debugState = next;
        UpdateAnimator();

        switch (next)
        {
            case NPCState.Idle:
                break;

            case NPCState.Walking:
                _stateCoroutine = StartCoroutine(WanderLoop());
                break;

            case NPCState.GatheringWood:
            case NPCState.GatheringStone:
            case NPCState.GatheringGold:
            case NPCState.GatheringIron:
            case NPCState.GatheringFuel:
            case NPCState.GatheringBerries:
            case NPCState.GatheringCrops:
            case NPCState.GatheringMeat:
                _stateCoroutine = StartCoroutine(GatherRoutine());
                break;

            case NPCState.CarryingResources:
                ShowCarryProp(assignedResource);
                _stateCoroutine = StartCoroutine(CarryRoutine());
                break;
        }
    }
    private IEnumerator WanderLoop()
    {
        while (true)
        {
            moveDirection = RandomDirectionXZ();
            isMoving = true;

            float moveTime = Random.Range(minMoveTime, maxMoveTime);
            float t = 0f;
            while (t < moveTime)
            {
                t += Time.deltaTime;
                StepMove();
                yield return null;
            }

            isMoving = false;

            float idle = Random.Range(minIdleTime, maxIdleTime);
            float u = 0f;
            while (u < idle)
            {
                u += Time.deltaTime;
                StepRotateOnly();
                yield return null;
            }
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

    private void StepRotateOnly()
    {
        Vector3 flatDir = new Vector3(moveDirection.x, 0f, moveDirection.z).normalized;
        if (flatDir.sqrMagnitude < 0.001f) return;
        Quaternion targetRot = Quaternion.LookRotation(flatDir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * (turnSpeed * 0.5f));
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Path")) isOnPath = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Path")) isOnPath = false;
    }
    private IEnumerator GatherRoutine()
    {
        _targetPoint = GatheringPointManager.Instance.FindNearest(assignedResource, transform.position);

        if (_targetPoint == null)
        {
            yield return new WaitForSeconds(3f);
            if (isAssigned) EnterState(GatherStateForType(assignedResource));
            yield break;
        }

        _targetAccessPoint = _targetPoint.ReserveAccessPoint();
        if (_targetAccessPoint == null)
        {
            yield return new WaitForSeconds(2f);
            if (isAssigned) EnterState(GatherStateForType(assignedResource));
            yield break;
        }

        yield return StartCoroutine(WalkTo(_targetAccessPoint.position));
        yield return new WaitForSeconds(gatherDuration);

        _carryAmount = _targetPoint.TryGather();
        _targetPoint.ReleaseAccessPoint(_targetAccessPoint);
        _targetAccessPoint = null;

        if (_carryAmount > 0)
            EnterState(NPCState.CarryingResources);
        else if (isAssigned)
            EnterState(GatherStateForType(assignedResource));
    }

    private IEnumerator CarryRoutine()
    {
        _targetDropOff = FindNearestDropOff(assignedResource);

        if (_targetDropOff == null)
        {
            yield return new WaitForSeconds(3f);
            if (isAssigned) EnterState(GatherStateForType(assignedResource));
            yield break;
        }

        yield return StartCoroutine(WalkTo(_targetDropOff.transform.position));

        _targetDropOff.Deposit(assignedResource, _carryAmount);
        _carryAmount = 0;

        if (isAssigned)
            EnterState(GatherStateForType(assignedResource));
        else
            EnterState(NPCState.Walking);
    }

    private IEnumerator WalkTo(Vector3 destination)
    {
        while (Vector3.Distance(transform.position, destination) > arrivalRadius)
        {
            Vector3 dir = (destination - transform.position).normalized;
            dir.y = 0f;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), Time.deltaTime * turnSpeed);
            transform.position += dir * moveSpeed * Time.deltaTime;
            yield return null;
        }
    }
    private void ReleaseFromJob()
    {
        if (_targetAccessPoint != null && _targetPoint != null)
        {
            _targetPoint.ReleaseAccessPoint(_targetAccessPoint);
            _targetAccessPoint = null;
        }

        AddCurrentWorker(assignedResource, -1);
        EconomyManager.Instance.currentIdleNPCs++;
        isAssigned = false;
        _debugAssigned = false;
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
        if (_animator == null) return;
        _animator.SetInteger(AnimState, (int)_state);
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
}