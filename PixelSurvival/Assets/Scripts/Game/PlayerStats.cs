using UnityEngine;

/// <summary>
/// 伤害类型
/// </summary>
public enum DamageType
{
    Normal,     // 普通伤害
    Radiation,  // 辐射伤害
    Hunger,     // 饥饿伤害
    Thirst,     // 口渴伤害
    Fall,       // 坠落伤害
    Enemy,      // 敌人伤害
}

/// <summary>
/// 玩家属性 - HP/饥饿/口渴/辐射 + 时间衰减
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("基础属性")]
    public float maxHP = 100f;
    public float maxHunger = 100f;
    public float maxThirst = 100f;

    [Header("当前值")]
    public float currentHP;
    public float currentHunger;
    public float currentThirst;

    [Header("衰减速率（每秒）")]
    public float hungerDecayRate = 0.3f;
    public float thirstDecayRate = 0.4f;
    public float hungerDamageRate = 2f;
    public float thirstDamageRate = 3f;

    public bool isDead = false;
    public float deathTimer = 0f;

    private RadiationSystem radiation;

    void Start()
    {
        currentHP = maxHP;
        currentHunger = maxHunger;
        currentThirst = maxThirst;
        radiation = GetComponent<RadiationSystem>();

        LoadSaveData();
    }

    void Update()
    {
        if (isDead)
        {
            deathTimer += Time.deltaTime;
            if (deathTimer >= 3f) Respawn();
            return;
        }

        // 饥饿/口渴衰减
        currentHunger -= hungerDecayRate * Time.deltaTime;
        currentThirst -= thirstDecayRate * Time.deltaTime;

        // 饥饿为0扣血
        if (currentHunger <= 0)
        {
            currentHunger = 0;
            TakeDamage(hungerDamageRate * Time.deltaTime, DamageType.Hunger);
        }

        // 口渴为0扣血
        if (currentThirst <= 0)
        {
            currentThirst = 0;
            TakeDamage(thirstDamageRate * Time.deltaTime, DamageType.Thirst);
        }

        // 限制范围
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
        currentThirst = Mathf.Clamp(currentThirst, 0, maxThirst);

        if (currentHP <= 0) Die();
    }

    public void Eat(float amount)
    {
        if (isDead) return;
        currentHunger = Mathf.Clamp(currentHunger + amount, 0, maxHunger);
        currentHP = Mathf.Clamp(currentHP + amount * 0.1f, 0, maxHP); // 吃东西回少量血
    }

    public void Drink(float amount)
    {
        if (isDead) return;
        currentThirst = Mathf.Clamp(currentThirst + amount, 0, maxThirst);
    }

    public void TakeDamage(float damage, DamageType type = DamageType.Normal)
    {
        if (isDead) return;
        currentHP -= damage;
        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
    }

    void Die()
    {
        isDead = true;
        deathTimer = 0f;
        Debug.Log("你死了...辐射海水夺走了你的生命");
        // 掉落物品
        var inventory = GetComponent<Inventory>();
        if (inventory != null)
        {
            // 随机掉落一些物品
            inventory.DropRandomItems(3);
        }
    }

    void Respawn()
    {
        isDead = false;
        currentHP = maxHP * 0.5f;
        currentHunger = maxHunger * 0.5f;
        currentThirst = maxThirst * 0.5f;

        // 复活到最近的岛屿
        var world = WorldManager.Instance;
        if (world != null)
        {
            var spawnPos = world.GetRandomIslandPosition();
            transform.position = new Vector3(spawnPos.x, spawnPos.y, 0);
        }

        if (radiation != null)
        {
            radiation.currentRadiation = 0;
        }

        Debug.Log("你在一座岛屿上醒来...记忆模糊...");
    }

    void LoadSaveData()
    {
        var gm = GameManager.Instance;
        if (gm != null && gm.currentUserData != null)
        {
            var data = gm.currentUserData;
            maxHP = data.maxHP;
            maxHunger = data.maxHunger;
            maxThirst = data.maxThirst;
            currentHP = maxHP;
            currentHunger = maxHunger;
            currentThirst = maxThirst;
        }
    }
}