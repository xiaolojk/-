using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 主菜单 UI
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    public Text welcomeText;
    public Button startGameButton;
    public Button inventoryButton;
    public Button settingsButton;
    public Button logoutButton;

    void Start()
    {
        string username = PlayerPrefs.GetString("CurrentUser", "冒险者");
        welcomeText.text = "欢迎, " + username + "!";

        startGameButton.onClick.AddListener(StartGame);
        logoutButton.onClick.AddListener(Logout);
    }

    void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    void Logout()
    {
        PlayerPrefs.DeleteKey("CurrentUser");
        SceneManager.LoadScene("LoginScene");
    }
}