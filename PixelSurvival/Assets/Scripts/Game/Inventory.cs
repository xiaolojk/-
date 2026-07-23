using UnityEngine;

/// <summary>
/// 背包系统 - 支持新物品类型
/// </summary>
public class Inventory : MonoBehaviour
{
    public const int MAX_SLOTS = 28;
    public string[] items = new string[MAX_SLOTS];
    public int[] counts = new int[MAX_SLOTS];

    void Start()
    {
        var gm = GameManager.Instance;
        if (gm != null && gm.currentUserData != null)
        {
            var data = gm.currentUserData;
            for (int i = 0; i < Mathf.Min(data.inventory.Length, MAX_SLOTS); i++)
            {
                items[i] = data.inventory[i];
                counts[i] = data.inventoryCounts[i];
            }
        }
    }

    public bool AddItem(string itemName, int amount = 1)
    {
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            if (items[i] == itemName)
            {
                counts[i] += amount;
                return true;
            }
        }
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            if (string.IsNullOrEmpty(items[i]))
            {
                items[i] = itemName;
                counts[i] = amount;
                return true;
            }
        }
        Debug.Log("背包已满！");
        return false;
    }

    public bool RemoveItem(string itemName, int amount = 1)
    {
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            if (items[i] == itemName)
            {
                if (counts[i] >= amount)
                {
                    counts[i] -= amount;
                    if (counts[i] <= 0)
                    {
                        items[i] = "";
                        counts[i] = 0;
                    }
                    return true;
                }
                else
                {
                    Debug.Log($"物品 {itemName} 数量不足！");
                    return false;
                }
            }
        }
        return false;
    }

    public int GetItemCount(string itemName)
    {
        int total = 0;
        for (int i = 0; i < MAX_SLOTS; i++)
            if (items[i] == itemName) total += counts[i];
        return total;
    }

    public bool HasItem(string itemName, int amount = 1)
    {
        return GetItemCount(itemName) >= amount;
    }

    public void DropRandomItems(int count)
    {
        int dropped = 0;
        for (int i = 0; i < MAX_SLOTS && dropped < count; i++)
        {
            if (!string.IsNullOrEmpty(items[i]))
            {
                int drop = Mathf.Min(counts[i], Random.Range(1, 3));
                counts[i] -= drop;
                dropped++;
                if (counts[i] <= 0)
                {
                    items[i] = "";
                    counts[i] = 0;
                }
            }
        }
    }
}