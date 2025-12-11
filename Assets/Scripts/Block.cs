using UnityEngine;
using System.Collections.Generic;

public class Block : MonoBehaviour
{
    [Header("Block Stat")]
    public GameData.BlockType type = GameData.BlockType.Dirt;
    [HideInInspector] public float currentHP; 
    public bool mineable = true;

    [Header("Drop Settings")]
    public GameObject itemDropPrefab;

    // 이펙트 및 사운드 (이전 단계에서 추가했다면 유지)
    [Header("Effects")]
    public GameObject breakEffectPrefab; 
    public AudioClip breakSound;         
    [Range(0f, 1f)] public float soundVolume = 1f;

    private readonly Dictionary<GameData.BlockType, float> BlockMaxHP = new()
    {
        { GameData.BlockType.Grass, 3f },
        { GameData.BlockType.Dirt, 3f },
        { GameData.BlockType.Stone, 6f },
        { GameData.BlockType.CoalOre, 9f },  // 석탄 HP 추가
        { GameData.BlockType.IronOre, 12f },
        { GameData.BlockType.GoldOre, 20f },
        { GameData.BlockType.DiamondOre, 35f },
        { GameData.BlockType.Obsidian, 50f }
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

        if (currentHP <= 0)
        {
            DestroyBlock();
        }
    }

    private void DestroyBlock()
    {
        DropItemsToWorld(type);

        // 이펙트 재생 (없으면 무시됨)
        if (breakEffectPrefab != null) Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
        if (breakSound != null) AudioSource.PlayClipAtPoint(breakSound, transform.position, soundVolume);

        Destroy(gameObject);
    }
    
    private void DropItemsToWorld(GameData.BlockType blockType)
    {
        if (itemDropPrefab == null) return;

        GameData.ItemType dropType;
        int dropCount = 1; // 기본 1개 드롭

        switch (blockType)
        {
            case GameData.BlockType.Dirt: dropType = GameData.ItemType.Dirt; break;
            case GameData.BlockType.Grass: dropType = GameData.ItemType.Grass; break;
            case GameData.BlockType.Stone: dropType = GameData.ItemType.Stone; break;
            case GameData.BlockType.CoalOre: dropType = GameData.ItemType.Coal; break; 
            case GameData.BlockType.IronOre: dropType = GameData.ItemType.Iron; break;
            case GameData.BlockType.GoldOre: dropType = GameData.ItemType.Gold; break;
            case GameData.BlockType.DiamondOre:  dropType = GameData.ItemType.Diamond; break;
            case GameData.BlockType.Obsidian: dropType = GameData.ItemType.Obsidian; break;

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
                // 캘 때 나온 아이템은 쿨타임을 짧게 (바로 먹어지게)
                dropComponent.pickupDelay = 0.5f; 
            }
        }
    }
}