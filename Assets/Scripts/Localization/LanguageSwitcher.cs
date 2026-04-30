using RTLTMPro;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Components;
using NoSuchStudio.UI;
using UnityEngine.UI;

public class LanguageSwitcher : MonoBehaviour
{
    public static LanguageSwitcher Instance;

    private bool isChanging = false;

    [Header("Fonts")]
    public TMP_FontAsset englishFont;
    public TMP_FontAsset arabicFont;
    public TMP_FontAsset chineseFont;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            string saved = PlayerPrefs.GetString("SelectedLanguage", "en");
            SetLocaleInstant(saved);
            ApplyFontToAllTexts(saved);
            StartCoroutine(ForceFullRefresh(saved));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        string saved = PlayerPrefs.GetString("SelectedLanguage", "en");
        SetLocaleInstant(saved);
        ApplyFontToAllTexts(saved);
        StartCoroutine(ForceFullRefresh(saved));
    }

    void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    IEnumerator ForceFullRefresh(string code)
    {
        yield return LocalizationSettings.InitializationOperation;

        var locale = LocalizationSettings.SelectedLocale;
        LocalizationSettings.SelectedLocale = locale;

        yield return null;

        RefreshLocalizedTexts();
        //ApplyFontToAllTexts(code);
    }

    void SetLocaleInstant(string code)
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        foreach (var locale in locales)
        {
            if (locale.Identifier.Code == code)
            {
                LocalizationSettings.SelectedLocale = locale;
                break;
            }
        }
    }

    public void SetEnglish() { ChangeLanguage("en"); }
    public void SetArabic() { ChangeLanguage("ar"); }
    public void SetChinese() { ChangeLanguage("zh-Hans"); }

    void ChangeLanguage(string code)
    {
        if (isChanging) return;
        StartCoroutine(ChangeLanguageRoutine(code));
    }

    IEnumerator ChangeLanguageRoutine(string code)
    {
        isChanging = true;

        yield return LocalizationSettings.InitializationOperation;

        var locales = LocalizationSettings.AvailableLocales.Locales;
        foreach (var locale in locales)
        {
            if (locale.Identifier.Code == code)
            {
                LocalizationSettings.SelectedLocale = locale;
                PlayerPrefs.SetString("SelectedLanguage", code);
                PlayerPrefs.Save();

                LocalizationSettings.SelectedLocale = locale;
                yield return null;

                RefreshLocalizedTexts();
                ApplyFontToAllTexts(code);
                break;
            }
        }

        isChanging = false;
    }

    void RefreshLocalizedTexts()
    {
        var localizedTexts = FindObjectsOfType<LocalizeStringEvent>(true);
        foreach (var txt in localizedTexts)
        {
            txt.RefreshString();
        }
    }


    void ApplyTextAlignment(string langCode)
    {
        var targets = FindObjectsOfType<RTLAlignTarget>(true);
        foreach (var target in targets)
        {
            var txt = target.GetComponent<TMP_Text>();
            if (txt == null) continue;

            if (langCode == "ar")
            {
                txt.alignment = TextAlignmentOptions.Right;
            }
            else
            {
                txt.alignment = TextAlignmentOptions.Left;
            }
        }
    }

    public void ApplyFontToAllTexts(string langCode)
    {
        var texts = FindObjectsOfType<TMP_Text>(true);
        foreach (var txt in texts)
        {
            if (langCode == "ar")
            {
                if (txt is RTLTextMeshPro rtl)
                {
                    rtl.font = arabicFont;
                    rtl.isRightToLeftText = true;
                }
                else
                {
                    txt.font = arabicFont;
                }
            }
            else if (langCode == "zh-Hans")
            {
                if (txt is RTLTextMeshPro rtl)
                {
                    rtl.isRightToLeftText = false;
                    rtl.font = chineseFont;
                }
                else
                {
                    txt.font = chineseFont;
                }
            }
            else
            {
                if (txt is RTLTextMeshPro rtl)
                {
                    rtl.isRightToLeftText = false;
                    rtl.font = englishFont;
                }
                else
                {
                    txt.font = englishFont;
                }
            }

            txt.ForceMeshUpdate();
            LayoutRebuilder.ForceRebuildLayoutImmediate(txt.rectTransform);
        }

        ApplyLayoutDirection(langCode);
        ApplyTextAlignment(langCode);// 🔥 flips layout groups
        Canvas.ForceUpdateCanvases();
    }

    void ApplyLayoutDirection(string langCode)
    {
        var layoutGroups = FindObjectsOfType<BidirHorizontalLayoutGroup>(true);
        foreach (var group in layoutGroups)
        {
            group.IsReverse = (langCode == "ar"); // true for Arabic, false for everything else
        }
    }

    public void ApplyCurrentLanguage()
    {
        string saved = PlayerPrefs.GetString("SelectedLanguage", "en");
        StartCoroutine(ForceFullRefresh(saved));
    }
}