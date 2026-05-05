using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

public class ScoreShowing : MonoBehaviour
{
    [Header("Text Fields")]
    public TMP_Text scoreTxt;
    //public TMP_Text scoreTxt1;
    //public TMP_Text totalEquationsTxt;
    //public TMP_Text solvedEquationsTxt;
    //public TMP_Text level;

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
        //scoreTxt1.text = LocalizedNumber.Format(score);
        //totalEquationsTxt.text = LocalizedNumber.FormatPlain(total);
        //solvedEquationsTxt.text = LocalizedNumber.FormatPlain(solved);
        //level.text = LocalizedNumber.FormatPlain(solved);
    }


}
