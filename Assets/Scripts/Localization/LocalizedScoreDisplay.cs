using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using System.Collections;
using TMPro;

public class LocalizedScoreDisplay : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text scoreText;

    [Header("Settings")]
    public float countDuration = 1.5f; // seconds to count up

    private int currentScore = 0;
    private int targetScore = 0;
    private Coroutine countCoroutine;

    private void OnEnable()
    {
        // Re-render when language switches
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        // Refresh display in new locale without re-counting
        UpdateDisplay(currentScore);
    }

    /// <summary>
    /// Call this to animate score from current to newScore
    /// </summary>
    public void SetScore(int newScore)
    {
        targetScore = newScore;

        if (countCoroutine != null)
            StopCoroutine(countCoroutine);

        countCoroutine = StartCoroutine(CountTo(currentScore, targetScore));
    }

    /// <summary>
    /// Set instantly with no animation
    /// </summary>
    public void SetScoreInstant(int newScore)
    {
        currentScore = newScore;
        targetScore = newScore;
        UpdateDisplay(newScore);
    }

    private IEnumerator CountTo(int from, int to)
    {
        float elapsed = 0f;

        while (elapsed < countDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / countDuration);
            float eased = EaseOutQuart(t);     // smooth deceleration
            int display = Mathf.RoundToInt(Mathf.Lerp(from, to, eased));

            UpdateDisplay(display);
            yield return null;
        }

        currentScore = to;
        UpdateDisplay(to);
    }

    private void UpdateDisplay(int value)
    {
        scoreText.text = LocalizedNumber.Format(value);
    }

    private float EaseOutQuart(float t)
    {
        return 1f - Mathf.Pow(1f - t, 4f);
    }
}