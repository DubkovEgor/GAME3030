using UnityEngine;
using UnityEngine.UI;

public class CameraMove : MonoBehaviour
{
    [Header("UI")]
    public Slider speedSlider;
    public Text speedText;

    public Slider rotationSlider;
    public Text rotationText;

    public float zoomSpeed = 10f;
    public float minZoom = 5f;
    public float maxZoom = 50f;

    public float orbitSpeed = 5f;
    private float yaw = 0f;
    private float pitch = 30f;

    public Transform target;

    public float cameraXYspeed = 10f;
    public float cameraRotationSpeed = 50f;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("CameraSpeed"))
            cameraXYspeed = PlayerPrefs.GetFloat("CameraSpeed");

        if (PlayerPrefs.HasKey("CameraRotationSpeed"))
            cameraRotationSpeed = PlayerPrefs.GetFloat("CameraRotationSpeed");
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

        if (rotationSlider != null)
            rotationSlider.value = cameraRotationSpeed;

        UpdateCameraSpeedText();
        UpdateCameraRotationText();
    }

    void Update()
    {
        if (target == null) return;

        HandleZoom();
        HandleMovement();
        HandleRotation();
        HandleOrbit();

        if (Input.GetKeyDown(KeyCode.P))
            EconomyManager.Instance.Add1000Resources();
    }

    private void HandleMovement()
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
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(1)) return;

        float rotate = 0f;

        if (Input.GetKey(KeyCode.Q)) rotate -= 1f;
        if (Input.GetKey(KeyCode.E)) rotate += 1f;

        if (rotate != 0f)
        {
            yaw += rotate * orbitSpeed * cameraRotationSpeed * Time.unscaledDeltaTime;

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            float distance = Vector3.Distance(transform.position, target.position);
            Vector3 offset = rotation * new Vector3(0f, 0f, -distance);

            transform.position = target.position + offset;
            transform.LookAt(target.position);
        }
    }

    private void HandleOrbit()
    {
        if (!Input.GetMouseButton(1)) return;

        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        if (mouseX != 0f || mouseY != 0f)
        {
            yaw += mouseX * orbitSpeed * cameraRotationSpeed * Time.unscaledDeltaTime;
            pitch -= mouseY * orbitSpeed * cameraRotationSpeed * Time.unscaledDeltaTime;
            pitch = Mathf.Clamp(pitch, 10f, 80f);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            float distance = Vector3.Distance(transform.position, target.position);
            Vector3 offset = rotation * new Vector3(0f, 0f, -distance);

            transform.position = target.position + offset;
            transform.LookAt(target.position);
        }
    }

    private void HandleZoom()
    {
        if (Input.GetKey(KeyCode.R)) return;

        float zoomInput = Input.GetAxis("Mouse ScrollWheel");

        if (Input.GetKey(KeyCode.Comma)) zoomInput += 0.02f;
        if (Input.GetKey(KeyCode.Period)) zoomInput -= 0.02f;

        if (zoomInput == 0f) return;

        Vector3 direction = (target.position - transform.position).normalized;
        Vector3 newPosition = transform.position + direction * zoomInput * zoomSpeed;
        float distance = Vector3.Distance(newPosition, target.position);

        if (distance > minZoom && distance < maxZoom)
            transform.position = newPosition;
    }

    public void SetCameraSpeed(float newSpeed)
    {
        cameraXYspeed = newSpeed;

        if (speedText != null)
            speedText.text = Mathf.RoundToInt(cameraXYspeed).ToString();

        PlayerPrefs.SetFloat("CameraSpeed", cameraXYspeed);
        PlayerPrefs.Save();
    }

    public void SetCameraRotationSpeed(float newSpeed)
    {
        cameraRotationSpeed = newSpeed;

        if (rotationText != null)
            rotationText.text = Mathf.RoundToInt(cameraRotationSpeed).ToString();

        PlayerPrefs.SetFloat("CameraRotationSpeed", cameraRotationSpeed);
        PlayerPrefs.Save();
    }

    public void UpdateCameraSpeedText()
    {
        if (speedText != null)
            speedText.text = Mathf.RoundToInt(cameraXYspeed).ToString();
    }

    public void UpdateCameraRotationText()
    {
        if (rotationText != null)
            rotationText.text = Mathf.RoundToInt(cameraRotationSpeed).ToString();
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