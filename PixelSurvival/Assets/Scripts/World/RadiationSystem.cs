using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 辐射系统 - 接触放射性海水扣血
/// 核泄漏导致全球海水污染，接触即受辐射伤害
/// </summary>
public class RadiationSystem : MonoBehaviour
{
    [Header("辐射伤害")]
    public float waterDamagePerSecond = 8f;    // 水中每秒扣血
    public float radiationBuildRate = 5f;      // 辐射积累速度
    public float radiationDecayRate = 0.5f;    // 离开水面后辐射衰减

    [Header("辐射等级")]
    public float currentRadiation = 0f;
    public float maxRadiation = 100f;
    public bool isIrradiated = false;          // 是否已受辐射

    [Header("辐射症状")]
    public float radiationDamageThreshold = 30f; // 辐射超过此值开始额外扣血
    public float radiationDamageRate = 2f;       // 辐射超标每秒扣血

    private PlayerStats stats;
    private WorldManager world;
    private PlayerController player;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
        player = GetComponent<PlayerController>();
        world = WorldManager.Instance;
    }

    void Update()
    {
        if (stats == null || stats.isDead) return;

        bool inWater = world != null && world.isPlayerInWater;

        if (inWater)
        {
            // 水中：积累辐射 + 直接扣血
            currentRadiation += radiationBuildRate * Time.deltaTime;
            currentRadiation = Mathf.Clamp(currentRadiation, 0, maxRadiation);
            stats.TakeDamage(waterDamagePerSecond * Time.deltaTime, DamageType.Radiation);
            isIrradiated = true;
        }
        else
        {
            // 离开水面：辐射缓慢衰减
            if (currentRadiation > 0)
            {
                currentRadiation -= radiationDecayRate * Time.deltaTime;
                if (currentRadiation < 0) currentRadiation = 0;
            }
        }

        // 辐射超标额外扣血
        if (currentRadiation >= radiationDamageThreshold)
        {
            stats.TakeDamage(radiationDamageRate * Time.deltaTime, DamageType.Radiation);
        }
    }

    /// <summary>
    /// 获取辐射等级描述
    /// </summary>
    public string GetRadiationLevel()
    {
        if (currentRadiation < 10) return "安全";
        if (currentRadiation < 30) return "轻微";
        if (currentRadiation < 60) return "中度";
        if (currentRadiation < 85) return "严重";
        return "致命";
    }

    public Color GetRadiationColor()
    {
        if (currentRadiation < 10) return Color.white;
        if (currentRadiation < 30) return new Color(1f, 0.9f, 0.3f);
        if (currentRadiation < 60) return new Color(1f, 0.5f, 0.1f);
        return new Color(1f, 0.1f, 0.05f);
    }

    /// <summary>
    /// 喝生水受到的辐射伤害
    /// </summary>
    public void TakeRawWaterDamage()
    {
        currentRadiation += 15f;
        if (stats != null)
            stats.TakeDamage(5f, DamageType.Radiation);
    }
}