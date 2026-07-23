using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    public Image iconImage;
    public Text countText;

    private int slotIndex;
    private Inventory inventory;
    private System.Action<int> onSelected;

    public void Setup(Inventory inv, int index, System.Action<int> callback)
    {
        inventory = inv; slotIndex = index; onSelected = callback;
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (inventory == null) return;
        string itemName = inventory.items[slotIndex];
        if (string.IsNullOrEmpty(itemName))
        {
            iconImage.enabled = false; countText.text = "";
        }
        else
        {
            iconImage.enabled = true;
            iconImage.color = GetColor(itemName);
            countText.text = inventory.counts[slotIndex].ToString();
        }
    }

    Color GetColor(string n)
    {
        switch (n)
        {
            case "wood": return new Color(0.55f, 0.35f, 0.15f);
            case "stone": return new Color(0.5f, 0.5f, 0.5f);
            case "food": return new Color(0.9f, 0.3f, 0.3f);
            case "water": return new Color(0.3f, 0.5f, 0.9f);
            case "clean_water": return new Color(0.2f, 0.6f, 1f);
            case "cooked_meat": return new Color(0.7f, 0.2f, 0.1f);
            case "shovel": return new Color(0.6f, 0.5f, 0.3f);
            case "pickaxe": return new Color(0.5f, 0.5f, 0.5f);
            case "iron_pickaxe": return new Color(0.8f, 0.8f, 0.9f);
            case "ladder": return new Color(0.7f, 0.5f, 0.2f);
            case "iron_ore": return new Color(0.7f, 0.5f, 0.3f);
            case "coal_ore": return new Color(0.2f, 0.2f, 0.2f);
            case "dirt": return new Color(0.5f, 0.3f, 0.15f);
            case "plank": return new Color(0.8f, 0.6f, 0.3f);
            case "stone_wall": return new Color(0.4f, 0.4f, 0.4f);
            case "campfire": return new Color(1f, 0.5f, 0.2f);
            case "raft": return new Color(0.6f, 0.4f, 0.2f);
            case "rad_suit": return new Color(0.2f, 0.2f, 0.5f);
            case "mineshaft_entrance": return new Color(0.3f, 0.2f, 0.1f);
            default: return Color.white;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onSelected?.Invoke(slotIndex);
    }
}