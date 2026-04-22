using UnityEngine;
using UnityEngine.SceneManagement;

public class PausePanel : MonoBehaviour
{
    public GameObject pausePanel;
    public EquationManager equationManager;
    public static string targetPanel = "";
    public void OnPausePressed()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OnResumePressed()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnQuitPressed()
    {
        // ✅ Score comes from PlayerPrefs directly
        GameData.Score = PlayerPrefs.GetInt("Playerscore", 0);
        GameData.TotalEquations = equationManager.totalEquationsInAllJSON;
        GameData.SolvedEquations = equationManager.globalEquationIndex;
        targetPanel = "Main_Menu";
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }
}