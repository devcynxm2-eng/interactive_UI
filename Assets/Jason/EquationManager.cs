using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;





[System.Serializable]
public class Pair
{
    public int a;

}

[System.Serializable]
public class EquationData
{
    public int[] operands;
    public string[] operators;
    public int answer;
    public int blank_value;
    public int hint_value;
    public int equation_length;

    // Use List of Pair instead of int[][]
    public List<Pair> valid_pairs;

}

[Serializable]
public class EquationsListWrapper
{
    public int equation_length;
    public EquationData[] equations;
}

public class EquationManager : MonoBehaviour
{
    public static EquationManager Instance { get; private set; }


    public TextAsset[] equationJSONFiles;

    [Header("Prefabs in Order (left to right)")]
    public NumEq blankPrefab;
    public MathOperatorDisplay operatorPrefab;
    public NumEq SecondblankPrefab;
    public MathOperatorDisplay SecondoperatorPrefab;
    public TMP_Text operandText;
    public MathOperatorDisplay equalPrefab;
    public TMP_Text answerText;

    [Header("Number Buttons (assign all in Inspector)")]
    public Button[] numberButtons;

    [Header("Hint Settings")]
    public Color hintHighlightColor = new Color(1f, 0.9f, 0f, 1f);
    public float hintDuration = 1.5f;

    [Header("Settings")]
    public bool avoidRepeat = true;

    // ── CHANGED: separate list per operator instead of one combined list ──
    public List<EquationData>[] equationGroups; // index 0=Add, 1=Sub, 2=Mul, 3=Div
    private int currentGroupIndex = 0;           // which operator group we are on
    private int currentEquationIndex = 0;        // which equation inside that group
    // ─────────────────────────────────────────────────────────────────────

    public EquationData currentData;
    public NumEq numEq;
    private Button currentHintButton;
    private Color originalHintButtonColor;
    private Coroutine hintCoroutine;
    public static int currentEquationLength;
    private int currentHintStep;

    // for the double hint part
    private int[] currentHintPair;
    private bool isPairSelected = false;
    private bool doubleHintUsed = false;
    public Complete_Panel Complete_Panel;

    private Coroutine scrollCoroutine;

    public AllCompletePanel allCompletePanel;

    [Header("Scroll Settings")]
    public ScrollRect numberScrollRect; // assign in Inspector

    public GameObject leveltext;
    public GameObject wordlvltext;
    public ScoreManager scoreManager;
    [Header("UI")]
    public TMP_Text equationNumberText;
    public int globalEquationIndex = 0;


    [System.Serializable]
    public class ProgressData
    {
        public int jsonIndex;
        public int equationIndex;
        public int totalSolved;
    }


