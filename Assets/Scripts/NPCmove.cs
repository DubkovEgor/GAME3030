using UnityEngine;
using System.Collections;

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

    [Header("Optional")]
    public bool useRigidbody = false;           

    Vector3 moveDirection;
    bool isMoving = false;
    Rigidbody rb;
    bool isOnPath = false;
    float targetSpeed = 2f;
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

        StartCoroutine(WanderLoop());
    }

    private void Update()
    {
        targetSpeed = isOnPath ? 4f : 2f;
        moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, Time.deltaTime * 5f);
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
        {
            rb.MovePosition(rb.position + motion);
        }
        else
        {
            transform.position += motion;
        }
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
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Path"))
            isOnPath = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Path"))
            isOnPath = false;
    }


    private IEnumerator QuickEscapeAndResume()
    {
        isMoving = true;
        float burst = 0.5f;
        float t = 0f;
        while (t < burst)
        {
            t += Time.deltaTime;
            StepMove();
            yield return null;
        }
        isMoving = false;

        float idle = Random.Range(minIdleTime, maxIdleTime);
        yield return new WaitForSeconds(idle);

        StartCoroutine(WanderLoop());
    }
}
