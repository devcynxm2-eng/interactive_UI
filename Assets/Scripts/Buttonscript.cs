using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NumberButton : MonoBehaviour
{
    public int number;
    public NumEq numEq;
    private Button button;
    public int eqdata;
    private TMP_Text label;
    public SoundManager soundManager;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        label = GetComponentInChildren<TMP_Text>();
    }

    void Start()
    {
        eqdata = EquationManager.currentEquationLength;
    }

    // ✅ Call this whenever you assign a new number
    public void SetNumber(int value)
    {
        number = value;
        RefreshText();
    }

    // ✅ Actually update the text in current locale
    public void RefreshText()
    {
        if (label == null)
            label = GetComponentInChildren<TMP_Text>();

        if (label != null)
        {
            string result = LocalizedNumber.FormatPlain(number);
            Debug.Log($"RefreshText called — number={number}, result={result}, locale={UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale?.Identifier.Code}");
            label.text = result;
        }
    }

    void OnClick()
    {
        GameManager.Instance?.SelectNumber(number);
    }
}