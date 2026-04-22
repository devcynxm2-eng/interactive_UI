using UnityEngine;
using UnityEngine.SceneManagement;

public class AllCompletePanel : MonoBehaviour
{
    public enum PanelType
    {
        Equation,
        Word
    }

    [Header("Panel Type")]
    public PanelType panelType;   // ✅ THIS WAS MISSING
    
    public void Show()
    {
        gameObject.SetActive(true);
        //Time.timeScale = 0f;
    }

    public bool IsValidFor(PanelType type)
    {
        return panelType == type;
    }

    public void OnMainMenuPressed()
    {
        Time.timeScale = 1f;
        
        PlayerPrefs.SetInt("SolvedEquations", PlayerPrefs.GetInt("SolvedEquations", 0));
        PlayerPrefs.Save();

        SceneManager.LoadScene("SampleScene");
    }
}