    //calculatingall the jasons equation 
    public int totalEquationsInAllJSON = 0;
    public TMP_Text totalEquationText;
    public TMP_Text totalsolvedeqText;
    public Slider progressSlider;
    public int x;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }



    int currentJSONIndex = 0;
    [Header("UI")]
    public GameObject wordSolverUI;


    void Start()
    {

        //=--====================================================----------------
        CalculateTotalEquations();
        if (totalEquationText != null)
            totalEquationText.text = totalEquationsInAllJSON.ToString();


        LoadProgress();
        UpdateSlider();
        //currentJSONIndex = 0;
        LoadJSON(currentJSONIndex);

        x = globalEquationIndex;
        Debug.Log("--------------instart-----in " + currentEquationIndex);

    }

    void LoadJSON(int index)
    {
        currentJSONIndex = index;
        Debug.Log("========== Loadjson start currentindex value ========= " + currentJSONIndex);
        if (index >= equationJSONFiles.Length)
        {
            Debug.Log("All JSONs finished!");
            if (allCompletePanel != null &&
         allCompletePanel.IsValidFor(AllCompletePanel.PanelType.Equation))
            {
                allCompletePanel.Show();
            }
            return;
        }


        //Complete_Panel.ResetCompleteUI();
        equationGroups = new List<EquationData>[1]; // only ONE group

        equationGroups[0] = new List<EquationData>();

        TextAsset jsonFile = equationJSONFiles[index];

        if (jsonFile == null) return;



        EquationsListWrapper wrapper = JsonUtility.FromJson<EquationsListWrapper>(jsonFile.text);
        currentEquationLength = wrapper.equation_length;
        Debug.Log("============>" + currentEquationLength);
        if (wrapper != null && wrapper.equations != null)
        {
            equationGroups[0].AddRange(wrapper.equations);
        }



        currentGroupIndex = 0;


        LoadNextEquation();
    }


    public void showleveltext()
    {
        leveltext.gameObject.SetActive(false);
    }
    public void wordleveltext()
    {
        wordlvltext.gameObject.SetActive(false);
    }



    void CalculateTotalEquations()
    {
        totalEquationsInAllJSON = 0;

        for (int i = 0; i < equationJSONFiles.Length; i++)
        {
            if (equationJSONFiles[i] == null)
                continue;

            EquationsListWrapper wrapper =
                JsonUtility.FromJson<EquationsListWrapper>(equationJSONFiles[i].text);

            if (wrapper != null && wrapper.equations != null)
            {
                totalEquationsInAllJSON += wrapper.equations.Length;
            }
        }
        // ✅ Save it immediately so SampleScene can always read it
        PlayerPrefs.SetInt("TotalEquations", totalEquationsInAllJSON);
        PlayerPrefs.Save();
        Debug.Log("=======> TOTAL EQUATIONS IN ALL JSON: " + totalEquationsInAllJSON);
    }




    public bool HasDoubleHintAvailable()
    {
        return currentHintPair != null && !doubleHintUsed;
    }


    //public void OnHintPressed()
    //{
    //    if (currentData == null) return;

    //    // ── SINGLE BLANK ──
    //    if (currentEquationLength == 5)
    //    {
    //        ShowHintForValue(currentData.hint_value);
    //        scoreManager.Subtractscore(20);
    //        return;
    //    }

    //    // ── DOUBLE BLANK ──
    //    if (currentEquationLength == 7)
    //    {
    //        scoreManager.Subtractscore(20);
    //        if (currentData.valid_pairs == null || currentData.valid_pairs.Count == 0)
    //        {
    //            Debug.LogWarning("No valid pairs in JSON.");
    //            return;
    //        }

    //        // BLANK 1 HINT — pick random pair.a from JSON
    //        if (numEq.CurrentBlank == 1)
    //        {
    //            int randomIndex = UnityEngine.Random.Range(0, currentData.valid_pairs.Count);
    //            Pair randomPair = currentData.valid_pairs[randomIndex];

    //            currentHintPair = new int[] { randomPair.a };
    //            isPairSelected = true;

    //            Debug.Log($"[Hint Blank1] Showing random pair.a = {randomPair.a}");
    //            ShowHintForValue(randomPair.a);
    //            return;
    //        }

    //        // BLANK 2 HINT — use pre-calculated value from NumEq
    //        if (numEq.CurrentBlank == 2)
    //        {
    //            int hintB = numEq.GetBlank2HintValue();

    //            if (hintB != -1)
    //            {
    //                Debug.Log($"[Hint Blank2] Showing calculated value = {hintB}");
    //                ShowHintForValue(hintB);
    //            }
    //            else
    //            {
    //                Debug.LogWarning("[Hint Blank2] Value not calculated yet, blank1 not filled.");
    //            }
    //            return;
    //        }
    //    }
    //}



    public void OnHintPressed()
    {
        if (currentData == null) return;

        int currentScore = PlayerPrefs.GetInt("Playerscore", 0);
        if (currentScore <= 0)
        {
            // Show ad panel AND pass callback
            WatchAdPanel.Instance.Show(() =>
            {
                // After ad → give hint (no score deduction since they watched an ad)
                GiveHint();
            });
            return;
        }

        // Normal flow — deduct score then give hint
        scoreManager.Subtractscore(20);
        GiveHint();
    }

    void GiveHint()
    {
        // ── SINGLE BLANK ──
        if (currentEquationLength == 5)
        {
            ShowHintForValue(currentData.hint_value);
            return;
        }

        // ── DOUBLE BLANK ──
        if (currentEquationLength == 7)
        {
            if (currentData.valid_pairs == null || currentData.valid_pairs.Count == 0)
            {
                Debug.LogWarning("No valid pairs in JSON.");
                return;
            }

            // BLANK 1 HINT — pick random pair.a from JSON
            if (numEq.CurrentBlank == 1)
            {
                int randomIndex = UnityEngine.Random.Range(0, currentData.valid_pairs.Count);
                Pair randomPair = currentData.valid_pairs[randomIndex];
                currentHintPair = new int[] { randomPair.a };
                isPairSelected = true;
                Debug.Log($"[Hint Blank1] Showing random pair.a = {randomPair.a}");
                ShowHintForValue(randomPair.a);
                return;
            }

            // BLANK 2 HINT — use pre-calculated value from NumEq
            if (numEq.CurrentBlank == 2)
            {
                int hintB = numEq.GetBlank2HintValue();
                if (hintB != -1)
                {
                    Debug.Log($"[Hint Blank2] Showing calculated value = {hintB}");
                    ShowHintForValue(hintB);
                }
                else
                {
                    Debug.LogWarning("[Hint Blank2] Value not calculated yet, blank1 not filled.");
                }
                return;
            }
        }
    }


    void SetupNumberButtons()
    {
        List<int> finalNumbers = new List<int>();

        // ✅ SINGLE BLANK MODE
        if (currentEquationLength == 5)
        {
            finalNumbers.Add(currentData.hint_value);
        }

        // ✅ DOUBLE BLANK MODE
        else if (currentEquationLength == 7)
        {
            List<Pair> pairs = currentData.valid_pairs;

            if (pairs != null && pairs.Count > 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    Pair randomPair = pairs[UnityEngine.Random.Range(0, pairs.Count)];
                    if (!finalNumbers.Contains(randomPair.a))
                        finalNumbers.Add(randomPair.a);
                }
            }
        }

        // Fill remaining slots
        while (finalNumbers.Count < numberButtons.Length)
        {
            int rand = UnityEngine.Random.Range(0, 25);
            if (!finalNumbers.Contains(rand))
                finalNumbers.Add(rand);
        }

        // Shuffle
        for (int i = 0; i < finalNumbers.Count; i++)
        {
            int randIndex = UnityEngine.Random.Range(0, finalNumbers.Count);
            int temp = finalNumbers[i];
            finalNumbers[i] = finalNumbers[randIndex];
            finalNumbers[randIndex] = temp;
        }

        // Assign to buttons
        for (int i = 0; i < numberButtons.Length; i++)
        {
            NumberButton nb = numberButtons[i].GetComponent<NumberButton>();
            TMP_Text txt = numberButtons[i].GetComponentInChildren<TMP_Text>();
            int value = finalNumbers[i];
            nb.number = value;
            txt.text = value.ToString();
        }
    }


    private void ShowHintForValue(int targetValue)
    {
        // ✅ Stop BOTH coroutines and reset before starting new one
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }
        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
            hintCoroutine = null;
        }
        ResetHintButton();

        Button matchedButton = null;

        foreach (Button btn in numberButtons)
        {
            NumberButton nb = btn.GetComponent<NumberButton>();
            if (nb != null && nb.number == targetValue)
            {
                matchedButton = btn;
                break;
            }
        }

        if (matchedButton == null)
        {
            Debug.LogWarning($"No button found for hint value: {targetValue}");
            return;
        }

        currentHintButton = matchedButton;

        Image btnImage = matchedButton.GetComponent<Image>();
        if (btnImage != null)
            originalHintButtonColor = btnImage.color;

        if (numberScrollRect != null)
            scrollCoroutine = StartCoroutine(ScrollToButtonThenHighlight(matchedButton));
        else
            hintCoroutine = StartCoroutine(HintHighlight(matchedButton));
    }



    IEnumerator ScrollToButtonThenHighlight(Button btn)
    {
        // Wait one frame so layout is ready
        yield return null;

        RectTransform buttonRect = btn.GetComponent<RectTransform>();
        RectTransform contentRect = numberScrollRect.content;
        RectTransform viewportRect = numberScrollRect.viewport;

        // Convert button position to content local space
        Vector2 buttonLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            contentRect,
            RectTransformUtility.WorldToScreenPoint(null, buttonRect.position),
            null,
            out buttonLocalPos
        );

        // Calculate normalized scroll position (0 = top, 1 = bottom)
        float contentHeight = contentRect.rect.height;
        float viewportHeight = viewportRect.rect.height;

        if (contentHeight <= viewportHeight)
        {
            // No scrolling needed, content fits in viewport
            hintCoroutine = StartCoroutine(HintHighlight(btn));
            yield break;
        }

        // buttonLocalPos.y is negative going downward in Unity UI
        float buttonY = -buttonLocalPos.y - buttonRect.rect.height / 2f;
        float scrollableArea = contentHeight - viewportHeight;

        // Center the button in the viewport
        float targetY = buttonY - viewportHeight / 2f;
        float normalizedY = Mathf.Clamp01(targetY / scrollableArea);

        // Smoothly scroll to the button
        float elapsed = 0f;
        float scrollDuration = 0.3f;
        float startY = numberScrollRect.verticalNormalizedPosition;
        // Unity ScrollRect: 1 = top, 0 = bottom — so invert
        float targetNormalized = 1f - normalizedY;

        while (elapsed < scrollDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / scrollDuration);
            numberScrollRect.verticalNormalizedPosition = Mathf.Lerp(startY, targetNormalized, t);
            yield return null;
        }

        numberScrollRect.verticalNormalizedPosition = targetNormalized;

        // Now highlight after scrolling is done
        hintCoroutine = StartCoroutine(HintHighlight(btn));
    }



    IEnumerator HintHighlight(Button btn)
    {
        Image btnImage = btn.GetComponent<Image>();
        if (btnImage == null) yield break;

        float fadeTime = 0.25f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeTime;
            btnImage.color = Color.Lerp(originalHintButtonColor, hintHighlightColor, t);
            yield return null;
        }

        yield return new WaitForSeconds(hintDuration);

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeTime;
            btnImage.color = Color.Lerp(hintHighlightColor, originalHintButtonColor, t);
            yield return null;
        }

        btnImage.color = originalHintButtonColor;
        currentHintButton = null;
        hintCoroutine = null;
    }

    public void ResetHintButton()
    {
        if (currentHintButton == null) return;
        Image btnImage = currentHintButton.GetComponent<Image>();
        if (btnImage != null)
            btnImage.color = originalHintButtonColor;
        currentHintButton = null;
    }

    public void OnCorrectAnswer()
    {


        globalEquationIndex++;
        SaveProgress();
        // ✅ Stop everything and reset on correct answer
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }
        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
            hintCoroutine = null;
        }
        ResetHintButton();
        LoadNextEquation();
        //StartCoroutine(NextEquationDelay(1.0f));
    }


    public void SaveProgress()
    {
        ProgressData data = new ProgressData();

        data.jsonIndex = currentJSONIndex;
        data.equationIndex = currentEquationIndex;
        data.totalSolved = globalEquationIndex;

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("progress", json);





        PlayerPrefs.Save();



    }


    void LoadProgress()
    {
        if (!PlayerPrefs.HasKey("progress"))
            return;

        string json = PlayerPrefs.GetString("progress");
        ProgressData data = JsonUtility.FromJson<ProgressData>(json);

        currentJSONIndex = data.jsonIndex;
        currentEquationIndex = data.equationIndex;
        globalEquationIndex = data.totalSolved;
    }


    public void ResetAllHints()
    {
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }
        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
            hintCoroutine = null;
        }
        ResetHintButton();
    }

    public bool HasDoubleHint()
    {
        return currentHintPair != null && doubleHintUsed;
    }

    public void ShowHintForCurrentBlank()
    {
        if (numEq == null) return;

        if (numEq.CurrentBlank == 1)
        {
            if (currentHintPair != null)
                ShowHintForValue(currentHintPair[0]);
        }
        else if (numEq.CurrentBlank == 2)
        {
            int hintB = numEq.GetBlank2HintValue();
            if (hintB != -1)
                ShowHintForValue(hintB);
        }
    }


    IEnumerator NextEquationDelay(float delay)
    {
        yield return new WaitForSeconds(delay);


    }

    // ── CHANGED: replaced LoadRandomEquation() with LoadNextEquation() ──
    void LoadNextEquation()
    {
        if (equationNumberText != null)
            equationNumberText.text = (globalEquationIndex + 1).ToString();

        Debug.Log("globalEquationIndex = " + globalEquationIndex);
        if (totalsolvedeqText != null)
            totalsolvedeqText.text = (globalEquationIndex).ToString();

        // ✅ Keep PlayerPrefs in sync every equation
        PlayerPrefs.SetInt("SolvedEquations", globalEquationIndex);
        PlayerPrefs.Save();
        Debug.Log("globalEquationIndex 2 1 = " + globalEquationIndex);
        UpdateSlider();
        ResetAllHints();

        // If all groups are exhausted, loop back to addition
        if (currentGroupIndex >= equationGroups.Length)
        {
            currentGroupIndex = 0;
            currentEquationIndex = 0;
            Debug.Log("All operator groups done — looping back to Addition");
        }

        List<EquationData> currentGroup = equationGroups[currentGroupIndex];

        // If current group is finished, move to next operator group
        if (currentEquationIndex >= currentGroup.Count)
        {
            Debug.Log("Current JSON finished, loading next...");

            currentJSONIndex++;
            currentEquationIndex = 0;
            LoadJSON(currentJSONIndex);

            return;
        }

        // Load current equation in sequence
        Debug.Log("--------------before-----in " + currentEquationIndex);
        currentData = currentGroup[currentEquationIndex];
        Debug.Log("--------------afetr-----in " + currentEquationIndex);
        currentEquationIndex++;
        Debug.Log("--------------moreafter-----in " + currentEquationIndex);

        currentHintStep = 0;
        isPairSelected = false;
        currentHintPair = null;
        numEq.hintcheck = 0;



        // ✅ FIX: get length from currentData, not currentGroup[0]
        //currentEquationLength = currentData.equation_length;
        Debug.Log($"Equation length: {currentEquationLength}");

        if (currentEquationLength == 5)
        {
            SecondblankPrefab.gameObject.SetActive(false);
            SecondoperatorPrefab.gameObject.SetActive(false);


            Debug.Log("single test");
            AssignDatasingle();
        }
        else if (currentEquationLength == 7)
        {
            Debug.Log("Double test");

            AssignDatadouble();
        }

        numEq.Cross_Image.gameObject.SetActive(false);
        //numEq.change_no_Text.gameObject.SetActive(false);


        SetupNumberButtons();
    }




    // ────────────────────────────────────────────────────────────────────




    public void EnsureValueOnButton(int value)
    {
        // Check if value already exists on any button
        foreach (Button btn in numberButtons)
        {
            NumberButton nb = btn.GetComponent<NumberButton>();
            if (nb != null && nb.number == value)
            {
                Debug.Log($"[EnsureValueOnButton] Value {value} already exists on a button");
                return; // already exists, nothing to do
            }
        }

        Debug.Log($"[EnsureValueOnButton] Value {value} not found, replacing a button...");

        // Not found — find replaceable buttons
        // (buttons that are NOT a valid pair.a value)
        List<Button> replaceable = new List<Button>();

        foreach (Button btn in numberButtons)
        {
            NumberButton nb = btn.GetComponent<NumberButton>();
            if (nb == null) continue;

            bool isPairValue = false;

            if (currentData.valid_pairs != null)
            {
                foreach (var pair in currentData.valid_pairs)
                {
                    if (nb.number == pair.a)
                    {
                        isPairValue = true;
                        break;
                    }
                }
            }

            if (!isPairValue)
                replaceable.Add(btn);
        }

        if (replaceable.Count > 0)
        {
            Button targetBtn = replaceable[UnityEngine.Random.Range(0, replaceable.Count)];
            NumberButton targetNb = targetBtn.GetComponent<NumberButton>();
            TMP_Text targetTxt = targetBtn.GetComponentInChildren<TMP_Text>();

            targetNb.number = value;
            targetTxt.text = value.ToString();

            Debug.Log($"[EnsureValueOnButton] ✅ Replaced button with blank2 value = {value}");
        }
        else
        {
            Debug.LogWarning("[EnsureValueOnButton] No replaceable button found!");
        }
    }



    void AssignDatasingle()
    {

        int knownOperand = 0;
        string operatorSymbol = currentData.operators[0];

        for (int i = 0; i < currentData.operands.Length; i++)
        {
            if (currentData.operands[i] != currentData.blank_value)
                knownOperand = currentData.operands[i];
        }

        blankPrefab.ResetStatesingle();
        blankPrefab.secondNumber = knownOperand;
        blankPrefab.result = currentData.answer;
        blankPrefab.SetOperatorType(SymbolToEnum(operatorSymbol));
        blankPrefab.SetBlanksingle();

        operatorPrefab.SetOperator(SymbolToEnum(operatorSymbol));
        operandText.text = knownOperand.ToString();
        equalPrefab.SetOperator(MathOperator.Equal);
        answerText.text = currentData.answer.ToString();
    }


    void AssignDatadouble()
    {


        // Hide all prefabs initially
        blankPrefab.gameObject.SetActive(false);
        SecondblankPrefab.gameObject.SetActive(false);
        operatorPrefab.gameObject.SetActive(false);
        SecondoperatorPrefab.gameObject.SetActive(false);


        List<int> blanksIndices = new List<int>();
        List<int> knownOperands = new List<int>();

        // Find blanks and known operands
        for (int i = 0; i < currentData.operands.Length; i++)
        {
            if (currentData.operands[i] == currentData.blank_value)
                blanksIndices.Add(i);
            else
                knownOperands.Add(currentData.operands[i]);
        }

        // ✅ FIXED: Extract the constant value (the non-blank operand)
        int constantValue = 0;
        if (knownOperands.Count > 0)
            constantValue = knownOperands[knownOperands.Count - 1];

        Debug.Log($"[AssignData] blanksIndices.Count={blanksIndices.Count}, knownOperands={string.Join(",", knownOperands)}, constantValue={constantValue}");

        // Assign first blank
        if (blanksIndices.Count > 0)
        {
            blankPrefab.gameObject.SetActive(true);
            blankPrefab.result = currentData.answer;
            blankPrefab.secondNumber = constantValue;  // ✅ FIXED: Assign the constant
            blankPrefab.SetOperatorType(SymbolToEnum(currentData.operators[0]));
            Debug.Log($"[AssignData] blankPrefab: result={blankPrefab.result}, secondNumber={blankPrefab.secondNumber}, operator={currentData.operators[0]}");
            blankPrefab.ResetState();
        }

        // Assign second blank
        if (blanksIndices.Count > 1)
        {
            SecondblankPrefab.gameObject.SetActive(true);
            SecondblankPrefab.result = currentData.answer;
            SecondblankPrefab.secondNumber = constantValue;  // ✅ FIXED: Assign the constant
            if (currentData.operators.Length > 1)
                SecondblankPrefab.SetOperatorType(SymbolToEnum(currentData.operators[1]));
            Debug.Log($"[AssignData] SecondblankPrefab: result={SecondblankPrefab.result}, secondNumber={SecondblankPrefab.secondNumber}");
            SecondblankPrefab.ResetState();
        }

        // Assign operators dynamically
        if (currentData.operators.Length > 0)
        {
            operatorPrefab.gameObject.SetActive(true);
            operatorPrefab.SetOperator(SymbolToEnum(currentData.operators[0]));
        }

        if (currentData.operators.Length > 1)
        {
            SecondoperatorPrefab.gameObject.SetActive(true);
            SecondoperatorPrefab.SetOperator(SymbolToEnum(currentData.operators[1]));
        }

        // Assign known operands (the constant value)
        if (knownOperands.Count > 0)
            operandText.text = knownOperands[knownOperands.Count - 1].ToString();

        // Set equals and answer
        equalPrefab.SetOperator(MathOperator.Equal);
        answerText.text = currentData.answer.ToString();
    }
    public bool TryGetPairMatch(int firstValue, out int secondValue)
    {
        secondValue = -1;


        return false;
    }


    public void ResetHintState()
    {
        isPairSelected = false;
        currentHintPair = null;
        doubleHintUsed = false;
    }

    public void UpdateSlider()
    {
        int total = totalEquationsInAllJSON;
        int solved = globalEquationIndex;

        if (total <= 0)
        {
            progressSlider.value = 0f;
            return;
        }

        float progress = (float)solved / total;

        progressSlider.value = Mathf.Clamp01(progress);
    }


    MathOperator SymbolToEnum(string symbol)
    {
        switch (symbol)
        {
            case "+": return MathOperator.Add;
            case "-": return MathOperator.Subtract;
            case "*": return MathOperator.Multiply;
            case "/": return MathOperator.Divide;
            default: return MathOperator.Add;
        }
    }
}