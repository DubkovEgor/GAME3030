using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorkerAllocationUI : MonoBehaviour
{
    [System.Serializable]
    public class ResourceRow
    {
        public string label;
        public ResourceType resourceType;

        [Header("UI References")]
        public TextMeshProUGUI labelText;
        public TextMeshProUGUI countText;
        public Slider slider;
        public TMP_InputField inputField;
        public Button clearButton;
        public Button addFiveButton;
    }

    [Header("Rows")]
    public ResourceRow[] rows;

    [Header("Header")]
    public TextMeshProUGUI totalWorkersText;
    public TextMeshProUGUI idleWorkersText;

    private bool _updating = false;
    private bool _initialized = false;

    private void Start()
    {
        StartCoroutine(InitDelayed());
    }

    private IEnumerator InitDelayed()
    {
        while (EconomyManager.Instance == null || NPCRegistry.Instance == null)
            yield return null;

        SetupListeners();

        EconomyManager.Instance.OnResourcesChanged -= Refresh;
        EconomyManager.Instance.OnResourcesChanged += Refresh;

        _initialized = true;
        Refresh();
    }

    private void OnEnable()
    {
        if (!_initialized || EconomyManager.Instance == null) return;
        EconomyManager.Instance.OnResourcesChanged -= Refresh;
        EconomyManager.Instance.OnResourcesChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnResourcesChanged -= Refresh;
    }

    private void SetupListeners()
    {
        foreach (var row in rows)
        {
            var r = row;

            r.slider.onValueChanged.AddListener(val =>
            {
                if (_updating) return;
                NPCRegistry.Instance.SetWorkerCount(r.resourceType, Mathf.RoundToInt(val));
                EconomyManager.Instance.NotifyResourcesChanged();
            });

            r.inputField.onEndEdit.AddListener(val =>
            {
                if (_updating) return;
                if (int.TryParse(val, out int parsed))
                {
                    int current = NPCRegistry.Instance.GetWorkers(r.resourceType).Count;
                    int idle = EconomyManager.Instance.currentIdleNPCs;
                    int max = GetMax(r.resourceType);
                    int target = Mathf.Clamp(parsed, 0, Mathf.Min(current + idle, max));
                    NPCRegistry.Instance.SetWorkerCount(r.resourceType, target);
                    EconomyManager.Instance.NotifyResourcesChanged();
                }
                r.inputField.DeactivateInputField();
            });

            r.clearButton.onClick.AddListener(() =>
            {
                NPCRegistry.Instance.UnassignAll(r.resourceType);
                EconomyManager.Instance.NotifyResourcesChanged();
            });
            r.addFiveButton.onClick.AddListener(() =>
            {
                int current = NPCRegistry.Instance.GetWorkers(r.resourceType).Count;
                int idle = EconomyManager.Instance.currentIdleNPCs;
                int max = GetMax(r.resourceType);
                int target = Mathf.Min(current + 5, current + idle, max);
                NPCRegistry.Instance.SetWorkerCount(r.resourceType, target);
                EconomyManager.Instance.NotifyResourcesChanged();
            });
        }
    }

    private void Refresh()
    {
        if (!_initialized) return;

        _updating = true;

        var em = EconomyManager.Instance;
        if (totalWorkersText) totalWorkersText.text = $"Workers: {em.currentNPCs}";
        if (idleWorkersText) idleWorkersText.text = $"Idle: {em.currentIdleNPCs}";

        foreach (var row in rows)
        {
            int max = GetMax(row.resourceType);
            int current = NPCRegistry.Instance.GetWorkers(row.resourceType).Count;

            if (row.labelText) row.labelText.text = row.label;
            if (row.countText) row.countText.text = $"/ {max}";

            row.slider.minValue = 0;
            row.slider.maxValue = max;
            row.slider.value = current;

            if (row.inputField) row.inputField.text = current.ToString();
        }

        _updating = false;
    }

    private int GetMax(ResourceType type)
    {
        var em = EconomyManager.Instance;
        return type switch
        {
            ResourceType.Wood => em.woodWorkers,
            ResourceType.Stone => em.stoneWorkers,
            ResourceType.Iron => em.ironWorkers,
            ResourceType.Gold => em.goldWorkers,
            ResourceType.Fuel => em.fuelWorkers,
            ResourceType.Food => em.foodWorkers,
            _ => 0
        };
    }
}