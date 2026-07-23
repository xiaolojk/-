using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 背包格子 UI 组件
/// </summary>
public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    public Image iconImage;
    public Text countText;
    public Image selectedBorder;

    private int slotIndex;
    private Inventory inventory;
    private System.Action<int> onSelected;

    public void Setup(Inventory inv, int index, System.Action<int> callback)
    {
        inventory = inv;
        slotIndex = index;
        onSelected = callback;

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (inventory == null) return;

        string itemName = inventory.items[slotIndex];
        if (string.IsNullOrEmpty(itemName))
        {
            iconImage.enabled = false;
            countText.text = "";
        }
        else
        {
            iconImage.enabled = true;
            iconImage.color = GetItemColor(itemName);
            countText.text = inventory.counts[slotIndex].ToString();
        }
    }

    Color GetItemColor(string itemName)
    {
        switch (itemName)
        {
            case "wood": return new Color(0.55f, 0.35f, 0.15f);    // 棕色
            case "stone": return new Color(0.5f, 0.5f, 0.5f);       // 灰色
            case "food": return new Color(0.9f, 0.3f, 0.3f);        // 红色
            case "water": return new Color(0.3f, 0.5f, 0.9f);       // 蓝色
            case "tool_axe": return new Color(0.7f, 0.7f, 0.3f);    // 金色
            case "tool_pickaxe": return new Color(0.6f, 0.6f, 0.2f);
            case "weapon_sword": return new Color(0.8f, 0.8f, 0.8f);
            case "campfire": return new Color(1f, 0.5f, 0.2f);      // 橙色
            case "plank": return new Color(0.8f, 0.6f, 0.3f);
            case "stone_wall": return new Color(0.4f, 0.4f, 0.4f);
            case "cooked_meat": return new Color(0.7f, 0.2f, 0.1f);
            default: return Color.white;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onSelected?.Invoke(slotIndex);
    }
}