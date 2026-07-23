using UnityEngine;
using System.Collections;

/// <summary>
/// 资源采集 - 采集木材、石料、食物、水
/// </summary>
public class ResourceGathering : MonoBehaviour
{
    public float gatherRange = 2f;
    public float gatherCooldown = 1.5f;

    [Header("采集产量")]
    public int woodPerGather = 3;
    public int stonePerGather = 2;
    public int foodPerGather = 2;
    public int waterPerGather = 2;

    private bool canGather = true;
    private Inventory inventory;
    private PlayerStats stats;

    void Start()
    {
        inventory = FindObjectOfType<Inventory>();
        stats = FindObjectOfType<PlayerStats>();
    }

    public void GatherResource(ResourceType type)
    {
        if (!canGather) return;
        StartCoroutine(GatherRoutine(type));
    }

    IEnumerator GatherRoutine(ResourceType type)
    {
        canGather = false;

        switch (type)
        {
            case ResourceType.Wood:
                inventory.AddItem("wood", woodPerGather);
                Debug.Log($"采集了 {woodPerGather} 个木材");
                break;
            case ResourceType.Stone:
                inventory.AddItem("stone", stonePerGather);
                Debug.Log($"采集了 {stonePerGather} 个石料");
                break;
            case ResourceType.Food:
                inventory.AddItem("food", foodPerGather);
                Debug.Log($"采集了 {foodPerGather} 个食物");
                break;
            case ResourceType.Water:
                inventory.AddItem("water", waterPerGather);
                Debug.Log($"采集了 {waterPerGather} 份水");
                break;
        }

        yield return new WaitForSeconds(gatherCooldown);
        canGather = true;
    }
}

public enum ResourceType
{
    Wood,
    Stone,
    Food,
    Water
}