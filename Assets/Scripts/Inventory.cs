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

    public void Add(ItemType type, int count = 1)
    {
        if (!items.ContainsKey(type)) items[type] = 0;
        items[type] += count;
        Debug.Log($"[Inventory] +{count} {type} (รั {items[type]})");

        if (invenUI != null) invenUI.UpdateInventory(this);
    }

    public bool Consume(ItemType type, int count = 1)
    {
        if (!items.TryGetValue(type, out var have) || have < count) return false;
        items[type] = have - count;
        Debug.Log($"[Inventory] -{count} {type} (รั {items[type]})");

        if (items[type] == 0)
        {
            items.Remove(type);
            if (invenUI != null)
            {
                invenUI.selectedIndex = -1;
                invenUI.ResetSelection();
            }
        }

        if (invenUI != null) invenUI.UpdateInventory(this);
        return true;
    }

    public int GetItemCount(ItemType type)
    {
        return items.TryGetValue(type, out var count) ? count : 0;
    }
}