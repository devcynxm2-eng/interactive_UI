using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings; // ✅ add this

public class ScoreManager : MonoBehaviour
{
    [Header("Show Player Score")]
    public TMP_Text Show_Player_Score;
    private string scorekey = "Playerscore";

    void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged; // ✅
    }

    void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged; // ✅
    }

    private void OnLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        updateScoreui(); // ✅ refresh score in new locale
    }

    void Start()
    {
        if (Show_Player_Score == null)
        {
            GameObject scoreObj = GameObject.Find("Show_Player_Score");
            if (scoreObj != null)
                Show_Player_Score = scoreObj.GetComponent<TMP_Text>();
        }

        if (Show_Player_Score == null)
            Debug.LogError("ScoreManager: Show_Player_Score TMP_Text not found!");
        else
            updateScoreui();
    }

    public void addscore(int pointtoadd)
    {
        int currentscore = PlayerPrefs.GetInt(scorekey, 0);
        currentscore += pointtoadd;
        PlayerPrefs.SetInt(scorekey, currentscore);
        PlayerPrefs.Save();
        updateScoreui();
    }

    public void Subtractscore(int pointtoadd)
    {
        int currentscore = PlayerPrefs.GetInt(scorekey, 0);
        currentscore -= pointtoadd;
        currentscore = Mathf.Max(0, currentscore);
        PlayerPrefs.SetInt(scorekey, currentscore);
        PlayerPrefs.Save();
        updateScoreui();
    }

    public void updateScoreui()
    {
        if (Show_Player_Score == null) return;
        int currentscore = PlayerPrefs.GetInt(scorekey, 0);
        Show_Player_Score.text = LocalizedNumber.Format(currentscore);
    }
}