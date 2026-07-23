using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 登录/注册管理器 - 使用 PlayerPrefs + JSON 本地存储
/// </summary>
public class LoginManager : MonoBehaviour
{
    [Header("UI 引用")]
    public InputField usernameInput;
    public InputField passwordInput;
    public InputField emailInput;
    public Text messageText;
    public GameObject loginPanel;
    public GameObject registerPanel;
    public Button loginButton;
    public Button registerButton;
    public Button switchToRegisterButton;
    public Button switchToLoginButton;
    public Button confirmRegisterButton;

    private const string USER_DATA_PATH = "user_data.json";
    private const string CURRENT_USER_KEY = "CurrentUser";
    private Dictionary<string, UserData> users = new Dictionary<string, UserData>();

    void Start()
    {
        LoadUsers();
        ShowLoginPanel();
    }

    void LoadUsers()
    {
        string path = Path.Combine(Application.persistentDataPath, USER_DATA_PATH);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var wrapper = JsonUtility.FromJson<UserDataWrapper>(json);
            if (wrapper != null && wrapper.users != null)
            {
                foreach (var user in wrapper.users)
                {
                    users[user.username] = user;
                }
            }
        }
    }

    void SaveUsers()
    {
        var wrapper = new UserDataWrapper();
        wrapper.users = new List<UserData>(users.Values);
        string path = Path.Combine(Application.persistentDataPath, USER_DATA_PATH);
        File.WriteAllText(path, JsonUtility.ToJson(wrapper, true));
    }

    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        messageText.text = "";
    }

    public void ShowRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        messageText.text = "";
    }

    public void OnLoginClick()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowMessage("用户名和密码不能为空！", Color.red);
            return;
        }

        if (!users.ContainsKey(username))
        {
            ShowMessage("用户不存在，请先注册！", Color.red);
            return;
        }

        if (users[username].password != password)
        {
            ShowMessage("密码错误！", Color.red);
            return;
        }

        // 登录成功
        PlayerPrefs.SetString(CURRENT_USER_KEY, username);
        PlayerPrefs.Save();
        ShowMessage("登录成功！正在进入游戏...", Color.green);
        Invoke(nameof(GoToMainMenu), 1f);
    }

    public void OnRegisterClick()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;
        string email = emailInput != null ? emailInput.text.Trim() : "";

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowMessage("用户名和密码不能为空！", Color.red);
            return;
        }

        if (username.Length < 3)
        {
            ShowMessage("用户名至少3个字符！", Color.red);
            return;
        }

        if (password.Length < 4)
        {
            ShowMessage("密码至少4个字符！", Color.red);
            return;
        }

        if (users.ContainsKey(username))
        {
            ShowMessage("用户名已存在！", Color.red);
            return;
        }

        var newUser = new UserData
        {
            username = username,
            password = password,
            email = email
        };

        users[username] = newUser;
        SaveUsers();

        // 自动登录
        PlayerPrefs.SetString(CURRENT_USER_KEY, username);
        PlayerPrefs.Save();
        ShowMessage("注册成功！正在进入游戏...", Color.green);
        Invoke(nameof(GoToMainMenu), 1f);
    }

    void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    void ShowMessage(string msg, Color color)
    {
        messageText.text = msg;
        messageText.color = color;
    }

    void OnDestroy()
    {
        // 解除按钮事件绑定
    }
}

[System.Serializable]
public class UserDataWrapper
{
    public List<UserData> users;
}