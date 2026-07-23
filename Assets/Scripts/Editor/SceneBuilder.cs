#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// 一键搭建所有场景 - 在 Unity 菜单栏 Tools > Build All Scenes 运行
/// </summary>
public class SceneBuilder : EditorWindow
{
    [MenuItem("Tools/像素生存/搭建所有场景", false, 1)]
    public static void BuildAllScenes()
    {
        SetupSpriteImportSettings();
        BuildLoginScene();
        BuildMainMenuScene();
        BuildGameScene();
        AssetDatabase.Refresh();
        Debug.Log("=== 所有场景搭建完成！===");
    }

    [MenuItem("Tools/像素生存/仅搭建登录场景", false, 2)]
    public static void BuildLoginOnly() { SetupSpriteImportSettings(); BuildLoginScene(); }

    [MenuItem("Tools/像素生存/仅搭建主菜单场景", false, 3)]
    public static void BuildMainMenuOnly() { SetupSpriteImportSettings(); BuildMainMenuScene(); }

    [MenuItem("Tools/像素生存/仅搭建游戏场景", false, 4)]
    public static void BuildGameOnly() { SetupSpriteImportSettings(); BuildGameScene(); }

    // ==================== 精灵导入设置 ====================
    static void SetupSpriteImportSettings()
    {
        string[] sprites = { "player", "tree", "rock", "bush", "water", "grass", "btn_normal", "panel_bg" };
        foreach (var name in sprites)
        {
            string path = $"Assets/Sprites/{name}.png";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 16;  // 像素风 16 PPU
            importer.filterMode = FilterMode.Point; // 点采样 = 像素风
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
        AssetDatabase.Refresh();
    }

    // ==================== 登录场景 ====================
    static void BuildLoginScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var canvas = CreateCanvas("LoginCanvas");

        // 背景
        var bg = CreateImage(canvas.transform, "Background", "panel_bg", new Vector2(0, 0));
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        bg.color = new Color(0.08f, 0.08f, 0.12f, 1f);

        // 标题
        var title = CreateText(canvas.transform, "Title", "像素生存", 48);
        SetRect(title, new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.82f), new Vector2(400, 80));
        title.color = new Color(0.29f, 0.88f, 0.32f); // 绿色像素风
        title.alignment = TextAnchor.MiddleCenter;
        title.fontStyle = FontStyle.Bold;

        // 副标题
        var subtitle = CreateText(canvas.transform, "Subtitle", "-- Pixel Survival --", 20);
        SetRect(subtitle, new Vector2(0.5f, 0.76f), new Vector2(0.5f, 0.76f), new Vector2(300, 40));
        subtitle.color = new Color(0.5f, 0.5f, 0.6f);
        subtitle.alignment = TextAnchor.MiddleCenter;

        // 登录面板
        var panel = CreateImage(canvas.transform, "LoginPanel", "panel_bg", new Vector2(0.5f, 0.45f));
        SetRect(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(320, 280));
        panel.color = new Color(0.1f, 0.1f, 0.16f, 0.95f);

