using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 制造系统 - 配方合成
/// </summary>
public class CraftingSystem : MonoBehaviour
{
    public List<CraftRecipe> recipes = new List<CraftRecipe>();

    private Inventory inventory;

    void Start()
    {
        inventory = FindObjectOfType<Inventory>();
        InitRecipes();
    }

    void InitRecipes()
    {
        recipes = new List<CraftRecipe>
        {
            // 基础工具
            new CraftRecipe("木斧", "tool_axe", new Dictionary<string, int>
            {
                { "wood", 3 },
                { "stone", 2 }
            }),
            new CraftRecipe("木镐", "tool_pickaxe", new Dictionary<string, int>
            {
                { "wood", 3 },
                { "stone", 2 }
            }),
            new CraftRecipe("营火", "campfire", new Dictionary<string, int>
            {
                { "wood", 5 },
                { "stone", 3 }
            }),
            new CraftRecipe("木剑", "weapon_sword", new Dictionary<string, int>
            {
                { "wood", 2 },
                { "stone", 3 }
            }),
            // 建筑材料
            new CraftRecipe("木板", "plank", new Dictionary<string, int>
            {
                { "wood", 2 }
            }),
            new CraftRecipe("石墙", "stone_wall", new Dictionary<string, int>
            {
                { "stone", 4 }
            }),
            // 食物
            new CraftRecipe("烤肉", "cooked_meat", new Dictionary<string, int>
            {
                { "food", 1 }
            }),
        };
    }

    public bool CraftItem(string recipeName)
    {
        var recipe = recipes.Find(r => r.name == recipeName);
        if (recipe == null)
        {
            Debug.Log($"配方 {recipeName} 不存在！");
            return false;
        }

        // 检查材料
        if (!HasMaterials(recipe))
        {
            Debug.Log("材料不足！");
            return false;
        }

        // 消耗材料
        foreach (var mat in recipe.materials)
        {
            inventory.RemoveItem(mat.Key, mat.Value);
        }

        // 添加成品
        inventory.AddItem(recipe.resultItem, 1);
        Debug.Log($"制造成功: {recipeName}!");
        return true;
    }

    bool HasMaterials(CraftRecipe recipe)
    {
        foreach (var mat in recipe.materials)
        {
            if (!inventory.HasItem(mat.Key, mat.Value))
                return false;
        }
        return true;
    }

    public List<CraftRecipe> GetAvailableRecipes()
    {
        return recipes;
    }
}

[System.Serializable]
public class CraftRecipe
{
    public string name;
    public string resultItem;
    public Dictionary<string, int> materials;

    public CraftRecipe(string name, string resultItem, Dictionary<string, int> materials)
    {
        this.name = name;
        this.resultItem = resultItem;
        this.materials = materials;
    }
}