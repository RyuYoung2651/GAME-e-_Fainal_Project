using UnityEngine;
using System.Collections.Generic;
using static BlockTypeScript;
using static ItemTypeScript;

public class Block : MonoBehaviour
{
    [Header("Block Stat")]
    public BlockType type = BlockType.Dirt;
    [HideInInspector] public float currentHP;
    public bool mineable = true;

    // 추가: 드롭할 아이템 프리팹
    [Header("Drop Settings")]
    public GameObject itemDropPrefab; // ItemDrop.cs가 붙은 프리팹을 연결해야 함

    private readonly Dictionary<BlockType, float> BlockMaxHP = new()
    {
        { BlockType.Grass, 3f },
        { BlockType.Dirt, 3f },
        { BlockType.Stone, 6f },
        { BlockType.IronOre, 12f },
        { BlockType.GoldOre, 20f },
        { BlockType.DiamondOre, 35f }
    };

    void Awake()
    {
        if (BlockMaxHP.ContainsKey(type))
            currentHP = BlockMaxHP[type];
        else
            currentHP = 1f;
    }

    public void Hit(float damage)
    {
        if (!mineable) return;

        currentHP -= damage;
        Debug.Log($"Hit! Type: {type}, Damage: {damage}, Remaining HP: {currentHP}");

        if (currentHP <= 0)
        {
            DestroyBlock();
        }
    }


    private void DestroyBlock()
    {
        //DropItemsToWorld 호출
        DropItemsToWorld(type);

        // 유니티의 Destroy 함수 호출
        Destroy(gameObject);
    }

    //DropItemsToWorld 메서드 정의 (없었다면 추가하세요)
    private void DropItemsToWorld(BlockType blockType)
    {
        if (itemDropPrefab == null)
        {
            Debug.LogError("Item Drop Prefab이 Block에 연결되어 있지 않습니다!");
            return;
        }

        ItemType dropType;
        int dropCount = 1;

        switch (blockType)
        {
            case BlockType.Dirt: dropType = ItemType.Dirt; break;
            case BlockType.Grass: dropType = ItemType.Grass; break;
            case BlockType.Stone: dropType = ItemType.Stone; break;
            case BlockType.IronOre: dropType = ItemType.Iron; break;
            case BlockType.GoldOre: dropType = ItemType.Gold; break;
            case BlockType.DiamondOre: dropType = ItemType.Diamond; break;
            default: return;
        }

        if (dropCount > 0)
        {
            var dropGO = Instantiate(itemDropPrefab, transform.position, Quaternion.identity);

            var dropComponent = dropGO.GetComponent<ItemDrop>();
            if (dropComponent != null)
            {
                dropComponent.type = dropType;
                dropComponent.count = dropCount;
            }
        }
    }
}