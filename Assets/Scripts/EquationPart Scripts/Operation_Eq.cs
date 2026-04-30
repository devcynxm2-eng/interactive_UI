using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum MathOperator
{
    Add,
    Subtract,
    Multiply,
    Divide,
    Equal
}

public class MathOperatorDisplay : MonoBehaviour
{
    [Header("Operator Settings")]
    public MathOperator currentOperator;

    [Header("UI Reference")]
    public TMP_Text operatorText; 

    private void Start()
    {
        UpdateOperatorText();
    }

  
    public void UpdateOperatorText()
    {
        if (operatorText != null)
        {
            operatorText.text = GetSymbol(currentOperator);
        }
    }

 
    public static string GetSymbol(MathOperator op)
    {
        switch (op)
        {
            case MathOperator.Add: return "+";
            case MathOperator.Subtract: return "-";
            case MathOperator.Multiply: return "×";
            case MathOperator.Divide: return "÷";
            case MathOperator.Equal: return "=";
            default: return "?";
        }
    }

 
    public void SetOperator(MathOperator op)
    {
        currentOperator = op;
        UpdateOperatorText();
    }
}