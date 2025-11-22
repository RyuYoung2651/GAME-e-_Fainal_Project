using UnityEngine;

[System.Serializable]
public struct CraftingRecipe
{
    public string recipeName; // 레시피 이름 (예: 돌 곡괭이)
    
    // 결과물
    public GameData.ItemType resultItem; 
    
    // 재료 1
    public GameData.ItemType ingredient1; 
    public int count1;

    // 재료 2
    public GameData.ItemType ingredient2; 
    public int count2;
}