using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[Serializable]
public class EquationData
{
    public int[] operands;
    public string[] operators;
    public int answer;
    public int blank_value;
    public int hint_value;
}

[Serializable]
public class EquationsListWrapper
{
    public EquationData[] equations;
}

public class EquationManager : MonoBehaviour
{
    public static EquationManager Instance { get; private set; }

    [Header("JSON Files (assign in ORDER: Addition, Subtraction, Multiply, Divide)")]
    public TextAsset[] equationJSONFiles; // ← CHANGED: order now matters, slot 0=Add, 1=Sub, 2=Mul, 3=Div

    [Header("Prefabs in Order (left to right)")]
    public NumEq blankPrefab;
    public MathOperatorDisplay operatorPrefab;
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
    private List<EquationData>[] equationGroups; // index 0=Add, 1=Sub, 2=Mul, 3=Div
    private int currentGroupIndex = 0;           // which operator group we are on
    private int currentEquationIndex = 0;        // which equation inside that group
    // ─────────────────────────────────────────────────────────────────────

    private EquationData currentData;

    private Button currentHintButton;
    private Color originalHintButtonColor;
    private Coroutine hintCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // ── CHANGED: load each JSON into its own group ──
        equationGroups = new List<EquationData>[equationJSONFiles.Length];

        for (int i = 0; i < equationJSONFiles.Length; i++)
        {
            equationGroups[i] = new List<EquationData>();

            if (equationJSONFiles[i] == null) continue;

            EquationsListWrapper wrapper = JsonUtility.FromJson<EquationsListWrapper>(equationJSONFiles[i].text);
            if (wrapper != null && wrapper.equations != null)
                equationGroups[i].AddRange(wrapper.equations);

            Debug.Log($"Group {i} loaded: {equationGroups[i].Count} equations");
        }
        // ───────────────────────────────────────────────

