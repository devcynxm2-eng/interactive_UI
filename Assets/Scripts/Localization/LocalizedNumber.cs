using UnityEngine.Localization.Settings;

public static class LocalizedNumber
{
    private static readonly char[] arabicDigits =
        { '٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩' };

    public static string FormatPlain(int number)
    {
        string code = LocalizationSettings.SelectedLocale?.Identifier.Code ?? "en";

        string digits = number.ToString();

        if (code != "ar")
            return digits;

        // Manually convert each digit to Arabic
        char[] result = new char[digits.Length];
        for (int i = 0; i < digits.Length; i++)
        {
            if (digits[i] >= '0' && digits[i] <= '9')
                result[i] = arabicDigits[digits[i] - '0'];
            else
                result[i] = digits[i]; // keep minus sign as is
        }
        return new string(result);
    }

    public static string Format(int number)
    {
        // Same but with thousand separators
        string code = LocalizationSettings.SelectedLocale?.Identifier.Code ?? "en";

        if (code != "ar")
            return number.ToString("N0");

        string digits = number.ToString("N0"); // get formatted with commas first
        char[] result = new char[digits.Length];
        for (int i = 0; i < digits.Length; i++)
        {
            if (digits[i] >= '0' && digits[i] <= '9')
                result[i] = arabicDigits[digits[i] - '0'];
            else
                result[i] = digits[i];
        }
        return new string(result);
    }
}