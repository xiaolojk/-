using System;

/// <summary>
/// 用户数据模型 - 用于登录注册
/// </summary>
[Serializable]
public class UserData
{
    public string username;
    public string password;
    public string email;

    // 游戏进度
    public float maxHP = 100f;
    public float maxHunger = 100f;
    public float maxThirst = 100f;
    public int wood = 0;
    public int stone = 0;
    public int food = 0;
    public int water = 0;
    public float posX = 0f;
    public float posY = 0f;
    public string[] inventory = new string[24];
    public int[] inventoryCounts = new int[24];
}