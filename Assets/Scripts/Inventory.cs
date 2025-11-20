using System.Collections.Generic;
using UnityEngine;
using static ItemTypeScript;

public class Inventory : MonoBehaviour
{
    public Dictionary<ItemType, int> items = new();

    public void Add(ItemType type, int count = 1)
    {
        if (!items.ContainsKey(type)) items[type] = 0;
        items[type] += count;
        Debug.Log($"[Inventory] +{count} {type} (รั {items[type]})");
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