        currentGroupIndex = 0;
        currentEquationIndex = 0;
        LoadNextEquation();
    }

    public void OnHintPressed()
    {
        if (currentData == null) return;

        int targetValue = currentData.hint_value;

        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
            ResetHintButton();
        }

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
            Debug.LogWarning($"No button found for hint_value: {targetValue}");
            return;
        }

        currentHintButton = matchedButton;
        Image btnImage = matchedButton.GetComponent<Image>();
        if (btnImage != null)
            originalHintButtonColor = btnImage.color;

        hintCoroutine = StartCoroutine(HintHighlight(matchedButton));
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

    void ResetHintButton()
    {
        if (currentHintButton == null) return;
        Image btnImage = currentHintButton.GetComponent<Image>();
        if (btnImage != null)
            btnImage.color = originalHintButtonColor;
        currentHintButton = null;
    }

    public void OnCorrectAnswer()
    {
        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
            ResetHintButton();
        }
        StartCoroutine(NextEquationDelay(1.0f));
    }

    IEnumerator NextEquationDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadNextEquation();
    }

    // ── CHANGED: replaced LoadRandomEquation() with LoadNextEquation() ──
    void LoadNextEquation()
    {
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
            currentGroupIndex++;
            currentEquationIndex = 0;
            Debug.Log($"Moving to next operator group: {currentGroupIndex}");
            LoadNextEquation(); // recurse to load from new group
            return;
        }

        // Load current equation in sequence
        currentData = currentGroup[currentEquationIndex];
        currentEquationIndex++;
        Debug.Log($"Group {currentGroupIndex}, Equation {currentEquationIndex}/{currentGroup.Count}");
        AssignData();
    }
    // ────────────────────────────────────────────────────────────────────

    void AssignData()
    {
        int knownOperand = 0;
        string operatorSymbol = currentData.operators[0];

        for (int i = 0; i < currentData.operands.Length; i++)
        {
            if (currentData.operands[i] != currentData.blank_value)
                knownOperand = currentData.operands[i];
        }

        blankPrefab.ResetState();
        blankPrefab.secondNumber = knownOperand;
        blankPrefab.result = currentData.answer;
        blankPrefab.SetOperatorType(SymbolToEnum(operatorSymbol));
        blankPrefab.SetBlank();

        operatorPrefab.SetOperator(SymbolToEnum(operatorSymbol));
        operandText.text = knownOperand.ToString();
        equalPrefab.SetOperator(MathOperator.Equal);
        answerText.text = currentData.answer.ToString();
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

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using TMPro;

//[Serializable]
//public class EquationData
//{
//    public int[] operands;
//    public string[] operators;
//    public int answer;
//    public int blank_value;
//}

//[Serializable]
//public class EquationsListWrapper
//{
//    public EquationData[] equations;
//}

//public class EquationManager : MonoBehaviour
//{
//    public static EquationManager Instance { get; private set; }

//    [Header("JSON Files (assign all 4)")]
//    public TextAsset[] equationJSONFiles; // drag all 4 JSON files here in Inspector

//    [Header("Prefabs in Order (left to right)")]
//    public NumEq blankPrefab;
//    public MathOperatorDisplay operatorPrefab;
//    public TMP_Text operandText;
//    public MathOperatorDisplay equalPrefab;
//    public TMP_Text answerText;

//    [Header("Settings")]
//    [Tooltip("Avoid repeating the same equation twice in a row")]
//    public bool avoidRepeat = true;

//    private List<EquationData> allEquations = new List<EquationData>();
//    private EquationData currentData;
//    private int lastIndex = -1;

//    void Awake()
//    {
//        if (Instance == null) Instance = this;
//        else Destroy(gameObject);
//    }

//    void Start()
//    {
//        // Load all equations from all JSON files into one combined list
//        foreach (TextAsset jsonFile in equationJSONFiles)
//        {
//            if (jsonFile == null) continue;

//            EquationsListWrapper wrapper = JsonUtility.FromJson<EquationsListWrapper>(jsonFile.text);

//            if (wrapper != null && wrapper.equations != null)
//                allEquations.AddRange(wrapper.equations);
//        }

//        Debug.Log($"Total equations loaded: {allEquations.Count}");
//        LoadRandomEquation();
//    }

//    // ─────────────────────────────────────────
//    //  Public: called by NumEq on correct answer
//    // ─────────────────────────────────────────
//    public void OnCorrectAnswer()
//    {
//        StartCoroutine(NextEquationDelay(1.0f));
//    }

//    IEnumerator NextEquationDelay(float delay)
//    {
//        yield return new WaitForSeconds(delay);
//        LoadRandomEquation();
//    }

//    // ─────────────────────────────────────────
//    //  Pick a random equation (no immediate repeat)
//    // ─────────────────────────────────────────
//    void LoadRandomEquation()
//    {
//        int index;

//        if (allEquations.Count == 1)
//        {
//            index = 0;
//        }
//        else
//        {
//            do
//            {
//                index = UnityEngine.Random.Range(0, allEquations.Count);
//            }
//            while (avoidRepeat && index == lastIndex);
//        }

//        lastIndex = index;
//        currentData = allEquations[index];
//        AssignData();
//    }

//    // ─────────────────────────────────────────
//    //  Push data into UI elements
//    // ─────────────────────────────────────────
//    void AssignData()
//    {
//        int knownOperand = 0;
//        string operatorSymbol = currentData.operators[0];

//        for (int i = 0; i < currentData.operands.Length; i++)
//        {
//            if (currentData.operands[i] != currentData.blank_value)
//                knownOperand = currentData.operands[i];
//        }

//        blankPrefab.ResetState();

//        blankPrefab.secondNumber = knownOperand;
//        blankPrefab.result = currentData.answer;
//        blankPrefab.SetOperatorType(SymbolToEnum(operatorSymbol));
//        blankPrefab.SetBlank();

//        operatorPrefab.SetOperator(SymbolToEnum(operatorSymbol));
//        operandText.text = knownOperand.ToString();
//        equalPrefab.SetOperator(MathOperator.Equal);
//        answerText.text = currentData.answer.ToString();
//    }

//    MathOperator SymbolToEnum(string symbol)
//    {
//        switch (symbol)
//        {
//            case "+": return MathOperator.Add;
//            case "-": return MathOperator.Subtract;
//            case "*": return MathOperator.Multiply;
//            case "/": return MathOperator.Divide;
//            default: return MathOperator.Add;
//        }
//    }
//}