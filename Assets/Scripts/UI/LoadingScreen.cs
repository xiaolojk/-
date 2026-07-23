using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 加载画面 - Orgc 制作团队，像素风加载动画
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    [Header("UI")]
    public Image logoImage;
    public Text teamText;
    public Text loadingText;
    public Slider progressBar;
    public Text tipText;

    [Header("设置")]
    public float minLoadTime = 3f;
    public string[] tips;

    void Start()
    {
        tips = new string[]
        {
            "海水具有放射性，远离水面！",
            "用铲子和梯子可以建造矿井",
            "寻找天然洞穴来快速深入地下",
            "留意散落的线索，拼凑你的记忆",
            "营火可以驱散辐射",
            "木筏能让你在海上航行",
            "地下越深，矿石越丰富",
            "全球变暖融化了所有冰川...",
            "核泄漏污染了所有的海水",
            "你并非唯一的幸存者..."
        };

        StartCoroutine(LoadSequence());
    }

    IEnumerator LoadSequence()
    {
        // 显示 Orgc 团队标志
        teamText.text = "Orgc 出品";
        teamText.color = new Color(0.29f, 0.88f, 0.32f);

        // 像素风文字动画
        float elapsed = 0;
        string[] dots = { "", ".", "..", "..." };
        int dotIdx = 0;
        float dotTimer = 0;

        while (elapsed < minLoadTime)
        {
            elapsed += Time.deltaTime;

            // 加载进度条
            float progress = Mathf.Clamp01(elapsed / minLoadTime);
            progressBar.value = progress;

            // 加载文字动画
            dotTimer += Time.deltaTime;
            if (dotTimer > 0.5f)
            {
                dotTimer = 0;
                dotIdx = (dotIdx + 1) % 4;
                loadingText.text = "加载中" + dots[dotIdx];
            }

            // 切换提示
            if (elapsed > 1f && tipText != null)
            {
                tipText.text = "提示: " + tips[Random.Range(0, tips.Length)];
            }

            yield return null;
        }

        progressBar.value = 1f;
        loadingText.text = "完成!";

        yield return new WaitForSeconds(0.5f);

        // 进入登录场景
        SceneManager.LoadScene("LoginScene");
    }
}