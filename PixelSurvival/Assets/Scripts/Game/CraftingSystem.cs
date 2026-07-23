using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 制造系统 - 配方合成（含铲子、梯子、镐子等新工具）
/// </summary>
public class CraftingSystem : MonoBehaviour
{
    public List<CraftRecipe> recipes = new List<CraftRecipe>();
    private Inventory inventory;

    void Start()
    {
        inventory = GetComponent<Inventory>();
        InitRecipes();
    }

    void InitRecipes()
    {
        recipes = new List<CraftRecipe>
        {
            // === 基础工具 ===
            new CraftRecipe("木镐", "pickaxe", new Dictionary<string, int>
            {
                { "wood", 3 }, { "stone", 2 }
            }, "用来挖掘矿石的好工具"),
            new CraftRecipe("铲子", "shovel", new Dictionary<string, int>
            {
                { "wood", 2 }, { "stone", 2 }
            }, "挖土、造矿井必备"),
            new CraftRecipe("木斧", "tool_axe", new Dictionary<string, int>
            {
                { "wood", 3 }, { "stone", 2 }
            }, "砍树更快"),
            new CraftRecipe("木剑", "weapon_sword", new Dictionary<string, int>
            {
                { "wood", 2 }, { "stone", 3 }
            }, "防身武器"),

            // === 矿井建造 ===
            new CraftRecipe("梯子", "ladder", new Dictionary<string, int>
            {
                { "wood", 4 }
            }, "建造矿井的必需品"),
            new CraftRecipe("矿井入口", "mineshaft_entrance", new Dictionary<string, int>
            {
                { "wood", 5 }, { "ladder", 3 }, { "shovel", 1 }
            }, "创建通往地下的矿井"),

            // === 建筑 ===
            new CraftRecipe("木板", "plank", new Dictionary<string, int>
            {
                { "wood", 2 }
            }, "基础建材"),
            new CraftRecipe("石墙", "stone_wall", new Dictionary<string, int>
            {
                { "stone", 4 }
            }, "坚固的墙壁"),
            new CraftRecipe("营火", "campfire", new Dictionary<string, int>
            {
                { "wood", 5 }, { "stone", 3 }
            }, "取暖、烹饪、驱散辐射"),

            // === 食物 ===
            new CraftRecipe("烤肉", "cooked_meat", new Dictionary<string, int>
            {
                { "food", 1 }
            }, "恢复饥饿值"),
            new CraftRecipe("净水", "clean_water", new Dictionary<string, int>
            {
                { "water", 1 }
            }, "烧开的水，安全饮用"),

            // === 高级 ===
            new CraftRecipe("铁镐", "iron_pickaxe", new Dictionary<string, int>
            {
                { "iron_ore", 3 }, { "wood", 2 }
            }, "更高效的镐子"),
            new CraftRecipe("木筏", "raft", new Dictionary<string, int>
            {
                { "wood", 10 }, { "plank", 5 }
            }, "在海上航行的简易木筏（减少水伤害）"),
            new CraftRecipe("防辐射服", "rad_suit", new Dictionary<string, int>
            {
                { "plank", 5 }, { "iron_ore", 3 }
            }, "减少辐射伤害的简易防护服"),
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

        if (!HasMaterials(recipe))
        {
            Debug.Log($"材料不足！需要: {string.Join(", ", recipe.materials)}");
            return false;
        }

        foreach (var mat in recipe.materials)
            inventory.RemoveItem(mat.Key, mat.Value);

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

    public List<CraftRecipe> GetRecipes() => recipes;
}

[System.Serializable]
public class CraftRecipe
{
    public string name;
    public string resultItem;
    public Dictionary<string, int> materials;
    public string description;

    public CraftRecipe(string name, string resultItem, Dictionary<string, int> materials, string desc = "")
    {
        this.name = name;
        this.resultItem = resultItem;
        this.materials = materials;
        this.description = desc;
    }
}