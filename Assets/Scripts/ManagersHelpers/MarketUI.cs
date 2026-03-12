using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketUI : MonoBehaviour
{
    [Header("Sell Buttons")]
    public Button[] sellButtons;

    [Header("Buy Buttons")]
    public Button[] buyButtons;

    [Header("Rate Labels - Sell Side")]
    public TMP_Text[] sellRateLabels;

    [Header("Rate Labels - Buy Side")]
    public TMP_Text[] buyRateLabels;

    [Header("Stock Labels - Sell Side")]
    public TMP_Text[] sellStockLabels;

    [Header("Amount Controls")]
    public TMP_InputField amountInput;
    public Button btnPlus100;
    public Button btnPlus1000;
    public Button btnMinus100;
    public Button btnMinus1000;
    public Button btnAddHalf;
    public Button btnAddAll;
    public Button btnClear;
    public Button btnAccept;

    [Header("Info Display")]
    public TMP_Text youSellAmountText;
    public TMP_Text youBuyAmountText;
    public TMP_Text sellResNameText;
    public TMP_Text buyResNameText;
    public TMP_Text feedbackText;
    public TMP_Text tradeRateText;

    [Header("Selection Colors")]
    public Color sellHighlight = new Color(0.8f, 0.2f, 0.2f, 1f);
    public Color buyHighlight = new Color(0.2f, 0.7f, 0.2f, 1f);
    public Color defaultColor = Color.white;

    MarketResource _sellRes;
    MarketResource _buyRes;
    int _amount = 100;
    MarketResource[] _resources;
    Image[] _sellImages;
    Image[] _buyImages;

    void Start()
    {
        _resources = Market.Instance.GetAll();

        _sellImages = new Image[sellButtons.Length];
        _buyImages = new Image[buyButtons.Length];
        for (int i = 0; i < sellButtons.Length; i++) _sellImages[i] = sellButtons[i].GetComponent<Image>();
        for (int i = 0; i < buyButtons.Length; i++) _buyImages[i] = buyButtons[i].GetComponent<Image>();

        for (int i = 0; i < sellButtons.Length; i++)
        {
            int idx = i;
            sellButtons[i].onClick.AddListener(() => SelectSell(idx));
        }

        for (int i = 0; i < buyButtons.Length; i++)
        {
            int idx = i;
            buyButtons[i].onClick.AddListener(() => SelectBuy(idx));
        }

        btnPlus100.onClick.AddListener(() => AdjustAmount(+100));
        btnPlus1000.onClick.AddListener(() => AdjustAmount(+1000));
        btnMinus100.onClick.AddListener(() => AdjustAmount(-100));
        btnMinus1000.onClick.AddListener(() => AdjustAmount(-1000));
        btnAddHalf.onClick.AddListener(SetAmountHalf);
        btnAddAll.onClick.AddListener(SetAmountAll);
        btnClear.onClick.AddListener(ClearTrade);
        btnAccept.onClick.AddListener(DoTrade);

        amountInput.onEndEdit.AddListener(OnAmountTyped);

        Market.Instance.OnRatesChanged += RefreshRateLabels;
        EconomyManager.Instance.OnResourcesChanged += RefreshStockLabels;

        amountInput.text = _amount.ToString();
        RefreshRateLabels();
        RefreshStockLabels();
        UpdatePreview();
    }

    void OnDestroy()
    {
        if (Market.Instance != null)
            Market.Instance.OnRatesChanged -= RefreshRateLabels;
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnResourcesChanged -= RefreshStockLabels;
    }

    void SelectSell(int idx)
    {
        _sellRes = _resources[idx];
        for (int i = 0; i < _sellImages.Length; i++)
            _sellImages[i].color = (i == idx) ? sellHighlight : defaultColor;
        UpdatePreview();
    }

    void SelectBuy(int idx)
    {
        _buyRes = _resources[idx];
        for (int i = 0; i < _buyImages.Length; i++)
            _buyImages[i].color = (i == idx) ? buyHighlight : defaultColor;
        UpdatePreview();
    }

    void AdjustAmount(int delta)
    {
        _amount = Mathf.Max(1, _amount + delta);
        amountInput.text = _amount.ToString();
        UpdatePreview();
    }

    void OnAmountTyped(string val)
    {
        _amount = int.TryParse(val, out int parsed) ? Mathf.Max(1, parsed) : 1;
        amountInput.text = _amount.ToString();
        UpdatePreview();
    }
    void SetAmountAll()
    {
        if (_sellRes == null) return;
        _amount = GetPlayerStock(_sellRes);
        amountInput.text = _amount.ToString();
        UpdatePreview();
    }

    void SetAmountHalf()
    {
        if (_sellRes == null) return;
        _amount = Mathf.Max(1, GetPlayerStock(_sellRes) / 2);
        amountInput.text = _amount.ToString();
        UpdatePreview();
    }
    void UpdatePreview()
    {
        sellResNameText.text = _sellRes != null ? $"Selling: {_sellRes.name}" : "Selling: -";
        buyResNameText.text = _buyRes != null ? $"Getting: {_buyRes.name}" : "Getting: -";

        if (_sellRes == null || _buyRes == null)
        {
            youSellAmountText.text = "-";
            youBuyAmountText.text = "-";
            tradeRateText.text = "";
            feedbackText.text = "Select a resource on each side.";
            feedbackText.color = Color.gray;
            btnAccept.interactable = false;
            return;
        }

        if (_sellRes == _buyRes)
        {
            youSellAmountText.text = "-";
            youBuyAmountText.text = "-";
            tradeRateText.text = "";
            feedbackText.text = "Cannot trade a resource for itself.";
            feedbackText.color = Color.red;
            btnAccept.interactable = false;
            return;
        }

        int receive = Market.Instance.Preview(_sellRes, _buyRes, _amount);
        float rate = _sellRes.EffectiveValue / _buyRes.EffectiveValue * (1f - Market.Instance.feeFraction);
        int playerStock = GetPlayerStock(_sellRes);

        youSellAmountText.text = _amount.ToString();
        youBuyAmountText.text = receive.ToString();
        tradeRateText.text = $"Rate: {rate:F3}  ({_sellRes.RatePercent}% / {_buyRes.RatePercent}%)";

        if (receive <= 0)
        {
            feedbackText.text = "Amount too small for this trade.";
            feedbackText.color = Color.red;
            btnAccept.interactable = false;
        }
        else if (playerStock < _amount)
        {
            feedbackText.text = $"Not enough {_sellRes.name}. You have {playerStock}.";
            feedbackText.color = Color.red;
            btnAccept.interactable = false;
        }
        else
        {
            feedbackText.text = $"Sell {_amount} {_sellRes.name} get {receive} {_buyRes.name}";
            feedbackText.color = Color.white;
            btnAccept.interactable = true;
        }
    }

    void DoTrade()
    {
        int receive = Market.Instance.Preview(_sellRes, _buyRes, _amount);
        bool ok = Market.Instance.ExecuteTrade(_sellRes, _buyRes, _amount);

        if (!ok)
        {
            feedbackText.text = "Trade failed.";
            feedbackText.color = Color.red;
            return;
        }

        feedbackText.text = $"Traded. Received {receive} {_buyRes.name}.";
        feedbackText.color = Color.green;

        RefreshStockLabels();
        UpdatePreview();
    }

    void ClearTrade()
    {
        _sellRes = null;
        _buyRes = null;
        _amount = 100;
        amountInput.text = "100";

        foreach (var img in _sellImages) img.color = defaultColor;
        foreach (var img in _buyImages) img.color = defaultColor;

        UpdatePreview();
    }

    void RefreshRateLabels()
    {
        for (int i = 0; i < _resources.Length; i++)
        {
            var res = _resources[i];

            if (i < sellRateLabels.Length && sellRateLabels[i] != null)
            {
                sellRateLabels[i].text = res.RateLabel;
                sellRateLabels[i].color = res.RateColor();
            }
            if (i < buyRateLabels.Length && buyRateLabels[i] != null)
            {
                buyRateLabels[i].text = res.RateLabel;
                buyRateLabels[i].color = res.RateColor();
            }
        }

        UpdatePreview();
    }

    void RefreshStockLabels()
    {
        EconomyManager eco = EconomyManager.Instance;

        int[] stocks = new int[]
        {
            eco.food,
            eco.wood,
            eco.stone,
            eco.fuel,
            eco.iron,
            eco.gold
        };

        for (int i = 0; i < sellStockLabels.Length; i++)
        {
            if (sellStockLabels[i] != null)
                sellStockLabels[i].text = stocks[i].ToString();
        }

        UpdatePreview();
    }

    int GetPlayerStock(MarketResource res)
    {
        EconomyManager eco = EconomyManager.Instance;
        if (res == Market.Instance.food) return eco.food;
        if (res == Market.Instance.wood) return eco.wood;
        if (res == Market.Instance.stone) return eco.stone;
        if (res == Market.Instance.fuel) return eco.fuel;
        if (res == Market.Instance.iron) return eco.iron;
        if (res == Market.Instance.gold) return eco.gold;
        return 0;
    }
}