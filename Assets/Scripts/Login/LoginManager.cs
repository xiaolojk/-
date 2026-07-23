using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 登录/注册管理器
/// </summary>
public class LoginManager : MonoBehaviour
{
    [Header("UI")]
    public InputField usernameInput;
    public InputField passwordInput;
    public InputField emailInput;
    public Text messageText;
    public GameObject loginPanel;
    public Button loginButton;
    public Button registerButton;

    private const string USER_DATA_PATH = "user_data.json";
    private const string CURRENT_USER_KEY = "CurrentUser";
    private Dictionary<string, UserData> users = new Dictionary<string, UserData>();

    void Start()
    {
        LoadUsers();
        loginButton.onClick.AddListener(OnLogin);
        registerButton.onClick.AddListener(OnRegister);
    }

    void LoadUsers()
    {
        string path = Path.Combine(Application.persistentDataPath, USER_DATA_PATH);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var wrapper = JsonUtility.FromJson<UserDataWrapper>(json);
            if (wrapper != null && wrapper.users != null)
                foreach (var u in wrapper.users) users[u.username] = u;
        }
    }

    void SaveUsers()
    {
        var wrapper = new UserDataWrapper { users = new List<UserData>(users.Values) };
        File.WriteAllText(Path.Combine(Application.persistentDataPath, USER_DATA_PATH),
            JsonUtility.ToJson(wrapper, true));
    }

    public void OnLogin()
    {
        string u = usernameInput.text.Trim();
        string p = passwordInput.text;
        if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(p)) { ShowMsg("用户名和密码不能为空！", Color.red); return; }
        if (!users.ContainsKey(u)) { ShowMsg("用户不存在！", Color.red); return; }
        if (users[u].password != p) { ShowMsg("密码错误！", Color.red); return; }
        PlayerPrefs.SetString(CURRENT_USER_KEY, u);
        PlayerPrefs.Save();
        ShowMsg("登录成功！", Color.green);
        Invoke(nameof(GoMainMenu), 1f);
    }

    public void OnRegister()
    {
        string u = usernameInput.text.Trim();
        string p = passwordInput.text;
        string e = emailInput != null ? emailInput.text.Trim() : "";
        if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(p)) { ShowMsg("用户名和密码不能为空！", Color.red); return; }
        if (u.Length < 3) { ShowMsg("用户名至少3个字符！", Color.red); return; }
        if (p.Length < 4) { ShowMsg("密码至少4个字符！", Color.red); return; }
        if (users.ContainsKey(u)) { ShowMsg("用户名已存在！", Color.red); return; }
        users[u] = new UserData { username = u, password = p, email = e };
        SaveUsers();
        PlayerPrefs.SetString(CURRENT_USER_KEY, u);
        PlayerPrefs.Save();
        ShowMsg("注册成功！", Color.green);
        Invoke(nameof(GoMainMenu), 1f);
    }

    void GoMainMenu() => SceneManager.LoadScene("MainMenuScene");
    void ShowMsg(string m, Color c) { messageText.text = m; messageText.color = c; }
}

[System.Serializable]
public class UserDataWrapper { public List<UserData> users; }