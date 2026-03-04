using UnityEngine;

public class Building : MonoBehaviour
{

    public Renderer[] renderers;
    public Vector2Int Size = Vector2Int.one;
    public float CellSize = 10f;


    [HideInInspector] public Vector2Int CurrentSize;
    public ResourceCost Generation;
    public int HousingProvided = 0;
    private GameObject highlightParent;
    public Material highlightMaterial;
    private Color[][] originalColors;


    [Header("Cost to build")]
    public ResourceCost Cost;

    private void Awake()
     {
        renderers = GetComponentsInChildren<Renderer>();
        CurrentSize = Size;
        SaveMaterials();
     }
    void Start()
    {
        CreateHighlight();
    }
    public void SetTransparent(bool available)
    {
        Color tint = available ? new Color(0, 1, 0, 0.2f) : new Color(1, 0, 0, 0.2f);

        for (int r = 0; r < renderers.Length; r++)
        {
            var mats = renderers[r].materials;

            for (int m = 0; m < mats.Length; m++)
                mats[m].color = originalColors[r][m] * tint;
        }
    }



    public void SetNormal()
    {
        for (int r = 0; r < renderers.Length; r++)
        {
            var mats = renderers[r].materials;

            for (int m = 0; m < mats.Length; m++)
                mats[m].color = originalColors[r][m];
        }
    }

    void CreateHighlight()
    {
        if (tag == "Decoration" || tag == "Path")
            return;

        if (highlightParent != null)
            Destroy(highlightParent);

        if (highlightMaterial == null)
        {
            Debug.LogError("Highlight material not assigned on " + name);
            return;
        }

        highlightParent = new GameObject("Highlight");
        highlightParent.transform.parent = transform;
        highlightParent.transform.localPosition = Vector3.zero;

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.parent = highlightParent.transform;

        float width = Size.x * CellSize;
        float height = Size.y * CellSize;

        quad.transform.localPosition = new Vector3(
            width * 0.5f - CellSize * 0.5f,
            0.01f,
            height * 0.5f - CellSize * 0.5f
        );

        quad.transform.localRotation = Quaternion.Euler(90, 0, 0);
        quad.transform.localScale = new Vector3(width, height, 1f);

        Renderer quadRenderer = quad.GetComponent<Renderer>();
        quadRenderer.material = new Material(highlightMaterial);

        if (tag == "Building")
            quadRenderer.material.color = new Color(0.76f, 0.60f, 0.42f, 0.5f);
        else
            quadRenderer.material.color = new Color(1f, 1f, 1f, 0.5f);

        Destroy(quad.GetComponent<Collider>());
    }

    void SaveMaterials()
    {
        originalColors = new Color[renderers.Length][];

        for (int r = 0; r < renderers.Length; r++)
        {
            var mats = renderers[r].materials;
            originalColors[r] = new Color[mats.Length];

            for (int m = 0; m < mats.Length; m++)
                originalColors[r][m] = mats[m].color;
        }
    }
    public void OnPlaced()
    {
        EconomyManager.Instance.housing += HousingProvided;

        EconomyManager.Instance.goldPerSec += Generation.gold;
        EconomyManager.Instance.ironPerSec += Generation.iron;
        EconomyManager.Instance.stonePerSec += Generation.stone;
        EconomyManager.Instance.woodPerSec += Generation.wood;

        EconomyManager.Instance.NotifyResourcesChanged();
        
    }
        public void OnDestroyed()
    {
        EconomyManager.Instance.housing -= HousingProvided;
        EconomyManager.Instance.goldPerSec -= Generation.gold;
        EconomyManager.Instance.ironPerSec -= Generation.iron;
        EconomyManager.Instance.stonePerSec -= Generation.stone;
        EconomyManager.Instance.woodPerSec -= Generation.wood;

        EconomyManager.Instance.NotifyResourcesChanged();
    }
   private void OnDrawGizmos() 
   { 
       for (int x = 0; x < Size.x; x++)
       { 
           for (int y = 0; y < Size.y; y++) 
           { 
               if ((x + y) % 2 == 0) Gizmos.color = new Color(0.8f, 0f, 1f, 0.3f); 
               else Gizmos.color = new Color(1f, 0.6f, 0f, 0.3f); 
               Vector3 pos = transform.position + new Vector3(x * CellSize, 0, y * CellSize); 
               Vector3 size = new Vector3(CellSize, 0.1f, CellSize);
               Gizmos.DrawCube(pos, size);
           }
       }
   }
}