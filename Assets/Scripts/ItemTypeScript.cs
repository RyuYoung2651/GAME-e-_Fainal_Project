using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemTypeScript : MonoBehaviour
{
    public enum ItemType
    {
        None,        // 맨손 상태
        Dirt,
        Grass,
        Coal,       //석탄
        Stone,
        Iron,        // 광물 아이템
        Gold,
        Diamond,
        Wood,        // 제작 재료
        StonePickaxe,
        IronPickaxe,
        GoldPickaxe
    }
}
