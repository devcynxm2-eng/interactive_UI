using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Components;
using TMPro;

public class Room_Pick_Manager : MonoBehaviour
{
    public RectTransform[] images;

    private Vector3 normalScale = Vector3.one;
    private Vector3 zoomedScale = new Vector3(1.05f, 1.05f, 1.05f);
    private int currentZoomedIndex = -1;

    public float tweenDuration = 0.3f;
    public Ease easeType = Ease.OutBack;

    private Coroutine refreshRoutine;

    // ✅ CACHED REFERENCES (performance boost)
    private LocalizeStringEvent[] localizedTexts;
    private TMP_Text[] allTexts;

    private bool hasRefreshed = false;

    void Start()
    {
        // 🔥 Cache once (NO repeated searching)
        localizedTexts = GetComponentsInChildren<LocalizeStringEvent>(true);
        allTexts = GetComponentsInChildren<TMP_Text>(true);

        // 🔥 Run only once
        if (!hasRefreshed)
        {
            hasRefreshed = true;
            refreshRoutine = StartCoroutine(RefreshLocalization());
        }
    }

    void OnDisable()
    {
        if (refreshRoutine != null)
            StopCoroutine(refreshRoutine);
    }

    IEnumerator RefreshLocalization()
    {
        yield return LocalizationSettings.InitializationOperation;
        yield return null;

        // 🔥 1. Refresh all localized texts
        foreach (var txt in localizedTexts)
        {
            if (txt != null)
                txt.RefreshString();
        }

        // 🔥 2. Apply font ONCE (not inside loop)
        string langCode = LocalizationSettings.SelectedLocale.Identifier.Code;
        LanguageSwitcher.Instance.ApplyFontToAllTexts(langCode);

        // 🔥 3. Force UI update
        Canvas.ForceUpdateCanvases();

        foreach (var t in allTexts)
        {
            if (t != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(t.rectTransform);
        }
    }
    // =========================
    // YOUR ORIGINAL CODE
    // =========================

    public void ZoomImage(int index)
    {
        if (index < 0 || index >= images.Length) return;

        if (currentZoomedIndex == index)
        {
            ResetAllImages();
            currentZoomedIndex = -1;
            return;
        }

        ResetAllImages();

        if (images[index] == null) return;

        images[index].DOKill();
        images[index].localScale = normalScale;

        images[index]
            .DOScale(zoomedScale, tweenDuration)
            .SetEase(easeType)
            .SetLink(images[index].gameObject);

        currentZoomedIndex = index;
    }

    void ResetAllImages()
    {
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] == null) continue;

            images[i].DOKill();
            images[i].localScale = normalScale;
        }
    }

    void OnDestroy()
    {
        ResetAllImages();
    }
}