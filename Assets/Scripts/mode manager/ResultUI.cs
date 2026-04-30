using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections;
public class ResultUI : MonoBehaviour
{
    [Header("Text Fields")]
    public TMP_Text scoreTxt;
    public TMP_Text scoreTxt1;
    public TMP_Text totalEquationsTxt;
    public TMP_Text solvedEquationsTxt;
    public TMP_Text level;

    [Header("Progress Bar")]
    public Slider progressSlider;

    IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;
        UpdateUI();
    }



    void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    void OnLocaleChanged(Locale locale)
    {
        StartCoroutine(Refresh());
    }

    IEnumerator Refresh()
    {
        yield return null;
        UpdateUI();
    }

    void UpdateUI()
    {
        int score = PlayerPrefs.GetInt("Playerscore", 0);
        int total = PlayerPrefs.GetInt("TotalEquations", 0);
        int solved = PlayerPrefs.GetInt("SolvedEquations", 0);

        scoreTxt.text = LocalizedNumber.Format(score);
        scoreTxt1.text = LocalizedNumber.Format(score);
        totalEquationsTxt.text = LocalizedNumber.FormatPlain(total);
        solvedEquationsTxt.text = LocalizedNumber.FormatPlain(solved);
        level.text = LocalizedNumber.FormatPlain(solved);
    }
}





//using UnityEngine;
//using TMPro;
//using UnityEngine.UI;

//public class ResultUI : MonoBehaviour
//{
//    [Header("Text Fields")]
//    public TMP_Text scoreTxt;
//    public TMP_Text totalEquationsTxt;
//    public TMP_Text solvedEquationsTxt;

//    [Header("Progress Bar")]
//    public Slider progressSlider;

//    void Start()
//    {
//        int score = PlayerPrefs.GetInt("Playerscore", 0);
//        int total = PlayerPrefs.GetInt("TotalEquations", 0);
//        int solved = PlayerPrefs.GetInt("SolvedEquations", 0);

//        // ✅ Use localized format
//        scoreTxt.text = LocalizedNumber.FormatPlain(score);
//        totalEquationsTxt.text = LocalizedNumber.FormatPlain(total);
//        solvedEquationsTxt.text = LocalizedNumber.FormatPlain(solved);

//        if (progressSlider != null)
//        {
//            progressSlider.minValue = 0f;
//            progressSlider.maxValue = 1f;
//            progressSlider.interactable = false;
//            progressSlider.value = total > 0 ? (float)solved / total : 0f;
//        }
//    }
//}