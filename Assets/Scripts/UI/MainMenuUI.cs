using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 主菜单
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    public Text welcomeText;
    public Button startGameButton;
    public Button logoutButton;

    void Start()
    {
        string username = PlayerPrefs.GetString("CurrentUser", "失忆者");
        welcomeText.text = "迷失者: " + username;
        startGameButton.onClick.AddListener(() => SceneManager.LoadScene("GameScene"));
        logoutButton.onClick.AddListener(() =>
        {
            PlayerPrefs.DeleteKey("CurrentUser");
            SceneManager.LoadScene("LoginScene");
        });
    }
}