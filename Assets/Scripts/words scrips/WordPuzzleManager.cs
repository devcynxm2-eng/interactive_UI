using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.Localization.Settings;






[System.Serializable]
public class WordProgressData
{
    public int currentWordIndex;
    public int globalWordIndex;
}


public class WordPuzzleManager : MonoBehaviour
{
    public static WordPuzzleManager Instance { get; private set; }

    [Header("JSON")]
    public TextAsset[] wordJSONFiles;

    [Header("UI")]
    public Transform letterTileContainer;
    public Button[] letterButtons;

    [Header("Blur Overlay")]
    public Image wordImage;
    public Image blurOverlay;

    [Header("Hint")]
    public Color hintHighlightColor = new Color(1f, 0.9f, 0f, 1f);
    public float hintDuration = 1.5f;

    [Header("Reveal Settings")]
    public float blurAmount = 15f;
    public float revealDuration = 0.6f;

    // state
    private List<WordData> allWords = new List<WordData>();
    private WordData currentWord;
    private bool answered = false;

    private Dictionary<int, char> currentDisplayMap = new Dictionary<int, char>();
    private Dictionary<int, char> blankMap = new Dictionary<int, char>();
    private List<int> orderedBlankIndices = new List<int>();

    private GameObject blankTile;
    public GameObject shaking_container;
    public GameObject redworongcontainerimage;

    // hint
    private Button currentHintButton;
    private Color originalHintColor;
    private Coroutine hintCoroutine;

    // progress
    private int currentWordIndex = 0;
    public int globalWordIndex = 0;



    [Header("Completion UI")]
    //public GameObject allCompletePanel;


    [Header("Tile Prefabs")]
    public GameObject[] letterPrefabs;
    public GameObject blankPrefab;

    [Header("Progress UI")]
    public ProgressBar progressBarController;

    [Header("Scroll Settings")]
    public ScrollRect letterScrollRect;
    private Coroutine scrollCoroutine;


    [Header("UI Progress")]
    public TMP_Text wordNumberText;
    public TMP_Text totalWordsText;
    public GameObject Oncorrectgreen;