        // 用户名输入框
        var userLabel = CreateText(panel.transform, "UserLabel", "用户名", 18);
        SetRect(userLabel, new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.82f), new Vector2(260, 30));
        userLabel.color = new Color(0.7f, 0.7f, 0.8f);
        userLabel.alignment = TextAnchor.MiddleLeft;

        var userInput = CreateInputField(panel.transform, "UsernameInput", "输入用户名...");
        SetRect(userInput, new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(260, 40));

        // 密码输入框
        var passLabel = CreateText(panel.transform, "PassLabel", "密码", 18);
        SetRect(passLabel, new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.58f), new Vector2(260, 30));
        passLabel.color = new Color(0.7f, 0.7f, 0.8f);
        passLabel.alignment = TextAnchor.MiddleLeft;

        var passInput = CreateInputField(panel.transform, "PasswordInput", "输入密码...");
        SetRect(passInput, new Vector2(0.5f, 0.48f), new Vector2(0.5f, 0.48f), new Vector2(260, 40));
        passInput.GetComponent<InputField>().contentType = InputField.ContentType.Password;

        // 邮箱输入框
        var emailLabel = CreateText(panel.transform, "EmailLabel", "邮箱", 18);
        SetRect(emailLabel, new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), new Vector2(260, 30));
        emailLabel.color = new Color(0.7f, 0.7f, 0.8f);
        emailLabel.alignment = TextAnchor.MiddleLeft;

        var emailInput = CreateInputField(panel.transform, "EmailInput", "输入邮箱...");
        SetRect(emailInput, new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.28f), new Vector2(260, 40));

        // 登录按钮
        var loginBtn = CreatePixelButton(panel.transform, "LoginButton", "登  录", new Vector2(0.3f, 0.1f));
        SetRect(loginBtn, new Vector2(0.3f, 0.12f), new Vector2(0.3f, 0.12f), new Vector2(120, 44));

        // 注册按钮
        var regBtn = CreatePixelButton(panel.transform, "RegisterButton", "注  册", new Vector2(0.7f, 0.1f));
        SetRect(regBtn, new Vector2(0.7f, 0.12f), new Vector2(0.7f, 0.12f), new Vector2(120, 44));
        regBtn.GetComponent<Image>().color = new Color(0.15f, 0.4f, 0.15f);

        // 消息文本
        var msg = CreateText(panel.transform, "MessageText", "", 16);
        SetRect(msg, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(260, 30));
        msg.alignment = TextAnchor.MiddleCenter;

        // 挂载 LoginManager
        var loginMgr = canvas.AddComponent<LoginManager>();
        // 用反射设置字段
        var lmType = typeof(LoginManager);
        SetField(loginMgr, "usernameInput", userInput.GetComponent<InputField>());
        SetField(loginMgr, "passwordInput", passInput.GetComponent<InputField>());
        SetField(loginMgr, "emailInput", emailInput.GetComponent<InputField>());
        SetField(loginMgr, "messageText", msg);
        SetField(loginMgr, "loginPanel", panel.gameObject);
        SetField(loginMgr, "loginButton", loginBtn.GetComponent<Button>());
        SetField(loginMgr, "registerButton", regBtn.GetComponent<Button>());

        // 绑定按钮
        loginBtn.GetComponent<Button>().onClick.AddListener(() => loginMgr.OnLoginClick());
        regBtn.GetComponent<Button>().onClick.AddListener(() => loginMgr.OnRegisterClick());

        SaveScene("LoginScene");
    }

    // ==================== 主菜单场景 ====================
    static void BuildMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var canvas = CreateCanvas("MainMenuCanvas");

        // 背景
        var bg = CreateImage(canvas.transform, "Background", "panel_bg", Vector2.zero);
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        bg.color = new Color(0.06f, 0.1f, 0.06f, 1f);

        // 标题
        var title = CreateText(canvas.transform, "Title", "像素生存", 56);
        SetRect(title, new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), new Vector2(500, 90));
        title.color = new Color(0.29f, 0.88f, 0.32f);
        title.alignment = TextAnchor.MiddleCenter;
        title.fontStyle = FontStyle.Bold;

        // 欢迎文字
        var welcome = CreateText(canvas.transform, "WelcomeText", "欢迎, 冒险者!", 24);
        SetRect(welcome, new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.68f), new Vector2(400, 50));
        welcome.color = new Color(0.8f, 0.8f, 0.9f);
        welcome.alignment = TextAnchor.MiddleCenter;

        // 开始游戏按钮
        var startBtn = CreatePixelButton(canvas.transform, "StartButton", "开始探索", new Vector2(0.5f, 0.5f));
        SetRect(startBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(220, 56));
        var startImg = startBtn.GetComponent<Image>();
        startImg.color = new Color(0.15f, 0.5f, 0.15f);

        // 背包按钮
        var invBtn = CreatePixelButton(canvas.transform, "InventoryButton", "背  包", new Vector2(0.5f, 0.38f));
        SetRect(invBtn, new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), new Vector2(220, 50));

        // 登出按钮
        var logoutBtn = CreatePixelButton(canvas.transform, "LogoutButton", "退出登录", new Vector2(0.5f, 0.26f));
        SetRect(logoutBtn, new Vector2(0.5f, 0.26f), new Vector2(0.5f, 0.26f), new Vector2(220, 50));
        logoutBtn.GetComponent<Image>().color = new Color(0.4f, 0.15f, 0.15f);

        // 版本号
        var version = CreateText(canvas.transform, "Version", "v1.0.0", 14);
        SetRect(version, new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.08f), new Vector2(200, 30));
        version.color = new Color(0.4f, 0.4f, 0.5f);
        version.alignment = TextAnchor.MiddleCenter;

        // 挂载 MainMenuUI
        var menuUI = canvas.AddComponent<MainMenuUI>();
        var mmType = typeof(MainMenuUI);
        SetField(menuUI, "welcomeText", welcome);
        SetField(menuUI, "startGameButton", startBtn.GetComponent<Button>());
        SetField(menuUI, "inventoryButton", invBtn.GetComponent<Button>());
        SetField(menuUI, "logoutButton", logoutBtn.GetComponent<Button>());

        SaveScene("MainMenuScene");
    }

    // ==================== 游戏场景 ====================
    static void BuildGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 主相机设置
        var cam = Camera.main;
        cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.15f, 0.25f, 0.15f);
        cam.gameObject.AddComponent<CameraFollow>();

        // 创建地面（平铺草地）
        CreateGround();

        // 创建玩家
        var player = CreatePlayer();

        // 创建资源点
        SpawnResources();

        // 创建 GameManager
        var gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();

        // 创建 UI Canvas
        var canvas = CreateCanvas("GameCanvas");
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        BuildGameUI(canvas, player);

        SaveScene("GameScene");
    }

    static void CreateGround()
    {
        var ground = new GameObject("Ground");
        var sr = ground.AddComponent<SpriteRenderer>();
        var grassSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/grass.png");
        sr.sprite = grassSprite;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(30, 20);
        sr.sortingOrder = -10;
        ground.transform.position = new Vector3(0, 0, 0);
    }

    static GameObject CreatePlayer()
    {
        var player = new GameObject("Player");
        player.tag = "Player";

        var sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/player.png");
        sr.sortingOrder = 0;
        sr.drawMode = SpriteDrawMode.Simple;
        player.transform.localScale = Vector3.one * 2f;

        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = player.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 0.8f);

        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerStats>();
        player.AddComponent<Inventory>();
        player.AddComponent<CraftingSystem>();
        player.AddComponent<ResourceGathering>();

        player.transform.position = new Vector3(0, 0, 0);

        // 设置 CameraFollow 的 target
        var cam = Camera.main;
        if (cam != null)
        {
            var cf = cam.GetComponent<CameraFollow>();
            if (cf != null) SetField(cf, "target", player.transform);
        }

        return player;
    }

    static void SpawnResources()
    {
        var treeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/tree.png");
        var rockSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/rock.png");
        var bushSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/bush.png");
        var waterSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/water.png");

        // 树木
        Vector2[] treePositions = {
            new(-5, 2), new(-6, 4), new(-4, -3), new(5, 3), new(6, 1),
            new(4, -4), new(-7, -1), new(7, -2), new(-3, 5), new(3, -5)
        };
        foreach (var pos in treePositions)
            CreateResource("Tree", treeSprite, pos, 1.5f);

        // 石头
        Vector2[] rockPositions = {
            new(-3, 0), new(4, 2), new(-2, -4), new(3, -2), new(0, 4),
            new(-5, -3), new(6, -3), new(-1, 5)
        };
        foreach (var pos in rockPositions)
            CreateResource("Rock", rockSprite, pos, 1f);

        // 灌木（食物）
        Vector2[] bushPositions = {
            new(2, 1), new(-1, -2), new(5, -1), new(-4, 2), new(1, 4),
            new(-3, -2), new(4, 0), new(0, -3)
        };
        foreach (var pos in bushPositions)
            CreateResource("Bush", bushSprite, pos, 1f);

        // 水源
        Vector2[] waterPositions = {
            new(-7, 3), new(7, 4), new(0, -5), new(-6, -4)
        };
        foreach (var pos in waterPositions)
            CreateResource("Water", waterSprite, pos, 1.5f);
    }

    static GameObject CreateResource(string name, Sprite sprite, Vector2 pos, float scale)
    {
        var obj = new GameObject(name);
        obj.transform.position = new Vector3(pos.x, pos.y, 0);
        obj.transform.localScale = Vector3.one * scale;
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = -5;
        return obj;
    }

    static void BuildGameUI(Canvas canvas, GameObject player)
    {
        // 顶部状态栏背景
        var topBar = CreateImage(canvas.transform, "TopBar", "panel_bg", Vector2.zero);
        var topRt = topBar.GetComponent<RectTransform>();
        topRt.anchorMin = new Vector2(0, 0.88f);
        topRt.anchorMax = new Vector2(1, 1);
        topRt.sizeDelta = Vector2.zero;
        topRt.offsetMin = new Vector2(0, 0);
        topRt.offsetMax = new Vector2(0, 0);
        topBar.color = new Color(0.05f, 0.05f, 0.08f, 0.85f);

        // HP 条
        var hpBar = CreateSlider(topBar.transform, "HPBar", new Color(0.9f, 0.2f, 0.2f));
        SetRect(hpBar, new Vector2(0.15f, 0.5f), new Vector2(0.15f, 0.5f), new Vector2(180, 20));

        var hpText = CreateText(topBar.transform, "HPText", "HP: 100/100", 14);
        SetRect(hpText, new Vector2(0.15f, 0.85f), new Vector2(0.15f, 0.85f), new Vector2(180, 20));
        hpText.alignment = TextAnchor.MiddleCenter;
        hpText.color = Color.white;

        // 饥饿条
        var hungerBar = CreateSlider(topBar.transform, "HungerBar", new Color(0.9f, 0.6f, 0.1f));
        SetRect(hungerBar, new Vector2(0.42f, 0.5f), new Vector2(0.42f, 0.5f), new Vector2(180, 20));

        var hungerText = CreateText(topBar.transform, "HungerText", "饥饿: 100/100", 14);
        SetRect(hungerText, new Vector2(0.42f, 0.85f), new Vector2(0.42f, 0.85f), new Vector2(180, 20));
        hungerText.alignment = TextAnchor.MiddleCenter;
        hungerText.color = Color.white;

        // 口渴条
        var thirstBar = CreateSlider(topBar.transform, "ThirstBar", new Color(0.2f, 0.5f, 0.9f));
        SetRect(thirstBar, new Vector2(0.7f, 0.5f), new Vector2(0.7f, 0.5f), new Vector2(180, 20));

        var thirstText = CreateText(topBar.transform, "ThirstText", "口渴: 100/100", 14);
        SetRect(thirstText, new Vector2(0.7f, 0.85f), new Vector2(0.7f, 0.85f), new Vector2(180, 20));
        thirstText.alignment = TextAnchor.MiddleCenter;
        thirstText.color = Color.white;

        // 底部操作栏
        var bottomBar = CreateImage(canvas.transform, "BottomBar", "panel_bg", Vector2.zero);
        var botRt = bottomBar.GetComponent<RectTransform>();
        botRt.anchorMin = new Vector2(0, 0);
        botRt.anchorMax = new Vector2(1, 0.12f);
        botRt.sizeDelta = Vector2.zero;
        botRt.offsetMin = new Vector2(0, 0);
        botRt.offsetMax = new Vector2(0, 0);
        bottomBar.color = new Color(0.05f, 0.05f, 0.08f, 0.85f);

        // 采集按钮
        string[] gatherNames = { "木材", "石料", "食物", "取水" };
        float[] gatherX = { 0.12f, 0.34f, 0.56f, 0.78f };
        Button[] gatherBtns = new Button[4];

        for (int i = 0; i < 4; i++)
        {
            var btn = CreatePixelButton(bottomBar.transform, $"Gather{i}", gatherNames[i], new Vector2(gatherX[i], 0.5f));
            SetRect(btn, new Vector2(gatherX[i], 0.5f), new Vector2(gatherX[i], 0.5f), new Vector2(90, 40));
            btn.GetComponent<Image>().color = new Color(0.1f, 0.3f, 0.1f);
            gatherBtns[i] = btn.GetComponent<Button>();
        }

        // 右侧操作按钮
        var eatBtn = CreatePixelButton(bottomBar.transform, "EatButton", "吃", new Vector2(0.9f, 0.5f));
        SetRect(eatBtn, new Vector2(0.9f, 0.5f), new Vector2(0.9f, 0.5f), new Vector2(50, 40));

        var drinkBtn = CreatePixelButton(bottomBar.transform, "DrinkButton", "喝", new Vector2(0.95f, 0.5f));
        SetRect(drinkBtn, new Vector2(0.95f, 0.5f), new Vector2(0.95f, 0.5f), new Vector2(50, 40));

        // 资源文字
        var resText = CreateText(bottomBar.transform, "ResourceText", "木:0 石:0 食:0 水:0", 12);
        SetRect(resText, new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f), new Vector2(400, 20));
        resText.alignment = TextAnchor.MiddleCenter;
        resText.color = new Color(0.7f, 0.7f, 0.7f);

        // 挂载 GameUI
        var gameUI = canvas.gameObject.AddComponent<GameUI>();
        var guiType = typeof(GameUI);
        SetField(gameUI, "hpSlider", hpBar.GetComponent<Slider>());
        SetField(gameUI, "hungerSlider", hungerBar.GetComponent<Slider>());
        SetField(gameUI, "thirstSlider", thirstBar.GetComponent<Slider>());
        SetField(gameUI, "hpText", hpText);
        SetField(gameUI, "hungerText", hungerText);
        SetField(gameUI, "thirstText", thirstText);
        SetField(gameUI, "gatherWoodButton", gatherBtns[0]);
        SetField(gameUI, "gatherStoneButton", gatherBtns[1]);
        SetField(gameUI, "gatherFoodButton", gatherBtns[2]);
        SetField(gameUI, "gatherWaterButton", gatherBtns[3]);
        SetField(gameUI, "eatButton", eatBtn.GetComponent<Button>());
        SetField(gameUI, "drinkButton", drinkBtn.GetComponent<Button>());
    }

    // ==================== 工具方法 ====================
    static Canvas CreateCanvas(string name)
    {
        var go = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800, 480);
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    static Text CreateText(Transform parent, string name, string content, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    static Image CreateImage(Transform parent, string name, string spriteName, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        if (!string.IsNullOrEmpty(spriteName))
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/{spriteName}.png");
            if (sprite != null) img.sprite = sprite;
        }
        img.type = Image.Type.Sliced;
        return img;
    }

    static GameObject CreatePixelButton(Transform parent, string name, string label, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/btn_normal.png");
        if (sprite != null) img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.color = new Color(0.15f, 0.25f, 0.15f);

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = new Color(0.15f, 0.25f, 0.15f);
        colors.highlightedColor = new Color(0.2f, 0.35f, 0.2f);
        colors.pressedColor = new Color(0.1f, 0.15f, 0.1f);
        btn.colors = colors;

        // 按钮文字
        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(go.transform, false);
        var labelText = labelGo.AddComponent<Text>();
        labelText.text = label;
        labelText.fontSize = 20;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.color = new Color(0.8f, 0.95f, 0.8f);
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.fontStyle = FontStyle.Bold;
        labelText.raycastTarget = false;
        var labelRt = labelText.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.sizeDelta = Vector2.zero;

        return go;
    }

    static InputField CreateInputField(Transform parent, string name, string placeholder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var img = go.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.2f);

        var inputField = go.AddComponent<InputField>();

        // Placeholder
        var phGo = new GameObject("Placeholder");
        phGo.transform.SetParent(go.transform, false);
        var phText = phGo.AddComponent<Text>();
        phText.text = placeholder;
        phText.fontSize = 16;
        phText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        phText.color = new Color(0.5f, 0.5f, 0.6f);
        phText.alignment = TextAnchor.MiddleLeft;
        phText.raycastTarget = false;
        var phRt = phText.GetComponent<RectTransform>();
        phRt.anchorMin = Vector2.zero;
        phRt.anchorMax = Vector2.one;
        phRt.sizeDelta = Vector2.zero;
        phRt.offsetMin = new Vector2(10, 0);
        phRt.offsetMax = new Vector2(-10, 0);
        inputField.placeholder = phText;

        // Text
        var txtGo = new GameObject("Text");
        txtGo.transform.SetParent(go.transform, false);
        var txt = txtGo.AddComponent<Text>();
        txt.fontSize = 16;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleLeft;
        txt.raycastTarget = false;
        txt.supportRichText = false;
        var txtRt = txt.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;
        txtRt.offsetMin = new Vector2(10, 0);
        txtRt.offsetMax = new Vector2(-10, 0);
        inputField.textComponent = txt;

        return inputField;
    }

    static Slider CreateSlider(Transform parent, string name, Color fillColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        // Background
        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(go.transform, false);
        var bgImg = bgGo.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f);
        var bgRt = bgGo.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;

        // Fill Area
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        var faRt = fillArea.AddComponent<RectTransform>();
        faRt.anchorMin = Vector2.zero;
        faRt.anchorMax = Vector2.one;
        faRt.sizeDelta = new Vector2(-4, -4);
        faRt.anchoredPosition = new Vector2(2, 0);

        // Fill
        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(fillArea.transform, false);
        var fillImg = fillGo.AddComponent<Image>();
        fillImg.color = fillColor;
        var fillRt = fillGo.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.sizeDelta = Vector2.zero;

        var slider = go.AddComponent<Slider>();
        slider.fillRect = fillRt;
        slider.targetGraphic = fillImg;
        slider.interactable = false;
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 1;

        return slider;
    }

    static void SetRect(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 size)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;
    }

    static void SetRect(Text text, Vector2 anchorMin, Vector2 anchorMax, Vector2 size)
    {
        SetRect(text.gameObject, anchorMin, anchorMax, size);
    }

    static void SetRect(InputField input, Vector2 anchorMin, Vector2 anchorMax, Vector2 size)
    {
        SetRect(input.gameObject, anchorMin, anchorMax, size);
    }

    static void SetRect(Image img, Vector2 anchorMin, Vector2 anchorMax, Vector2 size)
    {
        SetRect(img.gameObject, anchorMin, anchorMax, size);
    }

    static void SetField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        if (field != null)
            field.SetValue(obj, value);
        else
            Debug.LogWarning($"Field '{fieldName}' not found on {obj.GetType().Name}");
    }

    static void SaveScene(string name)
    {
        string path = $"Assets/Scenes/{name}.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), path);
        Debug.Log($"Scene saved: {path}");
    }
}
#endif