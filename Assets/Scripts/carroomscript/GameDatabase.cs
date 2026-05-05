using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameDatabase", menuName = "Game/Database")]
public class GameDatabase : ScriptableObject
{
    public List<Category> categories;

    [System.Serializable]
    public class Category
    {
        public string categoryName;
        public List<Item> items;
    }

    [System.Serializable]
    public class Item
    {
        public string itemName;
        public Sprite image;
        public Sprite icon;
        public bool isLocked;
        public int unlockCost;      
        public int progressValue;   
    }
}