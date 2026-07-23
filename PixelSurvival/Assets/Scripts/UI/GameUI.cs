using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏 HUD - 显示血量/饥饿/口渴/辐射/线索进度 + 操作按钮
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("状态条")]
    public Slider hpSlider;
    public Slider hungerSlider;
    public Slider thirstSlider;
    public Slider radiationSlider;
    public Text hpText;
    public Text hungerText;
    public Text thirstText;
    public Text radiationText;

    [Header("资源显示")]
    public Text woodText;
    public Text stoneText;
    public Text foodText;
    public Text waterText;
    public Text depthText;

    [Header("面板")]
    public GameObject inventoryPanel;
    public GameObject craftingPanel;
    public GameObject journalPanel;
    public GameObject pausePanel;
    public GameObject deathPanel;

    [Header("按钮")]
    public Button gatherBtn;
    public Button eatBtn;
    public Button drinkBtn;
    public Button inventoryBtn;
    public Button craftBtn;
    public Button journalBtn;
    public Button digBtn;
    public Button pauseBtn;

    private PlayerStats stats;
    private Inventory inventory;
    private RadiationSystem radiation;
    private WorldManager world;
    private PlayerController player;
    private ResourceGathering gathering;
    private MiningSystem mining;

    void Start()
    {
        stats = FindObjectOfType<PlayerStats>();
        inventory = FindObjectOfType<Inventory>();
        radiation = FindObjectOfType<RadiationSystem>();
        world = WorldManager.Instance;
        player = FindObjectOfType<PlayerController>();
        gathering = FindObjectOfType<ResourceGathering>();
        mining = FindObjectOfType<MiningSystem>();

        // 按钮绑定
        gatherBtn.onClick.AddListener(OnGather);
        eatBtn.onClick.AddListener(OnEat);
        drinkBtn.onClick.AddListener(OnDrink);
        inventoryBtn.onClick.AddListener(() => inventoryPanel.SetActive(!inventoryPanel.activeSelf));
        craftBtn.onClick.AddListener(() => craftingPanel.SetActive(!craftingPanel.activeSelf));
        journalBtn.onClick.AddListener(() => journalPanel.SetActive(!journalPanel.activeSelf));
        digBtn.onClick.AddListener(() => mining?.TryDig());
        pauseBtn.onClick.AddListener(() => pausePanel.SetActive(!pausePanel.activeSelf));

        CloseAllPanels();
    }

    void Update()
    {
        if (stats == null) return;

        hpSlider.value = stats.currentHP / stats.maxHP;
        hungerSlider.value = stats.currentHunger / stats.maxHunger;
        thirstSlider.value = stats.currentThirst / stats.maxThirst;

        hpText.text = $"HP {stats.currentHP:F0}/{stats.maxHP}";
        hungerText.text = $"饥饿 {stats.currentHunger:F0}/{stats.maxHunger}";
        thirstText.text = $"口渴 {stats.currentThirst:F0}/{stats.maxThirst}";

        // 辐射
        if (radiation != null)
        {
            radiationSlider.value = radiation.currentRadiation / radiation.maxRadiation;
            radiationText.text = $"辐射 {radiation.GetRadiationLevel()}";
            radiationText.color = radiation.GetRadiationColor();
        }

        // 资源
        if (inventory != null)
        {
            woodText.text = $"木:{inventory.GetItemCount("wood")}";
            stoneText.text = $"石:{inventory.GetItemCount("stone")}";
            foodText.text = $"食:{inventory.GetItemCount("food")}";
            waterText.text = $"水:{inventory.GetItemCount("water")}";
        }

        // 深度
        if (world != null && player != null)
        {
            int layer = world.GetDepthLayer(player.transform.position.y);
            string layerName = layer == 0 ? "地表" : $"地下 {layer}层";
            depthText.text = $"{layerName}";
            depthText.color = world.isPlayerInWater ? new Color(1f, 0.3f, 0.3f) : Color.white;
        }

        // 死亡面板
        if (stats.isDead && deathPanel != null)
            deathPanel.SetActive(true);
    }

    void OnGather()
    {
        if (gathering != null)
        {
            if (player != null && player.isInWater)
                gathering.GatherResource(ResourceType.Water);
            else
                gathering.GatherResource(ResourceType.Wood);
        }
    }

    void OnEat()
    {
        if (inventory != null && inventory.RemoveItem("food", 1))
            stats.Eat(25f);
        else if (inventory != null && inventory.RemoveItem("cooked_meat", 1))
            stats.Eat(40f);
    }

    void OnDrink()
    {
        if (inventory != null && inventory.RemoveItem("clean_water", 1))
            stats.Drink(30f);
        else if (inventory != null && inventory.RemoveItem("water", 1))
        {
            stats.Drink(15f);
            // 喝生水有辐射风险
            radiation?.TakeRawWaterDamage();
        }
    }

    void CloseAllPanels()
    {
        if (inventoryPanel) inventoryPanel.SetActive(false);
        if (craftingPanel) craftingPanel.SetActive(false);
        if (journalPanel) journalPanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);
        if (deathPanel) deathPanel.SetActive(false);
    }
}