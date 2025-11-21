using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static BlockTypeScript;
using static ItemTypeScript;

public class InventoryUI : MonoBehaviour
{
    #region // 각 큐브 별 스프라이트
    public Sprite dirtSprite;
    public Sprite grassSprite;
    public Sprite stoneSprite;
    public Sprite coalSprite;
    public Sprite ironSprite;
    public Sprite goldSprite;
    public Sprite diamondSprite;
    #endregion

    public List<Transform> slot = new List<Transform>();
    public GameObject SlotItem;
    List<GameObject> items = new List<GameObject>();
    public int selectedIndex = -1;

    private void Update()
    {
        for (int i = 0; i < Mathf.Min(9, slot.Count); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SetSelectedIndex(i);
            }
        }
    }

    public void SetSelectedIndex(int idx)
    {
        ResetSelection();
        if (selectedIndex == idx)
        {
            selectedIndex = -1;
        }
        else
        {
            if (idx >= items.Count)
            {
                selectedIndex = -1;
            }
            else
            {
                Setselection(idx);
                selectedIndex = idx;
            }
        }
    }

    public void ResetSelection()
    {
        foreach (var slotTransform in slot)
        {
            if (slotTransform.GetComponent<Image>() != null)
            {
                slotTransform.GetComponent<Image>().color = Color.white;
            }
        }
    }

    void Setselection(int _idx)
    {
        if (slot[_idx].GetComponent<Image>() != null)
        {
            slot[_idx].GetComponent<Image>().color = Color.yellow;
        }
    }

    // PlayerHarvester가 설치할 블록 타입을 가져갈 때 사용
    public BlockType GetInventorySlot()
    {
        if (selectedIndex < 0 || selectedIndex >= items.Count)
            return BlockType.Air;

        return items[selectedIndex].GetComponent<SlotItemPrefab>().blockType;
    }

    public void UpdateInventory(Inventory myInven)
    {
        // 1. 기존 슬롯 초기화
        foreach (var slotItems in items)
        {
            Destroy(slotItems);
        }
        items.Clear();

        // 2. 새 인벤토리 데이터를 화면에 적용
        int idx = 0;
        foreach (var item in myInven.items) // item.Key는 ItemType
        {
            if (idx >= slot.Count) break;

            var go = Instantiate(SlotItem, slot[idx].transform);
            go.transform.localPosition = Vector3.zero;

            SlotItemPrefab slotItemPrefab = go.GetComponent<SlotItemPrefab>();
            items.Add(go);

            if (slotItemPrefab != null)
            {
                // ItemType을 BlockType으로 변환하여 SlotItemPrefab에 전달
                BlockType typeToPass = (BlockType)item.Key;
                Sprite itemSprite = GetSpriteForItemType(item.Key);

                switch (item.Key)
                {
                    case ItemType.Dirt:
                    case ItemType.Grass:
                    case ItemType.Stone:
                    case ItemType.Coal:
                    case ItemType.Iron:
                    case ItemType.Gold:
                    case ItemType.Diamond:
                        slotItemPrefab.ItemSetting(itemSprite, "x" + item.Value.ToString(), typeToPass);
                        break;
                }
            }
            idx++;
        }
    }

    private Sprite GetSpriteForItemType(ItemType type)
    {
        return type switch
        {
            ItemType.Dirt => dirtSprite,
            ItemType.Grass => grassSprite,
            ItemType.Stone => stoneSprite,
            ItemType.Coal => coalSprite,
            ItemType.Iron => ironSprite,
            ItemType.Gold => goldSprite,
            ItemType.Diamond => diamondSprite,
            _ => null
        };
    }
}