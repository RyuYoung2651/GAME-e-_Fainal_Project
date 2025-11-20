using System.Collections.Generic;
using UnityEngine;
using static ItemTypeScript;

public class Inventory : MonoBehaviour
{
    public Dictionary<ItemType, int> items = new();
    InventoryUI invenUI;

   void Start()
    {
        invenUI = FindObjectOfType<InventoryUI>();
    }

    public void SetSelectedIndex(int idx)
    {

    }


    public void ResetSelection()
    {
        foreach(var slot in Slot)
        {
            slot.
        }
    }


    public void Add(ItemType type, int count = 1)
    {
        if (!items.ContainsKey(type)) items[type] = 0;
        items[type] += count;
        Debug.Log($"[Inventory] +{count} {type} (รั {items[type]})");
        invenUI.UpdateInventory(this);

        if(invenUI != null) invenUI.UpdateInventory(this);
    }

    public bool Consume(ItemType type, int count = 1)
    {
        if (!items.TryGetValue(type, out var have) || have < count) return false;
        items[type] = have - count;
        Debug.Log($"[Inventory] -{count} {type} (รั {items[type]})");
        return true;
    }

    public int GetItemCount(ItemType type)
    {
        return items.TryGetValue(type, out var count) ? count : 0;
    }
}