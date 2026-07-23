using UnityEngine;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// 游戏 HUD - 显示生命值、饥饿度、口渴度、资源数量
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("状态条")]
    public Slider hpSlider;
    public Slider hungerSlider;
    public Slider thirstSlider;
    public Text hpText;
    public Text hungerText;
    public Text thirstText;

    [Header("资源显示")]
    public Text woodText;
    public Text stoneText;
    public Text foodText;
    public Text waterText;

    [Header("面板")]
    public GameObject inventoryPanel;
    public GameObject craftingPanel;
    public GameObject pausePanel;

    [Header("采集按钮")]
    public Button gatherWoodButton;
    public Button gatherStoneButton;
    public Button gatherFoodButton;
    public Button gatherWaterButton;

    [Header("操作按钮")]
    public Button eatButton;
    public Button drinkButton;
    public Button inventoryButton;
    public Button craftButton;
    public Button pauseButton;

    private PlayerStats stats;
    private Inventory inventory;
    private ResourceGathering gathering;

    void Start()
    {
        stats = FindObjectOfType<PlayerStats>();
        inventory = FindObjectOfType<Inventory>();
        gathering = FindObjectOfType<ResourceGathering>();

        if (stats == null)
        {
            Debug.LogError("GameUI: 找不到 PlayerStats!");
        }

        // 绑定按钮事件
        gatherWoodButton.onClick.AddListener(() => gathering?.GatherResource(ResourceType.Wood));
        gatherStoneButton.onClick.AddListener(() => gathering?.GatherResource(ResourceType.Stone));
        gatherFoodButton.onClick.AddListener(() => gathering?.GatherResource(ResourceType.Food));
        gatherWaterButton.onClick.AddListener(() => gathering?.GatherResource(ResourceType.Water));

        eatButton.onClick.AddListener(EatFood);
        drinkButton.onClick.AddListener(DrinkWater);
        inventoryButton.onClick.AddListener(ToggleInventory);
        craftButton.onClick.AddListener(ToggleCrafting);
        pauseButton.onClick.AddListener(TogglePause);

        inventoryPanel.SetActive(false);
        craftingPanel.SetActive(false);
        pausePanel.SetActive(false);
    }

    void Update()
    {
        if (stats == null) return;

        // 更新状态条
        hpSlider.value = stats.currentHP / stats.maxHP;
        hungerSlider.value = stats.currentHunger / stats.maxHunger;
        thirstSlider.value = stats.currentThirst / stats.maxThirst;

        hpText.text = $"HP: {stats.currentHP:F0}/{stats.maxHP}";
        hungerText.text = $"饥饿: {stats.currentHunger:F0}/{stats.maxHunger}";
        thirstText.text = $"口渴: {stats.currentThirst:F0}/{stats.maxThirst}";

        // 更新资源
        if (inventory != null)
        {
            woodText.text = $"木材: {inventory.GetItemCount("wood")}";
            stoneText.text = $"石料: {inventory.GetItemCount("stone")}";
            foodText.text = $"食物: {inventory.GetItemCount("food")}";
            waterText.text = $"水: {inventory.GetItemCount("water")}";
        }
    }

    void EatFood()
    {
        if (inventory != null && inventory.RemoveItem("food", 1))
        {
            stats.Eat(20f);
        }
    }

    void DrinkWater()
    {
        if (inventory != null && inventory.RemoveItem("water", 1))
        {
            stats.Drink(20f);
        }
    }

    void ToggleInventory() => inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    void ToggleCrafting() => craftingPanel.SetActive(!craftingPanel.activeSelf);
    void TogglePause() => pausePanel.SetActive(!pausePanel.activeSelf);
}