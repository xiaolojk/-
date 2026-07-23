using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 矿井/洞穴系统 - 地下探索
/// 可以找到天然洞穴，或用铲子+梯子制造矿井
/// </summary>
public class MiningSystem : MonoBehaviour
{
    [Header("挖掘设置")]
    public float digRange = 1.5f;
    public float digCooldown = 1f;
    public int dirtPerDig = 3;
    public int stonePerDig = 2;
    public int oreChance = 15;             // 挖到矿石的概率%

    [Header("地下层级")]
    public LayerInfo[] layers;

    private bool canDig = false;
    private float digTimer = 0f;
    private Inventory inventory;
    private PlayerController player;
    private WorldManager world;

    // 玩家已挖掘的矿井入口
    private List<Mineshaft> mineshafts = new List<Mineshaft>();

    void Start()
    {
        inventory = GetComponent<Inventory>();
        player = GetComponent<PlayerController>();
        world = WorldManager.Instance;

        layers = new LayerInfo[]
        {
            new LayerInfo { name = "地表", depth = 0, color = new Color(0.3f, 0.55f, 0.2f), blockType = "dirt", stoneChance = 5 },
            new LayerInfo { name = "浅层", depth = 10, color = new Color(0.45f, 0.35f, 0.2f), blockType = "dirt", stoneChance = 20 },
            new LayerInfo { name = "岩层", depth = 20, color = new Color(0.5f, 0.5f, 0.5f), blockType = "stone", stoneChance = 60 },
            new LayerInfo { name = "深层", depth = 30, color = new Color(0.35f, 0.35f, 0.4f), blockType = "stone_deep", stoneChance = 80 },
            new LayerInfo { name = "熔岩层", depth = 40, color = new Color(0.7f, 0.3f, 0.1f), blockType = "lava_rock", stoneChance = 90 },
        };
    }

    void Update()
    {
        if (!canDig)
        {
            digTimer -= Time.deltaTime;
            if (digTimer <= 0) canDig = true;
        }
    }

    /// <summary>
    /// 尝试挖掘当前位置
    /// </summary>
    public bool TryDig()
    {
        if (!canDig) return false;
        if (!inventory.HasItem("shovel") && !inventory.HasItem("pickaxe")) return false;

        canDig = false;
        digTimer = digCooldown;

        int layer = world.GetDepthLayer(player.transform.position.y);
        var info = layers[Mathf.Clamp(layer, 0, layers.Length - 1)];

        // 挖掘产出
        inventory.AddItem("dirt", dirtPerDig);

        if (Random.Range(0, 100) < info.stoneChance)
        {
            inventory.AddItem("stone", stonePerDig);
        }

        // 有概率挖到矿石
        if (Random.Range(0, 100) < oreChance)
        {
            string ore = layer >= 3 ? "iron_ore" : "coal_ore";
            inventory.AddItem(ore, 1);
        }

        // 挖掘消耗铲子耐久
        // (简化：铲子使用次数)

        Debug.Log($"挖掘 {info.name} -> +{dirtPerDig}泥土");
        return true;
    }

    /// <summary>
    /// 创建矿井入口（需要铲子+梯子）
    /// </summary>
    public bool CreateMineshaft(Vector2 position)
    {
        if (!inventory.HasItem("shovel") || !inventory.HasItem("ladder", 3))
        {
            Debug.Log("需要铲子和至少3个梯子才能建矿井！");
            return false;
        }

        inventory.RemoveItem("ladder", 3);
        // 铲子不消耗，但记录使用

        var shaft = new Mineshaft
        {
            position = position,
            depth = 0,
            maxDepth = 5,
            isActive = true
        };

        mineshafts.Add(shaft);
        Debug.Log($"矿井已创建！深度: {shaft.maxDepth}层");
        return true;
    }

    /// <summary>
    /// 检测附近是否有天然洞穴
    /// </summary>
    public bool IsNearCave(Vector2 pos)
    {
        // 简化：检查y坐标是否在地下，且随机概率
        if (pos.y < world.waterLevel - 5f)
        {
            return Random.Range(0, 100) < 30;
        }
        return false;
    }

    /// <summary>
    /// 获取最近的矿井
    /// </summary>
    public Mineshaft GetNearestMineshaft(Vector2 pos)
    {
        Mineshaft nearest = null;
        float minDist = float.MaxValue;
        foreach (var shaft in mineshafts)
        {
            float dist = Vector2.Distance(pos, shaft.position);
            if (dist < minDist && shaft.isActive)
            {
                minDist = dist;
                nearest = shaft;
            }
        }
        return nearest;
    }

    public List<Mineshaft> GetMineshafts() => mineshafts;
}

[System.Serializable]
public class LayerInfo
{
    public string name;
    public float depth;
    public Color color;
    public string blockType;
    public int stoneChance;
}

[System.Serializable]
public class Mineshaft
{
    public Vector2 position;
    public int depth;
    public int maxDepth;
    public bool isActive;
}