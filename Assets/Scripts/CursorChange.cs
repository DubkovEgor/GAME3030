using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CursorChange : MonoBehaviour
{
    [System.Serializable]
    public class CursorEntry
    {
        public string name;
        public Sprite sprite;
    }

    [Header("UI")]
    public RectTransform cursorTransform;
    public Image cursorImage;

    [Header("Cursor Library")]
    public List<CursorEntry> cursors = new List<CursorEntry>();

    private Dictionary<string, Sprite> cursorDictionary;
    private string currentCursor;

    private List<RaycastResult> raycastResults = new List<RaycastResult>();

    void Awake()
    {
        Cursor.visible = false;
        BuildDictionary();
    }

    void Start()
    {
        Cursor.visible = false;
        SetCursor("Cursor");
    }

    void Update()
    {
        cursorTransform.position = Input.mousePosition;

        if (IsPointerOverInteractiveUI())
        {
            // SetCursor("Cursor_UI");
        }
        else if (currentCursor == "Cursor_UI")
        {
            SetCursor("Cursor");
        }
    }

    private void BuildDictionary()
    {
        cursorDictionary = new Dictionary<string, Sprite>();

        foreach (var entry in cursors)
        {
            if (!cursorDictionary.ContainsKey(entry.name))
            {
                cursorDictionary.Add(entry.name, entry.sprite);
            }
        }
    }

    public void SetCursor(string cursorName)
    {
        if (currentCursor == cursorName)
            return;

        if (cursorDictionary.TryGetValue(cursorName, out Sprite newSprite))
        {
            cursorImage.sprite = newSprite;
            currentCursor = cursorName;
        }
        else
        {
            Debug.LogWarning("Cursor not found: " + cursorName);
        }
    }
    private bool IsPointerOverInteractiveUI()
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (var result in raycastResults)
        {
            if (result.gameObject.GetComponent<Button>() != null)
                return true;

            if (result.gameObject.GetComponent<Toggle>() != null)
                return true;

            if (result.gameObject.GetComponent<Slider>() != null)
                return true;
        }

        return false;
    }
}







//public class CursorChange : MonoBehaviour
//{
//    [SerializeField] private Texture2D cursorTexture;


//    // Start is called before the first frame update
//    private void Start()
//    {
//        Cursor.SetCursor(cursorTexture, new Vector2(28, 22), CursorMode.Auto);
//    }


//}

//public class CursorChange : MonoBehaviour
//{
//    [SerializeField] private Texture2D[] cursorTextureArray;
//    [SerializeField] private int frameCount;
//    [SerializeField] private float frameRate;

//    private int currentFrame;
//    private float frameTimer;

//    // Start is called before the first frame update
//    private void Start()
//    {
//        Cursor.SetCursor(cursorTextureArray[0], new Vector2(28, 22), CursorMode.Auto);
//    }



//    // Update is called once per frame
//    private void Update()
//    {
//        frameTimer -= Time.deltaTime;
//        if (frameTimer >= 0f)
//        {
//            frameTimer += frameRate;
//            currentFrame = (currentFrame + 1) % frameCount;
//            Cursor.SetCursor(cursorTextureArray[currentFrame], new Vector2(28, 22), CursorMode.Auto);
//        }
//    }
//}
