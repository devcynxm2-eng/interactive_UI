using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GamePlaySceneController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject solveEquationPart;
    public GameObject solveEQCOMPLETEPANEL;
    public GameObject solvewordCOMPLETEPANEL;
    public GameObject solveWordPuzzlePart;

    [Header("Managers")]
    public GameObject equationManagerGO;
    public GameObject wordManagerGO;

    [Header("References")]
    public ProgressBar progressBarController;
    public restart_timer timer;
    public EquationManager equationManager;

    

    void Start()
    {
        ActiveGame activeGame = GameSession.Current;

        if (activeGame == ActiveGame.Equation)
        {
            solveEquationPart.SetActive(true);
            equationManagerGO.SetActive(true);
            
            solveWordPuzzlePart.SetActive(false);
            solvewordCOMPLETEPANEL.SetActive(false);
            wordManagerGO.SetActive(false);
            equationManager.wordleveltext();
        }
        else if (activeGame == ActiveGame.Word)
        {
            solveWordPuzzlePart.SetActive(true);
            wordManagerGO.SetActive(true);
        
            solveEquationPart.SetActive(false);
            solveEQCOMPLETEPANEL.SetActive(false);
            equationManagerGO.SetActive(false);
            equationManager.showleveltext();
        }

        progressBarController.restartprogress();
        timer.RestartTimerExternally();

        // 🔥 ADD THIS — apply font AFTER all objects are active
        if (LanguageSwitcher.Instance != null)
        {
            LanguageSwitcher.Instance.ApplyCurrentLanguage();
        }
    }

    public void OnBackToMenuPressed()
    {
        DOTween.KillAll(); // ✅ kills all tweens before scene change
        GameSession.SetActiveGame(ActiveGame.None);
        timer.StopTimerAndProgress();
        SceneManager.LoadScene("SampleScene"); // ✅ correct name
    }
}