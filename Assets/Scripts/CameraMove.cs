using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float zoomSpeed = 10f;
    public float minZoom = 5f;
    public float maxZoom = 50f;

    public float orbitSpeed = 5f; 
    private float yaw = 0f;       
    private float pitch = 30f;    


    public Transform target;

    void Start()
    {
        if (target != null)
        {
            Vector3 angles = transform.eulerAngles;
            yaw = angles.y;
            pitch = angles.x;
        }
    }

    void Update()
    {
        if (target == null) return;

        HandleZoom();
        HandleOrbit();
        HandleQuit();
        if (Input.GetKeyDown(KeyCode.P))
        {
            EconomyManager.Instance.Add1000Resources();
        }
    }

    private void HandleZoom()
    {
        if (!Input.GetKey(KeyCode.R))
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                Vector3 direction = (target.position - transform.position).normalized;
                Vector3 newPosition = transform.position + direction * scroll * zoomSpeed;

                float distance = Vector3.Distance(newPosition, target.position);
                if (distance > minZoom && distance < maxZoom)
                {
                    transform.position = newPosition;
                }
            }
        }
    }

    private void HandleOrbit()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yaw += mouseX * orbitSpeed;
            pitch -= mouseY * orbitSpeed;
            pitch = Mathf.Clamp(pitch, 10f, 80f);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 offset = rotation * new Vector3(0f, 0f, -Vector3.Distance(transform.position, target.position));
            transform.position = target.position + offset;
            transform.LookAt(target.position);
        }
    }
    private void HandleQuit()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(); // quit the build
#endif
    }

}
