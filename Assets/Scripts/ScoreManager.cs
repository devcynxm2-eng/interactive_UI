using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("Show Player Score")]
    public TMP_Text Show_Player_Score;

    private string scorekey = "Playerscore";

    void Start()
    {
        // Auto-find if not assigned in Inspector
        if (Show_Player_Score == null)
        {
            // Try to find it by name in the scene
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
        Debug.Log("SCORE ADDED: " + currentscore);
        updateScoreui();
    }

    public void Subtractscore(int pointtoadd)
    {
        int currentscore = PlayerPrefs.GetInt(scorekey, 0);
        currentscore -= pointtoadd;
        currentscore = Mathf.Max(0, currentscore);
        PlayerPrefs.SetInt(scorekey, currentscore);
        PlayerPrefs.Save();
        Debug.Log("SCORE AFTER SUBTRACTION: " + currentscore);
        updateScoreui();
    }

    public void updateScoreui()
    {
        if (Show_Player_Score == null) return; // ✅ safe null check
        int currentscore = PlayerPrefs.GetInt(scorekey, 0);
        Show_Player_Score.text = currentscore.ToString();
    }
}