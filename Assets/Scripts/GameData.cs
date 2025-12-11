using UnityEngine;

public class GameData : MonoBehaviour
{
    // BlockType Enum 정의
    public enum BlockType
    {
        Air,         // 0
        Dirt,        // 1
        Grass,       // 2
        Stone,       // 3
        IronOre,     // 4
        GoldOre,     // 5
        DiamondOre,  // 6
        Water,       // 7
        CoalOre,      // 8
        Obsidian,
        Portal
    }

    // ItemType Enum 정의 (순서 중요!)
    public enum ItemType
    {
        None,        // 0
        Dirt,        // 1
        Grass,       // 2
        Stone,       // 3
        Iron,        // 4
        Gold,        // 5
        Diamond,     // 6
        Water,       // 7
        Coal,        // 8
        Obsidian,

        // --- 도구 ---
        StonePickaxe,// 9
        IronPickaxe, // 10
        GoldPickaxe, // 11
        DiamondPickaxe,

        // --- 가방 ---
        Bag_Small,   // 12
        Bag_Medium,  // 13
        Bag_Large,   // 14
        Bag_Max,      // 15

        //--- 상점 ---
        Wood,       //16
        Dynamite,       // 폭발
        ReturnScroll,   // 귀환
        EvolutionItem,   // 진화
        Flint,      // 부싯돌
        IronIngot,  // 철괴
        Lighter     // 라이터
    }
    
    [System.Serializable]
    public struct CraftingRecipe
    {
        public string recipeName; //"돌 곡괭이"
        public GameData.ItemType resultItem; // 결과물
    
        // 필요한 재료와 개수
        public GameData.ItemType ingredient1;   //재료 1
        public int count1;                      //그 재료1의 개수
        public GameData.ItemType ingredient2;   //재료 2
        public int count2;                      //재료2의 개수
    }
}