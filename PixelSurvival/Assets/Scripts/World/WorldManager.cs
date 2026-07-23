using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 水世界管理器 - 孤岛 + 放射性海洋 + 地下层
/// 冰川融化后的世界：到处都是水，仅剩零星孤岛
/// </summary>
public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance { get; private set; }

    [Header("世界设置")]
    public int worldWidth = 200;         // 世界宽度（块）
    public int islandCount = 3;          // 可见岛屿数量
    public float waterLevel = -2f;       // 水面高度
    public float waterDamageRadius = 0.5f; // 水中判定距离

    [Header("地下层")]
    public int undergroundLayers = 4;    // 地下层数
    public float layerDepth = 10f;       // 每层深度

    [Header("时间")]
    public float dayLength = 480f;       // 一天长度（秒）= 8分钟
    public float currentTime = 0f;       // 当前时间 (0-1)
    public bool isDaytime = true;

    [Header("线索")]
    public List<StoryClue> worldClues = new List<StoryClue>();

    public bool isPlayerInWater = false;
    public float waterContactTimer = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // 昼夜循环
        currentTime += Time.deltaTime / dayLength;
        if (currentTime >= 1f) currentTime -= 1f;
        isDaytime = currentTime < 0.5f;

        // 检查玩家是否在水中
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            isPlayerInWater = player.transform.position.y < waterLevel;
        }
    }

    /// <summary>
    /// 获取当前深度层级（0=地面, 1~N=地下）
    /// </summary>
    public int GetDepthLayer(float yPos)
    {
        if (yPos >= waterLevel) return 0;
        return Mathf.Min((int)(Mathf.Abs(yPos - waterLevel) / layerDepth) + 1, undergroundLayers);
    }

    /// <summary>
    /// 检查位置是否安全（不在水中）
    /// </summary>
    public bool IsSafePosition(Vector2 pos)
    {
        return pos.y >= waterLevel + 0.5f;
    }

    /// <summary>
    /// 获取随机水面上岛屿位置
    /// </summary>
    public Vector2 GetRandomIslandPosition()
    {
        float x = Random.Range(-worldWidth / 2f, worldWidth / 2f);
        float y = waterLevel + Random.Range(2f, 5f);
        return new Vector2(x, y);
    }
}

/// <summary>
/// 故事线索
/// </summary>
[System.Serializable]
public class StoryClue
{
    public string clueId;
    public string title;
    public string content;
    public Vector2 worldPosition;
    public bool isFound = false;
    public ClueType type;

    public enum ClueType
    {
        Note,           // 笔记
        RadioSignal,    // 无线电信号
        Wreckage,       // 残骸
        Recording,      // 录音
        Graffiti,       // 涂鸦
        Corpse,         // 遗骸
    }
}