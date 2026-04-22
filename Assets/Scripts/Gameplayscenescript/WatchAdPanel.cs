using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class WatchAdPanel : MonoBehaviour
{
    public static WatchAdPanel Instance;

    public GameObject panelUI;
    public TextMeshProUGUI statusText; // ← assign in Inspector (optional)

    private Action onRewardComplete;

    void Awake()
    {
        Instance = this;
    }

    public void Show(Action onComplete)
    {
        onRewardComplete = onComplete;

        // Reset status text
        if (statusText != null)
            statusText.text = "Watch an ad to get a hint!";

        panelUI.SetActive(true);
    }

    public void Hide()
    {
        panelUI.SetActive(false);
    }

    public void OnWatchAdPressed()
    {
        // Show loading feedback while ad loads
        if (statusText != null)
            statusText.text = "Loading ad, please wait...";

        GoogleAdmobAutoLoad.Instance.ShowRewarded(
            onRewarded: () =>
            {
                // ✅ Give 100 coins
                ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
                if (scoreManager != null)
                    scoreManager.addscore(100);

                // ✅ Continue hint
                onRewardComplete?.Invoke();

                Hide();
            },
            onFailed: () =>
            {
                // ❌ Ad not available — show message, keep panel open
                if (statusText != null)
                    statusText.text = "No ad available right now.\nPlease try again later.";

                Debug.Log("Ad failed or not available.");

                // Auto-hide panel after 2 seconds
                Invoke(nameof(Hide), 2f);
            }
        );
    }

    public void OnClosePressed()
    {
        Hide();
    }
}