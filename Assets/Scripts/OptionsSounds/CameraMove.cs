using UnityEngine;
using UnityEngine.UI;

public class CameraMove : MonoBehaviour
{
    [Header("UI")]
    public Slider speedSlider;
    public Text speedText;

    public float zoomSpeed = 10f;
    public float minZoom = 5f;
    public float maxZoom = 50f;

    public float orbitSpeed = 5f;
    private float yaw = 0f;
    private float pitch = 30f;

    public Transform target;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("CameraSpeed"))
            cameraXYspeed = PlayerPrefs.GetFloat("CameraSpeed");
    }

    void Start()
    {
        if (target != null)
        {
            Vector3 angles = transform.eulerAngles;
            yaw = angles.y;
            pitch = angles.x;
        }
        if (speedSlider != null)
            speedSlider.value = cameraXYspeed;

        UpdateCameraSpeedText();
    }

    void Update()
    {
        if (target == null) return;

        HandleZoom();
        HandleOrbit();
        // HandleQuit();

        if (Input.GetKeyDown(KeyCode.P))
            EconomyManager.Instance.Add1000Resources();
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
                    transform.position = newPosition;
            }
        }
    }

    public float cameraXYspeed = 10f;

    private void HandleOrbit()
    {
        if (Input.GetMouseButton(1))
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");

            if (moveX != 0f || moveY != 0f)
            {
                Vector3 right = transform.right;
                Vector3 forward = Vector3.Cross(right, Vector3.up).normalized;

                Vector3 move = (right * moveX + forward * moveY) * cameraXYspeed * Time.unscaledDeltaTime;

                transform.position += move;
                target.position += move;
            }

            float mouseX = Input.GetAxisRaw("Mouse X");
            float mouseY = Input.GetAxisRaw("Mouse Y");

            if (mouseX != 0f || mouseY != 0f)
            {
                yaw += mouseX * orbitSpeed;
                pitch -= mouseY * orbitSpeed;
                pitch = Mathf.Clamp(pitch, 10f, 80f);

                Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
                float distance = Vector3.Distance(transform.position, target.position);
                Vector3 offset = rotation * new Vector3(0f, 0f, -distance);

                transform.position = target.position + offset;
                transform.LookAt(target.position);
            }
        }
    }

    public void SetCameraSpeed(float newSpeed)
    {
        int speedInt = Mathf.RoundToInt(newSpeed);
        cameraXYspeed = speedInt;

        if (speedText != null)
            speedText.text = speedInt.ToString();

        PlayerPrefs.SetFloat("CameraSpeed", speedInt);
        PlayerPrefs.Save();
    }

    public void UpdateCameraSpeedText()
    {
        if (speedText != null)
            speedText.text = Mathf.RoundToInt(cameraXYspeed).ToString();
    }

    private void HandleQuit()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            QuitGame();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}