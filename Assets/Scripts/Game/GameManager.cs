using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// 游戏全局管理器 - 单例
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public UserData currentUserData;
    public bool isGamePaused = false;

    private const string CURRENT_USER_KEY = "CurrentUser";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCurrentUser();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadCurrentUser()
    {
        string username = PlayerPrefs.GetString(CURRENT_USER_KEY, "");
        if (string.IsNullOrEmpty(username)) return;

        string path = Path.Combine(Application.persistentDataPath, "user_data.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var wrapper = JsonUtility.FromJson<UserDataWrapper>(json);
            if (wrapper != null && wrapper.users != null)
            {
                foreach (var user in wrapper.users)
                {
                    if (user.username == username)
                    {
                        currentUserData = user;
                        break;
                    }
                }
            }
        }
    }

    public void SaveGameProgress()
    {
        if (currentUserData == null) return;

        var stats = FindObjectOfType<PlayerStats>();
        var player = FindObjectOfType<PlayerController>();
        var inventory = FindObjectOfType<Inventory>();

        if (stats != null)
        {
            currentUserData.maxHP = stats.maxHP;
            currentUserData.maxHunger = stats.maxHunger;
            currentUserData.maxThirst = stats.maxThirst;
        }
        if (player != null)
        {
            currentUserData.posX = player.transform.position.x;
            currentUserData.posY = player.transform.position.y;
        }
        if (inventory != null)
        {
            currentUserData.inventory = inventory.items;
            currentUserData.inventoryCounts = inventory.counts;
            currentUserData.wood = inventory.GetItemCount("wood");
            currentUserData.stone = inventory.GetItemCount("stone");
            currentUserData.food = inventory.GetItemCount("food");
            currentUserData.water = inventory.GetItemCount("water");
        }

        // 保存到文件
        string path = Path.Combine(Application.persistentDataPath, "user_data.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var wrapper = JsonUtility.FromJson<UserDataWrapper>(json);
            if (wrapper != null && wrapper.users != null)
            {
                for (int i = 0; i < wrapper.users.Count; i++)
                {
                    if (wrapper.users[i].username == currentUserData.username)
                    {
                        wrapper.users[i] = currentUserData;
                        break;
                    }
                }
                File.WriteAllText(path, JsonUtility.ToJson(wrapper, true));
            }
        }
    }

    public void ReturnToMainMenu()
    {
        SaveGameProgress();
        SceneManager.LoadScene("MainMenuScene");
    }

    public void QuitGame()
    {
        SaveGameProgress();
        Application.Quit();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause) SaveGameProgress();
    }

    void OnApplicationQuit()
    {
        SaveGameProgress();
    }
}