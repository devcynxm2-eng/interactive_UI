using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    [Header("Text Fields")]
    public TMP_Text scoreTxt;
    public TMP_Text totalEquationsTxt;
    public TMP_Text solvedEquationsTxt;

    [Header("Progress Bar")]
    public Slider progressSlider;

    void Start()
    {
        int score = PlayerPrefs.GetInt("Playerscore", 0);
        int total = PlayerPrefs.GetInt("TotalEquations", 0);
        int solved = PlayerPrefs.GetInt("SolvedEquations", 0);

        scoreTxt.text = score.ToString();
        totalEquationsTxt.text = total.ToString();
        solvedEquationsTxt.text = solved.ToString();

        // ✅ Fill slider based on solved vs total
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.interactable = false;

            // avoid divide by zero
            progressSlider.value = total > 0 ? (float)solved / total : 0f;
        }
    }
}