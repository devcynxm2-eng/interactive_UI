using UnityEngine;

/// <summary>
/// Attach to any GameObject in the scene.
/// Wire your shared Next button onClick  → OnNextPressed()
/// Wire your shared Hint button onClick  → OnHintPressed()
/// </summary>
public class SharedUIHandler : MonoBehaviour
{
    public void OnNextPressed()
    {
        switch (GameSession.Current)
        {
            case ActiveGame.Equation:
                GameManager.Instance.OnNextButtonPressed();
                break;

            case ActiveGame.Word:
                GameManager.Instance.OnNextWordButtonPressed();
                break;

            default:
                Debug.LogWarning("[SharedUIHandler] OnNextPressed — no active game set.");
                break;
        }
    }

    public void OnHintPressed()
    {
        switch (GameSession.Current)
        {
            case ActiveGame.Equation:
                EquationManager.Instance.OnHintPressed();
                break;

            case ActiveGame.Word:
                WordPuzzleManager.Instance.OnHintPressed();
                break;

            default:
                Debug.LogWarning("[SharedUIHandler] OnHintPressed — no active game set.");
                break;
        }
    }
}