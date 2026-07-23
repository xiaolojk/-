using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public GameObject itemSlotPrefab;
    public Transform slotsParent;
    public Text selectedItemInfo;
    public Button useButton;
    public Button dropButton;

    private Inventory inventory;
    private List<GameObject> slotObjects = new List<GameObject>();
    private int selectedSlot = -1;

    void Start()
    {
        inventory = FindObjectOfType<Inventory>();
        useButton.onClick.AddListener(UseSelected);
        dropButton.onClick.AddListener(DropSelected);
        gameObject.SetActive(false);
    }

    void OnEnable() { RefreshSlots(); }

    void RefreshSlots()
    {
        foreach (var obj in slotObjects) Destroy(obj);
        slotObjects.Clear();
        for (int i = 0; i < inventory.items.Length; i++)
        {
            var slot = Instantiate(itemSlotPrefab, slotsParent);
            slotObjects.Add(slot);
            var slotUI = slot.GetComponent<InventorySlotUI>();
            if (slotUI != null) slotUI.Setup(inventory, i, OnSlotSelected);
        }
    }

    void OnSlotSelected(int index)
    {
        selectedSlot = index;
        if (selectedSlot >= 0 && !string.IsNullOrEmpty(inventory.items[index]))
            selectedItemInfo.text = $"{inventory.items[index]} x{inventory.counts[index]}";
        else
            selectedItemInfo.text = "";
    }

    void UseSelected()
    {
        if (selectedSlot < 0) return;
        string itemName = inventory.items[selectedSlot];
        if (string.IsNullOrEmpty(itemName)) return;
        var stats = FindObjectOfType<PlayerStats>();
        if (itemName == "food" || itemName == "cooked_meat")
        {
            inventory.RemoveItem(itemName, 1);
            stats?.Eat(itemName == "cooked_meat" ? 40f : 20f);
        }
        else if (itemName == "clean_water" || itemName == "water")
        {
            inventory.RemoveItem(itemName, 1);
            stats?.Drink(itemName == "clean_water" ? 30f : 15f);
        }
        RefreshSlots();
    }

    void DropSelected()
    {
        if (selectedSlot < 0) return;
        inventory.RemoveItem(inventory.items[selectedSlot], 1);
        RefreshSlots();
    }
}