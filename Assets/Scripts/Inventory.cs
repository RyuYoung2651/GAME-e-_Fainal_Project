using System.Collections.Generic;
using UnityEngine;

// 슬롯 정보를 담는 클래스
[System.Serializable]
public class InventorySlot
{
    public GameData.ItemType itemType;
    public int count;

    public InventorySlot()
    {
        itemType = GameData.ItemType.None;
        count = 0;
    }

    public bool IsEmpty => itemType == GameData.ItemType.None || count == 0;

    public void AddItem(GameData.ItemType type, int amount)
    {
        itemType = type;
        count += amount;
    }

    public void Clear()
    {
        itemType = GameData.ItemType.None;
        count = 0;
    }
}

public class Inventory : MonoBehaviour
{
    public int currentGold = 0;
    
    // 슬롯 리스트 (9칸)
    public List<InventorySlot> slots = new List<InventorySlot>(); 

    InventoryUI invenUI;

    public int bagLevel = 0; 
    public int maxSlots = 9;        
    public int maxStackCount = 10;  // 기본 10개

    void Awake()
    {
        // 슬롯 초기화 (9개 생성)
        for (int i = 0; i < maxSlots; i++)
        {
            slots.Add(new InventorySlot());
        }
    }

    void Start()
    {
        invenUI = FindObjectOfType<InventoryUI>();
        if (invenUI != null) invenUI.UpdateInventory(this);
    }

    // [핵심 수정] 아이템 추가 함수 (공간 확인 후 추가)
    public bool Add(GameData.ItemType type, int amount = 1)
    {
        // 1. 먼저 담을 공간이 충분한지 계산 (시뮬레이션)
        int availableSpace = 0;

        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                availableSpace += maxStackCount;
            }
            else if (slot.itemType == type)
            {
                availableSpace += (maxStackCount - slot.count);
            }
        }

        // 2. 공간이 부족하면 즉시 실패 처리 (아이템을 먹지 않음)
        if (availableSpace < amount)
        {
            if (invenUI != null) invenUI.ShowWarningMessage("가방 공간이 부족합니다!");
            return false; // 실패 반환 -> ItemDrop이 파괴되지 않음
        }

        // 3. 공간이 충분하면 실제로 아이템 추가
        int remaining = amount;

        // 3-1. 기존 슬롯에 채우기
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty && slot.itemType == type)
            {
                int space = maxStackCount - slot.count;
                if (space > 0)
                {
                    int toAdd = Mathf.Min(remaining, space);
                    slot.count += toAdd;
                    remaining -= toAdd;
                    if (remaining <= 0) break;
                }
            }
        }

        // 3-2. 빈 슬롯에 채우기
        if (remaining > 0)
        {
            foreach (var slot in slots)
            {
                if (slot.IsEmpty)
                {
                    int toAdd = Mathf.Min(remaining, maxStackCount);
                    slot.AddItem(type, toAdd);
                    remaining -= toAdd;
                    if (remaining <= 0) break;
                }
            }
        }

        // UI 업데이트
        if (invenUI != null) invenUI.UpdateInventory(this);
        
        return true; // 성공 반환 -> ItemDrop 파괴됨
    }

    // 가방 업그레이드
    public void UpgradeBag(int level)
    {
        bagLevel = level;
        switch (level)
        {
            case 1: maxStackCount = 30; break;
            case 2: maxStackCount = 64; break;
            case 3: maxStackCount = 100; break;
            case 4: maxStackCount = 999; break;
        }
        Debug.Log($"가방 업그레이드! (한 칸당 {maxStackCount}개)");
        if (invenUI != null) invenUI.UpdateInventory(this);
    }

    // 아이템 소모
    public bool Consume(GameData.ItemType type, int amount = 1)
    {
        if (GetItemCount(type) < amount) return false;

        int remainingToRemove = amount;

        // 뒤쪽 슬롯부터 소모
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            var slot = slots[i];
            if (!slot.IsEmpty && slot.itemType == type)
            {
                int take = Mathf.Min(remainingToRemove, slot.count);
                slot.count -= take;
                remainingToRemove -= take;

                if (slot.count <= 0) slot.Clear();

                if (remainingToRemove <= 0) break;
            }
        }

        if (invenUI != null) invenUI.UpdateInventory(this);
        return true;
    }

    // 총 아이템 개수 확인
    public int GetItemCount(GameData.ItemType type)
    {
        int total = 0;
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty && slot.itemType == type)
                total += slot.count;
        }
        return total;
    }
}