    public Complete_Panel Complete_Panel;
    public ScoreManager scoreManager;
    //public SoundManager soundManager;
    [Header("otherscriptss")]
    public restart_timer timer;
    public AllCompletePanel allcompletePanel;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
    }

    void Start()
    {
        Time.timeScale = 1f;
        LoadAllWords();
        LoadProgress();

        // SET TOTAL HERE — currently missing
        //if (totalWordsText != null)
        //    totalWordsText.text = allWords.Count.ToString();

        if (totalWordsText != null)
            totalWordsText.text = LocalizedNumber.FormatPlain(allWords.Count);

        UpdateSlider();
        LoadNextWord();
        scoreManager.updateScoreui();
        StartCoroutine(InitAfterLocale());
        //allCompletePanel.gameObject.SetActive(false);
    }

    void LoadAllWords()
    {
        foreach (TextAsset file in wordJSONFiles)
        {
            if (file == null) continue;
            WordsListWrapper wrapper = JsonUtility.FromJson<WordsListWrapper>(file.text);
            if (wrapper?.words != null)
                allWords.AddRange(wrapper.words);
        }
        Debug.Log($"[WordPuzzleManager] Loaded {allWords.Count} words total.");
    }

    public void LoadNextWord()
    {


        if (currentWordIndex >= allWords.Count)
        {
            timer.StopTimerAndProgress();
            Debug.Log("All words completed.");
            if (allcompletePanel != null &&
                allcompletePanel.IsValidFor(AllCompletePanel.PanelType.Word))
            {
                allcompletePanel.Show();
            }
            return;
        }

        answered = false;
        currentWord = allWords[currentWordIndex];
        
        currentWordIndex++;
        globalWordIndex = currentWordIndex;

        blankMap.Clear();
        orderedBlankIndices.Clear();

        int len = currentWord.word.Length;
        int blankCount = 1;

        if (len == 4) blankCount = Random.Range(1, 3);
        else if (len >= 5) blankCount = Random.Range(1, 3);

        while (blankMap.Count < blankCount)
        {
            int idx = Random.Range(0, len);
            if (!blankMap.ContainsKey(idx))
                blankMap.Add(idx, currentWord.word[idx]);
        }

        orderedBlankIndices.AddRange(blankMap.Keys);
        orderedBlankIndices.Sort();

        Debug.Log($"Word: {currentWord.word}, Blanks: {string.Join(",", blankMap)}");

        BuildLetterTiles();
        LoadWordImage();
        SetupLetterButtons();


        //if (wordNumberText != null)
        //    wordNumberText.text = currentWordIndex.ToString();

        if (wordNumberText != null)
            wordNumberText.text = LocalizedNumber.FormatPlain(currentWordIndex);
    }

    void BuildLetterTiles()
    {
        string word = currentWord.word;

        for (int i = 0; i < letterPrefabs.Length; i++)
        {
            GameObject tile = letterPrefabs[i];
            TMP_Text txt = tile.GetComponentInChildren<TMP_Text>(true);
            Image bg = tile.GetComponent<Image>();

            if (i < word.Length)
            {
                if (blankMap.ContainsKey(i))
                {
                    txt.text = " ";
                    if (bg != null) bg.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                }
                else
                {
                    txt.text = word[i].ToString();
                    if (bg != null) bg.color = Color.white;
                }

                tile.SetActive(true);
            }
            else
            {
                tile.SetActive(false);
            }
        }
    }

    void LoadWordImage()
    {
        if (wordImage == null) return;

        Sprite sprite = Resources.Load<Sprite>($"WordImages/{currentWord.image}");

        if (sprite == null)
        {
            Debug.LogWarning($"Image not found: {currentWord.image}");
            return;
        }

        wordImage.sprite = sprite;

        Texture2D pixelTex = CreatePixelated(sprite.texture, GetBlurAlpha());
        Sprite pixelSprite = Sprite.Create(
            pixelTex,
            new Rect(0, 0, pixelTex.width, pixelTex.height),
            new Vector2(0.5f, 0.5f)
        );

        blurOverlay.sprite = pixelSprite;
        blurOverlay.color = new Color(1, 1, 1, 1);
        blurOverlay.gameObject.SetActive(true);
    }

    void SetImageBlur(bool blurred)
    {
        if (blurOverlay == null) return;
        blurOverlay.gameObject.SetActive(blurred);

        if (blurred)
        {
            float alpha = GetBlurAlpha();
            Color c = blurOverlay.color;
            c.a = alpha;
            blurOverlay.color = c;
        }
    }

    int GetBlurAlpha()
    {
        int len = currentWord.word.Length;
        if (len <= 3) return 60;
        if (len == 4) return 55;
        if (len == 5) return 50;
        return 75;
    }

    Texture2D CreatePixelated(Texture2D source, int factor)
    {
        int width = source.width;
        int height = source.height;
        Texture2D result = new Texture2D(width, height);
        result.filterMode = FilterMode.Point;

        for (int y = 0; y < height; y += factor)
        {
            for (int x = 0; x < width; x += factor)
            {
                Color c = source.GetPixel(x, y);
                for (int yy = 0; yy < factor; yy++)
                    for (int xx = 0; xx < factor; xx++)
                        if (x + xx < width && y + yy < height)
                            result.SetPixel(x + xx, y + yy, c);
            }
        }

        result.Apply();
        return result;
    }

    // ── Called by letter buttons ──
    public void OnLetterPressed(char letter)
    {
        if (answered) return;

        bool exists = false;
        foreach (var kvp in blankMap)
        {
            if (kvp.Value == letter)
            {
                exists = true;
                break;
            }
        }

        if (exists)
        {
            scoreManager.addscore(10);
            HandleCorrect(letter);
        }
        else
            HandleWrong(letter);
    }

    void HandleCorrect(char letter)
    {
        SoundManager.Instance.PlayCorrect();
        int filledIndex = -1;

        for (int i = 0; i < orderedBlankIndices.Count; i++)
        {
            int idx = orderedBlankIndices[i];
            if (blankMap.ContainsKey(idx) && blankMap[idx] == letter)
            {
                filledIndex = idx;
                orderedBlankIndices.RemoveAt(i);
                break;
            }
        }

        if (filledIndex != -1)
        {
            GameObject tile = letterPrefabs[filledIndex];
            TMP_Text txt = tile.GetComponentInChildren<TMP_Text>();
            Image bg = tile.GetComponent<Image>();

            txt.text = letter.ToString();
            if (bg != null) bg.color = new Color(0.7f, 1f, 0.7f, 1f);

            blankMap.Remove(filledIndex);
        }

        if (blankMap.Count == 0)
        {
            answered = true;
            SaveProgress();
            if (Complete_Panel != null)
            {
                Complete_Panel.gameObject.SetActive(true);
                StartCoroutine(WordCompleteSequence());
                //StartCoroutine(Complete_Panel.DisplayRoutine(2f));
            }
            else
            {
                Debug.LogError("Complete_Panel is NULL");
            }
          
            StartCoroutine(RevealImage());
        }
    }
    private IEnumerator WordCompleteSequence()
    {
        // 1. show green feedback
        if (Oncorrectgreen != null)
            Oncorrectgreen.SetActive(true);

        yield return new WaitForSeconds(1.0f);

        // 2. hide green
        if (Oncorrectgreen != null)
            Oncorrectgreen.SetActive(false);

        // 3. show complete panel AFTER green
        if (Complete_Panel != null)
        {
            Complete_Panel.gameObject.SetActive(true);
            if (timer != null)
                timer.StopTimerAndProgress();
            yield return StartCoroutine(Complete_Panel.DisplayRoutine(0.1f));
        }

        // 4. optional: auto next word
         //LoadNextWord(); // ← only if you want auto progressionh
    }
    // ── Wrong: show pressed letter in blank briefly then clear ──
    void HandleWrong(char letter)
    {
        SoundManager.Instance.PlayWrong();
        StartCoroutine(ShakeTile(shaking_container.GetComponent<RectTransform>()));
        StartCoroutine(FlashBlankRedWithLetter(letter));
    }

    IEnumerator FlashBlankRedWithLetter(char letter)
    {
        if (orderedBlankIndices.Count == 0) yield break;

        int currentBlankIndex = orderedBlankIndices[0];
        GameObject tile = letterPrefabs[currentBlankIndex];
        if (tile == null) yield break;

        Image bg = tile.GetComponent<Image>();
        TMP_Text txt = tile.GetComponentInChildren<TMP_Text>();
        if (bg == null) yield break;

        Color orig = bg.color;

        // Show wrong letter in red
        if (txt != null) txt.text = letter.ToString();
        if (redworongcontainerimage != null) redworongcontainerimage.SetActive(true);
        bg.color = new Color(1f, 0.5f, 0.5f, 1f);

        yield return new WaitForSeconds(0.5f);

        // Restore back to empty blank
        if (txt != null) txt.text = " ";
        bg.color = orig;
        if (redworongcontainerimage != null) redworongcontainerimage.SetActive(false);
    }

    IEnumerator RevealImage()
    {
        if (blurOverlay == null) yield break;

        float duration = 0.6f;
        float t = 0f;
        Color c = blurOverlay.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / duration);
            blurOverlay.color = c;
            yield return null;
        }

        c.a = 0f;
        blurOverlay.color = c;
    }

    IEnumerator NextWordDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadNextWord();
    }

    IEnumerator ShakeTile(RectTransform rt)
    {
        if (rt == null) yield break;
        Vector2 original = rt.anchoredPosition;
        float duration = 0.4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Mathf.Sin(elapsed * 60f) * 8f;
            rt.anchoredPosition = original + new Vector2(x, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rt.anchoredPosition = original;
    }

    void SetupLetterButtons()
    {
        List<char> letters = new List<char>();

        foreach (char c in blankMap.Values)
            if (!letters.Contains(c)) letters.Add(c);

        foreach (char c in currentWord.word)
            if (!blankMap.ContainsValue(c) && !letters.Contains(c)) letters.Add(c);

        while (letters.Count < letterButtons.Length)
        {
            char rand = (char)Random.Range('A', 'Z' + 1);
            if (!letters.Contains(rand)) letters.Add(rand);
        }

        for (int i = 0; i < letters.Count; i++)
        {
            int r = Random.Range(0, letters.Count);
            (letters[i], letters[r]) = (letters[r], letters[i]);
        }

        for (int i = 0; i < letterButtons.Length; i++)
        {
            Button btn = letterButtons[i];
            TMP_Text txt = btn.GetComponentInChildren<TMP_Text>();
            char assignedLetter = letters[i];
            txt.text = assignedLetter.ToString();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnLetterPressed(assignedLetter));
        }
    }

    
    //public void OnHintPressed()
    //{
    //    if (currentWord == null) return;
    //    if (blankMap.Count > 0 && orderedBlankIndices.Count > 0)
    //    {
    //        scoreManager.Subtractscore(20);
    //        int idx = orderedBlankIndices[0];
    //        ShowHintForLetter(blankMap[idx]);
    //    }
    //}


    public void OnHintPressed()
    {
        if (currentWord == null) return;

        int currentScore = PlayerPrefs.GetInt("Playerscore", 0);
        //int currentScore = PlayerPrefs.GetInt("Playerscore", 0);
        Debug.Log($"[Hint] Score check: {currentScore}"); // ← ADD THIS
        if (currentScore <= 0)
        {

            if (timer != null)
                timer.StopTimerAndProgress();


            // Show ad panel AND pass callback
            WatchAdPanel.Instance.Show(() =>
            {
                // After ad → give hint (no score deduction)
                GiveWordHint();
            });
            return;
        }

        // Normal flow — deduct score then give hint
        scoreManager.Subtractscore(20);
        GiveWordHint();
    }

    void GiveWordHint()
    {
        if (blankMap.Count > 0 && orderedBlankIndices.Count > 0)
        {
            int idx = orderedBlankIndices[0];
            ShowHintForLetter(blankMap[idx]);
        }
    }
    
        


