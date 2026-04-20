using System;
using System.Collections.Generic;

[Serializable]
public class WordData
{
    public string word;       // e.g. "CAT"
    public string image;      // e.g. "cat" — must match filename in Resources/WordImages/
}

[Serializable]
public class WordsListWrapper
{
    public WordData[] words;
}