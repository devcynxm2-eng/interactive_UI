using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressBarUI : MonoBehaviour
{
    [Header("References")]
    public GameDatabase database;
    public Slider progressSlider;       // UI Slider (0 to 1)
    public TMP_Text progressLabel;      // e.g. "Progress: 3 / 10 items"
    public Image fillImage;             // Optional: color changes at milestones

    [Header("Colors")]
    public Color lowColor = new Color(0.9f, 0.3f, 0.3f);   // Red
    public Color midColor = new Color(1f, 0.8f, 0.1f);   // Yellow
    public Color highColor = new Color(0.2f, 0.85f, 0.4f);  // Green

    void OnEnable()
    {
        UnlockManager.OnItemUnlocked += UpdateProgress;
    }

    void OnDisable()
    {
        UnlockManager.OnItemUnlocked -= UpdateProgress;
    }

    void Start()
    {
        UpdateProgress();
    }

    public void UpdateProgress()
    {
        if (database == null || progressSlider == null) return;

        int totalItems = 0;
        int unlockedItems = 0;

        foreach (var category in database.categories)
        {
            foreach (var item in category.items)
            {
                totalItems++;
                if (!item.isLocked) unlockedItems++;
            }
        }

        float ratio = totalItems > 0 ? (float)unlockedItems / totalItems : 0f;

        // Animate the slider
        progressSlider.value = ratio;

        // Update label
        if (progressLabel != null)
            progressLabel.text = $"Progress: {unlockedItems} / {totalItems} items unlocked";

        // Color feedback based on progress
        if (fillImage != null)
        {
            if (ratio < 0.33f) fillImage.color = lowColor;
            else if (ratio < 0.66f) fillImage.color = midColor;
            else fillImage.color = highColor;
        }

        Debug.Log($"📊 Progress updated: {unlockedItems}/{totalItems} ({ratio * 100f:F1}%)");
    }

    /// <summary>
    /// Call this after a correct upgrade answer to boost progress visually.
    /// Pass the progressValue from the item that was upgraded.
    /// </summary>
    public void ApplyUpgradeBoost(int progressValue)
    {
        if (progressSlider == null) return;

        float boost = progressValue / 100f;
        progressSlider.value = Mathf.Clamp01(progressSlider.value + boost);

        if (progressLabel != null)
            progressLabel.text = $"⬆️ Upgrade applied! +{progressValue} progress";

        Debug.Log($"⬆️ Upgrade boost applied: +{progressValue}");

        // Refresh true state after a moment
        Invoke(nameof(UpdateProgress), 1.5f);
    }
}