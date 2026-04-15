using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{


    //player score part by usinh playerpref
    [Header(" Show Player Score ")]
    public TMP_Text Show_Player_Score;
    private string scorekey = "Playerscore";

    public void addscore(int pointtoadd)
    {
        int currentscore = PlayerPrefs.GetInt(scorekey, 0);

        currentscore += pointtoadd;

        PlayerPrefs.SetInt(scorekey, currentscore);

        PlayerPrefs.Save();

        Debug.Log(" SCORE ADDED BY CORRECT EQUATION ----------=============> " + currentscore);
        updateScoreui();
    }


    public void Subtractscore(int pointtoadd)
    {
        int currentscore = PlayerPrefs.GetInt(scorekey, 0);
        currentscore -= pointtoadd;
        currentscore = Mathf.Max(0, currentscore); // ✅ never goes below 0
        PlayerPrefs.SetInt(scorekey, currentscore);
        PlayerPrefs.Save();
        Debug.Log("SCORE AFTER SUBTRACTION: " + currentscore);
        updateScoreui();
    }


    public void updateScoreui()
    {
        int currentscore = PlayerPrefs.GetInt(scorekey, 0);
        Show_Player_Score.text = currentscore.ToString();
    }


}
