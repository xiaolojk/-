using UnityEngine;
using System.Collections;

/// <summary>
/// 资源采集
/// </summary>
public class ResourceGathering : MonoBehaviour
{
    public float gatherCooldown = 1.2f;
    public int woodPerGather = 3;
    public int stonePerGather = 2;
    public int foodPerGather = 2;
    public int waterPerGather = 2;

    private bool canGather = true;
    private Inventory inventory;

    void Start() { inventory = GetComponent<Inventory>(); }

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
            case ResourceType.Wood: inventory.AddItem("wood", woodPerGather); break;
            case ResourceType.Stone: inventory.AddItem("stone", stonePerGather); break;
            case ResourceType.Food: inventory.AddItem("food", foodPerGather); break;
            case ResourceType.Water: inventory.AddItem("water", waterPerGather); break;
        }
        yield return new WaitForSeconds(gatherCooldown);
        canGather = true;
    }
}

public enum ResourceType { Wood, Stone, Food, Water }