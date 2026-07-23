using UnityEngine;

/// <summary>
/// 玩家属性 - HP、饥饿、口渴，随时间衰减
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

    [Header("衰减设置")]
    public float hungerDecayRate = 0.5f;   // 每秒减少的饥饿值
    public float thirstDecayRate = 0.7f;   // 每秒减少的口渴值
    public float hungerDamageRate = 3f;    // 饥饿时每秒扣血量
    public float thirstDamageRate = 5f;    // 口渴时每秒扣血量

    public bool isDead = false;

    void Start()
    {
        currentHP = maxHP;
        currentHunger = maxHunger;
        currentThirst = maxThirst;

        // 加载存档
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

    void Update()
    {
        if (isDead) return;

        // 饥饿和口渴随时间衰减
        currentHunger -= hungerDecayRate * Time.deltaTime;
        currentThirst -= thirstDecayRate * Time.deltaTime;

        // 饥饿为0时扣血
        if (currentHunger <= 0)
        {
            currentHunger = 0;
            currentHP -= hungerDamageRate * Time.deltaTime;
        }

        // 口渴为0时扣血
        if (currentThirst <= 0)
        {
            currentThirst = 0;
            currentHP -= thirstDamageRate * Time.deltaTime;
        }

        // 限制范围
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
        currentThirst = Mathf.Clamp(currentThirst, 0, maxThirst);

        // 死亡检查
        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Eat(float amount)
    {
        currentHunger = Mathf.Clamp(currentHunger + amount, 0, maxHunger);
        Debug.Log($"吃东西 +{amount} 饥饿值 -> {currentHunger}");
    }

    public void Drink(float amount)
    {
        currentThirst = Mathf.Clamp(currentThirst + amount, 0, maxThirst);
        Debug.Log($"喝水 +{amount} 口渴值 -> {currentThirst}");
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
    }

    void Die()
    {
        isDead = true;
        Debug.Log("玩家死亡！");
        GameManager.Instance?.SaveGameProgress();
        // 显示死亡面板
        var gameUI = FindObjectOfType<GameUI>();
        // 可以在这里触发死亡UI
        Invoke(nameof(RestartGame), 3f);
    }

    void RestartGame()
    {
        currentHP = maxHP;
        currentHunger = maxHunger;
        currentThirst = maxThirst;
        isDead = false;
    }
}