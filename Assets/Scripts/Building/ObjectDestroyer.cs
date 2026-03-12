using UnityEngine;

public class ObjectDestroyer : MonoBehaviour
{
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButton(1))
            return;
        if (Input.GetKeyDown(KeyCode.D))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                GameObject target = hit.collider.gameObject;

                if (target.CompareTag("Building") || target.CompareTag("Path")|| target.CompareTag("Decoration"))
                {
                    Building building = target.GetComponent<Building>();
                    if (building != null)
                        building.OnDestroyed();
                    Destroy(target);
                }
            }
        }
    }
}