void ShowHintForLetter(char letter)
    {
        if (hintCoroutine != null) { StopCoroutine(hintCoroutine); hintCoroutine = null; }
        if (scrollCoroutine != null) { StopCoroutine(scrollCoroutine); scrollCoroutine = null; }

        Button match = null;
        foreach (Button btn in letterButtons)
        {
            TMP_Text txt = btn.GetComponentInChildren<TMP_Text>();
            if (txt != null && txt.text == letter.ToString())
            {
                match = btn;
                break;
            }
        }

        if (match == null)
        {
            Debug.LogWarning($"No button found for letter: {letter}");
            return;
        }

        ResetHintButton();
        currentHintButton = match;

        Image img = match.GetComponent<Image>();
        if (img != null) originalHintColor = img.color;

        if (letterScrollRect != null)
            scrollCoroutine = StartCoroutine(ScrollToButtonThenHighlight(match));
        else
            hintCoroutine = StartCoroutine(HintHighlight(match));
    }

    IEnumerator ScrollToButtonThenHighlight(Button btn)
    {
        yield return null;

        RectTransform buttonRect = btn.GetComponent<RectTransform>();
        RectTransform contentRect = letterScrollRect.content;
        RectTransform viewportRect = letterScrollRect.viewport;

        Vector2 buttonLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            contentRect,
            RectTransformUtility.WorldToScreenPoint(null, buttonRect.position),
            null,
            out buttonLocalPos
        );

        float contentHeight = contentRect.rect.height;
        float viewportHeight = viewportRect.rect.height;

        if (contentHeight <= viewportHeight)
        {
            hintCoroutine = StartCoroutine(HintHighlight(btn));
            yield break;
        }

        float buttonY = -buttonLocalPos.y - buttonRect.rect.height / 2f;
        float scrollableArea = contentHeight - viewportHeight;
        float targetY = buttonY - viewportHeight / 2f;
        float normalizedY = Mathf.Clamp01(targetY / scrollableArea);
        float targetNormalized = 1f - normalizedY;

        float start = letterScrollRect.verticalNormalizedPosition;
        float duration = 0.3f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.SmoothStep(0f, 1f, t / duration);
            letterScrollRect.verticalNormalizedPosition = Mathf.Lerp(start, targetNormalized, lerp);
            yield return null;
        }

        letterScrollRect.verticalNormalizedPosition = targetNormalized;
        hintCoroutine = StartCoroutine(HintHighlight(btn));
    }

    IEnumerator HintHighlight(Button btn)
    {
        Image img = btn.GetComponent<Image>();
        if (img == null) yield break;

        float t = 0f;
        while (t < 1f) { t += Time.deltaTime / 0.25f; img.color = Color.Lerp(originalHintColor, hintHighlightColor, t); yield return null; }
        yield return new WaitForSeconds(hintDuration);
        t = 0f;
        while (t < 1f) { t += Time.deltaTime / 0.25f; img.color = Color.Lerp(hintHighlightColor, originalHintColor, t); yield return null; }

        img.color = originalHintColor;
        currentHintButton = null;
        hintCoroutine = null;
    }

    void ResetHintButton()
    {
        if (currentHintButton == null) return;
        Image img = currentHintButton.GetComponent<Image>();
        if (img != null) img.color = originalHintColor;
        currentHintButton = null;
    }

    void UpdateSlider()
    {
        progressBarController.restartprogress();
    }

    // ── Called on Time Up reset ──
    public void ClearTiles()
    {
        for (int i = 0; i < letterPrefabs.Length; i++)
        {
            TMP_Text txt = letterPrefabs[i].GetComponentInChildren<TMP_Text>(true);
            Image bg = letterPrefabs[i].GetComponent<Image>();

            if (txt != null) txt.text = "";
            if (bg != null) bg.color = Color.white;

            letterPrefabs[i].SetActive(false);
        }
    }

    public void ResetWordState()
    {
        answered = false;
        blankMap.Clear();
        orderedBlankIndices.Clear();

        // Hide red wrong container if visible
        if (redworongcontainerimage != null)
            redworongcontainerimage.SetActive(false);

        if (hintCoroutine != null) { StopCoroutine(hintCoroutine); hintCoroutine = null; }
        if (scrollCoroutine != null) { StopCoroutine(scrollCoroutine); scrollCoroutine = null; }

        ResetHintButton();
    }

    public void OnWordCompleted()
    {
        ResetWordState();
        LoadNextWord();
    }



    public void OnTimeUp()
    {
        ClearTiles();
        ResetWordState();
    }
    public void SaveProgress()
    {
        WordProgressData data = new WordProgressData();

        data.currentWordIndex = currentWordIndex;
        data.globalWordIndex = globalWordIndex;

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("word_progress", json);
        PlayerPrefs.Save();
    }
    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        StartCoroutine(RefreshAfterLocaleChange());
    }

    private IEnumerator RefreshAfterLocaleChange()
    {
        yield return null;
        if (wordNumberText != null)
            wordNumberText.text = LocalizedNumber.FormatPlain(currentWordIndex);
        if (totalWordsText != null)
            totalWordsText.text = LocalizedNumber.FormatPlain(allWords.Count);
    }

    void LoadProgress()
    {
        if (!PlayerPrefs.HasKey("word_progress"))
            return;

        string json = PlayerPrefs.GetString("word_progress");
        WordProgressData data = JsonUtility.FromJson<WordProgressData>(json);

        currentWordIndex = data.currentWordIndex;
        globalWordIndex = data.globalWordIndex;
    }
    private IEnumerator InitAfterLocale()
    {
        yield return LocalizationSettings.InitializationOperation;

        if (totalWordsText != null)
            totalWordsText.text = LocalizedNumber.FormatPlain(allWords.Count);

        if (wordNumberText != null)
            wordNumberText.text = LocalizedNumber.FormatPlain(currentWordIndex);
    }

}