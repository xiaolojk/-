#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

/// <summary>
/// 迷茫蓝海 - 一键搭建所有场景
/// Orgc 制作团队
/// </summary>
public class SceneBuilder : EditorWindow
{
    [MenuItem("Tools/迷茫蓝海/搭建所有场景", false, 1)]
    public static void BuildAll()
    {
        SetupSprites();
        BuildLoadingScene();
        BuildLoginScene();
        BuildMainMenuScene();
        BuildGameScene();
        AssetDatabase.Refresh();
        Debug.Log("=== 迷茫蓝海 场景搭建完成！Orgc 出品 ===");
    }

    [MenuItem("Tools/迷茫蓝海/仅搭建加载场景", false, 2)]
    public static void BuildLoadOnly() { SetupSprites(); BuildLoadingScene(); }
    [MenuItem("Tools/迷茫蓝海/仅搭建登录场景", false, 3)]
    public static void BuildLoginOnly() { SetupSprites(); BuildLoginScene(); }
    [MenuItem("Tools/迷茫蓝海/仅搭建游戏场景", false, 4)]
    public static void BuildGameOnly() { SetupSprites(); BuildGameScene(); }

    static void SetupSprites()
    {
        string[] names = { "player","water_danger","ladder","shovel","pickaxe","cave_entrance",
            "dirt_ug","stone_ug","clue_note","island_tile","loading_logo","wood","rock","bush","water","grass","btn_normal","panel_bg" };
        foreach (var n in names)
        {
            string p = $"Assets/Sprites/{n}.png";
            var imp = AssetImporter.GetAtPath(p) as TextureImporter;
            if (imp == null) continue;
            imp.textureType = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = 16;
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled = false;
            imp.SaveAndReimport();
        }
        AssetDatabase.Refresh();
    }

    // ============ 加载场景 ============
    static void BuildLoadingScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var cam = Camera.main;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.06f);
        var canvas = CreateCanvas("LoadingCanvas");

        // Logo
        var logo = CreateImage(canvas.transform, "Logo", "loading_logo", Vector2.zero);
        SetRt(logo, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(256, 128));

        // 团队名
        var team = Txt(canvas.transform, "TeamText", "Orgc 出品", 28);
        SetRt(team, new Vector2(0.5f, 0.35f), new Vector2(0.5f, 0.35f), new Vector2(300, 40));
        team.color = new Color(0.29f, 0.88f, 0.32f);
        team.alignment = TextAnchor.MiddleCenter;
        team.fontStyle = FontStyle.Bold;

        // 加载文字
        var loadTxt = Txt(canvas.transform, "LoadingText", "加载中...", 18);
        SetRt(loadTxt, new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(200, 30));
        loadTxt.color = new Color(0.6f, 0.6f, 0.7f);
        loadTxt.alignment = TextAnchor.MiddleCenter;

        // 进度条
        var bar = Sld(canvas.transform, "ProgressBar", new Color(0.29f, 0.88f, 0.32f));
        SetRt(bar, new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), new Vector2(300, 16));

        // 提示
        var tip = Txt(canvas.transform, "TipText", "提示: 海水具有放射性，远离水面！", 14);
        SetRt(tip, new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.08f), new Vector2(500, 30));
        tip.color = new Color(0.5f, 0.5f, 0.5f);
        tip.alignment = TextAnchor.MiddleCenter;

        // 挂载 LoadingScreen
        var ls = canvas.gameObject.AddComponent<LoadingScreen>();
        SetF(ls, "logoImage", logo.GetComponent<Image>());
        SetF(ls, "teamText", team);
        SetF(ls, "loadingText", loadTxt);
        SetF(ls, "progressBar", bar.GetComponent<Slider>());
        SetF(ls, "tipText", tip);

        SaveScene("LoadingScene");
    }

    // ============ 登录场景 ============
    static void BuildLoginScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var cam = Camera.main;
        cam.backgroundColor = new Color(0.04f, 0.06f, 0.1f);
        var canvas = CreateCanvas("LoginCanvas");

        // 背景
        var bg = Img(canvas.transform, "Bg", "panel_bg");
        var brt = bg.GetComponent<RectTransform>();
        brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
        brt.sizeDelta = Vector2.zero;
        bg.color = new Color(0.05f, 0.07f, 0.12f);

        // 标题
        var title = Txt(canvas.transform, "Title", "迷茫蓝海", 52);
        SetRt(title, new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), new Vector2(400, 80));
        title.color = new Color(0.29f, 0.88f, 0.32f);
        title.alignment = TextAnchor.MiddleCenter;
        title.fontStyle = FontStyle.Bold;

        var sub = Txt(canvas.transform, "Subtitle", "Lost Blue Sea", 20);
        SetRt(sub, new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), new Vector2(300, 35));
        sub.color = new Color(0.4f, 0.5f, 0.6f);
        sub.alignment = TextAnchor.MiddleCenter;

        // 面板
        var panel = Img(canvas.transform, "Panel", "panel_bg");
        SetRt(panel, new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(320, 260));
        panel.color = new Color(0.08f, 0.08f, 0.15f, 0.95f);

        var ul = Txt(panel.transform, "UserLabel", "用户名", 16);
        SetRt(ul, new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), new Vector2(260, 25));
        ul.color = new Color(0.7f, 0.7f, 0.8f); ul.alignment = TextAnchor.MiddleLeft;

        var uin = Inp(panel.transform, "UserInput", "输入用户名...");
        SetRt(uin, new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(260, 38));

        var pl = Txt(panel.transform, "PassLabel", "密码", 16);
        SetRt(pl, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), new Vector2(260, 25));
        pl.color = new Color(0.7f, 0.7f, 0.8f); pl.alignment = TextAnchor.MiddleLeft;

        var pin = Inp(panel.transform, "PassInput", "输入密码...");
        SetRt(pin, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(260, 38));
        pin.GetComponent<InputField>().contentType = InputField.ContentType.Password;

        var el = Txt(panel.transform, "EmailLabel", "邮箱", 16);
        SetRt(el, new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), new Vector2(260, 25));
        el.color = new Color(0.7f, 0.7f, 0.8f); el.alignment = TextAnchor.MiddleLeft;

        var ein = Inp(panel.transform, "EmailInput", "输入邮箱...");
        SetRt(ein, new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(260, 38));

        var logB = Btn(panel.transform, "LoginBtn", "登  录", new Vector2(0.3f, 0.1f));
        SetRt(logB, new Vector2(0.3f, 0.12f), new Vector2(0.3f, 0.12f), new Vector2(120, 42));

        var regB = Btn(panel.transform, "RegBtn", "注  册", new Vector2(0.7f, 0.1f));
        SetRt(regB, new Vector2(0.7f, 0.12f), new Vector2(0.7f, 0.12f), new Vector2(120, 42));
        regB.GetComponent<Image>().color = new Color(0.12f, 0.35f, 0.12f);

        var msg = Txt(panel.transform, "Msg", "", 14);
        SetRt(msg, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(260, 25));
        msg.alignment = TextAnchor.MiddleCenter;

        var lm = canvas.gameObject.AddComponent<LoginManager>();
        SetF(lm, "usernameInput", uin.GetComponent<InputField>());
        SetF(lm, "passwordInput", pin.GetComponent<InputField>());
        SetF(lm, "emailInput", ein.GetComponent<InputField>());
        SetF(lm, "messageText", msg);
        SetF(lm, "loginPanel", panel.gameObject);
        SetF(lm, "loginButton", logB.GetComponent<Button>());
        SetF(lm, "registerButton", regB.GetComponent<Button>());

        SaveScene("LoginScene");
    }

    // ============ 主菜单场景 ============
    static void BuildMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        Camera.main.backgroundColor = new Color(0.03f, 0.08f, 0.04f);
        var canvas = CreateCanvas("MenuCanvas");

        var bg = Img(canvas.transform, "Bg", "panel_bg");
        var brt = bg.GetComponent<RectTransform>();
        brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
        brt.sizeDelta = Vector2.zero;
        bg.color = new Color(0.04f, 0.08f, 0.05f);

        var title = Txt(canvas.transform, "Title", "迷茫蓝海", 48);
        SetRt(title, new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.8f), new Vector2(400, 80));
        title.color = new Color(0.29f, 0.88f, 0.32f);
        title.alignment = TextAnchor.MiddleCenter; title.fontStyle = FontStyle.Bold;

        var wel = Txt(canvas.transform, "Welcome", "失忆者", 22);
        SetRt(wel, new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(300, 40));
        wel.color = new Color(0.7f, 0.7f, 0.8f); wel.alignment = TextAnchor.MiddleCenter;

        var start = Btn(canvas.transform, "StartBtn", "开始探索", new Vector2(0.5f, 0.5f));
        SetRt(start, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(220, 54));
        start.GetComponent<Image>().color = new Color(0.12f, 0.4f, 0.12f);

        var logB = Btn(canvas.transform, "LogoutBtn", "退出登录", new Vector2(0.5f, 0.35f));
        SetRt(logB, new Vector2(0.5f, 0.35f), new Vector2(0.5f, 0.35f), new Vector2(220, 50));
        logB.GetComponent<Image>().color = new Color(0.35f, 0.12f, 0.12f);

        var ver = Txt(canvas.transform, "Version", "Orgc v1.0", 14);
        SetRt(ver, new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.08f), new Vector2(200, 25));
        ver.color = new Color(0.4f, 0.4f, 0.5f); ver.alignment = TextAnchor.MiddleCenter;

        var mu = canvas.gameObject.AddComponent<MainMenuUI>();
        SetF(mu, "welcomeText", wel);
        SetF(mu, "startGameButton", start.GetComponent<Button>());
        SetF(mu, "logoutButton", logB.GetComponent<Button>());

        SaveScene("MainMenuScene");
    }

    // ============ 游戏场景 ============
    static void BuildGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var cam = Camera.main;
        cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.05f, 0.08f, 0.15f);
        cam.gameObject.AddComponent<CameraFollow>();

        // 岛屿（地面）
        var island = new GameObject("Island");
        var isr = island.AddComponent<SpriteRenderer>();
        isr.sprite = LoadSprite("island_tile");
        isr.drawMode = SpriteDrawMode.Tiled;
        isr.size = new Vector2(20, 3);
        isr.sortingOrder = -5;
        island.transform.position = new Vector3(0, 0, 0);

        // 水面背景
        var water = new GameObject("WaterSurface");
        var wsr = water.AddComponent<SpriteRenderer>();
        wsr.sprite = LoadSprite("water_danger");
        wsr.drawMode = SpriteDrawMode.Tiled;
        wsr.size = new Vector2(40, 10);
        wsr.sortingOrder = -10;
        water.transform.position = new Vector3(0, -5, 1);

        // 玩家
        var player = new GameObject("Player");
        player.tag = "Player";
        var psr = player.AddComponent<SpriteRenderer>();
        psr.sprite = LoadSprite("player");
        psr.sortingOrder = 0;
        player.transform.localScale = Vector3.one * 2f;
        player.transform.position = new Vector3(0, 2, 0);

        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        var col = player.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 0.8f);

        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerStats>();
        player.AddComponent<Inventory>();
        player.AddComponent<CraftingSystem>();
        player.AddComponent<ResourceGathering>();
        player.AddComponent<MiningSystem>();
        player.AddComponent<RadiationSystem>();

        // 散布资源
        SpawnResources();

        // GameManager
        var gm = new GameObject("GameManager");
        gm.AddComponent<GameManager>();
        gm.AddComponent<WorldManager>();
        gm.AddComponent<StoryManager>();

        // UI Canvas
        var canvas = CreateCanvas("GameCanvas");
        canvas.sortingOrder = 10;

        BuildGameHUD(canvas, player);

        // 设置 CameraFollow target
        var cf = cam.GetComponent<CameraFollow>();
        SetF(cf, "target", player.transform);

        SaveScene("GameScene");
    }

    static void SpawnResources()
    {
        var treeSpr = LoadSprite("tree");
        var rockSpr = LoadSprite("rock");
        var bushSpr = LoadSprite("bush");
        var caveSpr = LoadSprite("cave_entrance");

        Vector2[] trees = { new(-5,2.5f), new(-6,3.5f), new(-4,1.5f), new(5,3), new(6,2), new(4,1.5f), new(-7,2), new(7,1.5f), new(-3,4), new(3,3.5f) };
        foreach (var p in trees) SpawnRes("Tree", treeSpr, p, 1.5f);

        Vector2[] rocks = { new(-3,1.5f), new(4,2.5f), new(-2,1), new(3,1.5f), new(0,3.5f), new(-5,1), new(6,1.5f) };
        foreach (var p in rocks) SpawnRes("Rock", rockSpr, p, 1f);

        Vector2[] bushes = { new(2,2), new(-1,1), new(5,2), new(-4,2.5f), new(1,3.5f), new(-3,1.5f), new(4,1.5f) };
        foreach (var p in bushes) SpawnRes("Bush", bushSpr, p, 1f);

        // 洞穴入口
        SpawnRes("Cave", caveSpr, new Vector2(-7, 1.5f), 2f);
        SpawnRes("Cave", caveSpr, new Vector2(8, 2), 2f);
    }

    static GameObject SpawnRes(string name, Sprite sprite, Vector2 pos, float scale)
    {
        var obj = new GameObject(name);
        obj.transform.position = new Vector3(pos.x, pos.y, 0);
        obj.transform.localScale = Vector3.one * scale;
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = -3;
        return obj;
    }

    static void BuildGameHUD(Canvas canvas, GameObject player)
    {
        // 顶部状态栏
        var topBar = Img(canvas.transform, "TopBar", "panel_bg");
        var tr = topBar.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0, 0.85f); tr.anchorMax = Vector2.one;
        tr.sizeDelta = Vector2.zero;
        topBar.color = new Color(0.03f, 0.03f, 0.06f, 0.85f);

        var hpBar = Sld(topBar.transform, "HPBar", new Color(0.9f, 0.15f, 0.15f));
        SetRt(hpBar, new Vector2(0.1f, 0.5f), new Vector2(0.1f, 0.5f), new Vector2(140, 16));
        var hpT = Txt(topBar.transform, "HPT", "HP 100/100", 12);
        SetRt(hpT, new Vector2(0.1f, 0.82f), new Vector2(0.1f, 0.82f), new Vector2(140, 18));
        hpT.alignment = TextAnchor.MiddleCenter; hpT.color = Color.white;

        var huBar = Sld(topBar.transform, "HungerBar", new Color(0.9f, 0.5f, 0.1f));
        SetRt(huBar, new Vector2(0.3f, 0.5f), new Vector2(0.3f, 0.5f), new Vector2(140, 16));
        var huT = Txt(topBar.transform, "HungerT", "饥饿 100/100", 12);
        SetRt(huT, new Vector2(0.3f, 0.82f), new Vector2(0.3f, 0.82f), new Vector2(140, 18));
        huT.alignment = TextAnchor.MiddleCenter; huT.color = Color.white;

        var thBar = Sld(topBar.transform, "ThirstBar", new Color(0.2f, 0.5f, 0.9f));
        SetRt(thBar, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(140, 16));
        var thT = Txt(topBar.transform, "ThirstT", "口渴 100/100", 12);
        SetRt(thT, new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.82f), new Vector2(140, 18));
        thT.alignment = TextAnchor.MiddleCenter; thT.color = Color.white;

        var radBar = Sld(topBar.transform, "RadBar", new Color(1f, 0.4f, 0f));
        SetRt(radBar, new Vector2(0.7f, 0.5f), new Vector2(0.7f, 0.5f), new Vector2(140, 16));
        var radT = Txt(topBar.transform, "RadT", "辐射 安全", 12);
        SetRt(radT, new Vector2(0.7f, 0.82f), new Vector2(0.7f, 0.82f), new Vector2(140, 18));
        radT.alignment = TextAnchor.MiddleCenter; radT.color = Color.white;

        var depthT = Txt(topBar.transform, "DepthT", "地表", 12);
        SetRt(depthT, new Vector2(0.9f, 0.5f), new Vector2(0.9f, 0.5f), new Vector2(80, 20));
        depthT.alignment = TextAnchor.MiddleCenter; depthT.color = new Color(0.6f, 0.8f, 0.6f);

        // 底部操作栏
        var botBar = Img(canvas.transform, "BottomBar", "panel_bg");
        var br = botBar.GetComponent<RectTransform>();
        br.anchorMin = new Vector2(0, 0); br.anchorMax = new Vector2(1, 0.15f);
        br.sizeDelta = Vector2.zero;
        botBar.color = new Color(0.03f, 0.03f, 0.06f, 0.85f);

        // D-Pad 方向键
        var dpadGo = new GameObject("Dpad");
        dpadGo.transform.SetParent(canvas.transform, false);
        var dpadRt = dpadGo.AddComponent<RectTransform>();
        dpadRt.anchorMin = new Vector2(0.02f, 0.05f);
        dpadRt.anchorMax = new Vector2(0.35f, 0.95f);
        dpadRt.sizeDelta = Vector2.zero;
        dpadGo.AddComponent<DpadController>();

        var dpad = dpadGo.AddComponent<DpadController>();

        // 上
        dpad.btnUp = MakeDPadBtn(dpadGo.transform, "Up", new Vector2(0.5f, 0.75f), new Vector2(50, 30));
        // 下
        dpad.btnDown = MakeDPadBtn(dpadGo.transform, "Down", new Vector2(0.5f, 0.25f), new Vector2(50, 30));
        // 左
        dpad.btnLeft = MakeDPadBtn(dpadGo.transform, "Left", new Vector2(0.2f, 0.5f), new Vector2(30, 50));
        // 右
        dpad.btnRight = MakeDPadBtn(dpadGo.transform, "Right", new Vector2(0.8f, 0.5f), new Vector2(30, 50));

        // 右侧操作按钮
        float[] bx = { 0.74f, 0.82f, 0.90f, 0.98f };
        string[] bn = { "采集", "吃", "喝", "挖" };
        var gatherBtn = Btn(botBar.transform, "GatherBtn", "采集", new Vector2(0.74f, 0.5f));
        SetRt(gatherBtn, new Vector2(0.74f, 0.5f), new Vector2(0.74f, 0.5f), new Vector2(55, 38));
        var eatBtn = Btn(botBar.transform, "EatBtn", "吃", new Vector2(0.82f, 0.5f));
        SetRt(eatBtn, new Vector2(0.82f, 0.5f), new Vector2(0.82f, 0.5f), new Vector2(45, 38));
        var drinkBtn = Btn(botBar.transform, "DrinkBtn", "喝", new Vector2(0.90f, 0.5f));
        SetRt(drinkBtn, new Vector2(0.90f, 0.5f), new Vector2(0.90f, 0.5f), new Vector2(45, 38));
        var digBtn = Btn(botBar.transform, "DigBtn", "挖", new Vector2(0.98f, 0.5f));
        SetRt(digBtn, new Vector2(0.98f, 0.5f), new Vector2(0.98f, 0.5f), new Vector2(45, 38));

        // 资源文字
        var resT = Txt(botBar.transform, "ResT", "木:0 石:0 食:0 水:0", 11);
        SetRt(resT, new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f), new Vector2(400, 18));
        resT.alignment = TextAnchor.MiddleCenter; resT.color = new Color(0.6f, 0.6f, 0.6f);

        // 挂载 GameUI
        var gui = canvas.gameObject.AddComponent<GameUI>();
        SetF(gui, "hpSlider", hpBar.GetComponent<Slider>());
        SetF(gui, "hungerSlider", huBar.GetComponent<Slider>());
        SetF(gui, "thirstSlider", thBar.GetComponent<Slider>());
        SetF(gui, "radiationSlider", radBar.GetComponent<Slider>());
        SetF(gui, "hpText", hpT);
        SetF(gui, "hungerText", huT);
        SetF(gui, "thirstText", thT);
        SetF(gui, "radiationText", radT);
        SetF(gui, "depthText", depthT);
        SetF(gui, "woodText", resT);
        SetF(gui, "gatherBtn", gatherBtn.GetComponent<Button>());
        SetF(gui, "eatBtn", eatBtn.GetComponent<Button>());
        SetF(gui, "drinkBtn", drinkBtn.GetComponent<Button>());
        SetF(gui, "digBtn", digBtn.GetComponent<Button>());
    }

    static Button MakeDPadBtn(Transform parent, string name, Vector2 anchor, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.3f, 0.7f);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;
        return go.AddComponent<Button>();
    }

    // ============ 工具方法 ============
    static Canvas CreateCanvas(string name)
    {
        var go = new GameObject(name);
        var c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        var s = go.AddComponent<CanvasScaler>();
        s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution = new Vector2(800, 480);
        s.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return c;
    }

    static Text Txt(Transform p, string n, string t, int sz)
    {
        var go = new GameObject(n); go.transform.SetParent(p, false);
        var tx = go.AddComponent<Text>();
        tx.text = t; tx.fontSize = sz;
        tx.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tx.color = Color.white; tx.raycastTarget = false;
        return tx;
    }

    static Image Img(Transform p, string n, string spr)
    {
        var go = new GameObject(n); go.transform.SetParent(p, false);
        var img = go.AddComponent<Image>();
        var s = LoadSprite(spr);
        if (s != null) img.sprite = s;
        img.type = Image.Type.Sliced;
        return img;
    }

    static GameObject Btn(Transform p, string n, string label, Vector2 pos)
    {
        var go = new GameObject(n); go.transform.SetParent(p, false);
        var img = go.AddComponent<Image>();
        var s = LoadSprite("btn_normal");
        if (s != null) img.sprite = s;
        img.type = Image.Type.Sliced;
        img.color = new Color(0.1f, 0.2f, 0.1f);
        var btn = go.AddComponent<Button>();
        var lb = new GameObject("Label"); lb.transform.SetParent(go.transform, false);
        var lt = lb.AddComponent<Text>();
        lt.text = label; lt.fontSize = 18;
        lt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        lt.color = new Color(0.8f, 0.95f, 0.8f);
        lt.alignment = TextAnchor.MiddleCenter;
        lt.fontStyle = FontStyle.Bold;
        lt.raycastTarget = false;
        var lrt = lt.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.sizeDelta = Vector2.zero;
        return go;
    }

    static InputField Inp(Transform p, string n, string ph)
    {
        var go = new GameObject(n); go.transform.SetParent(p, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.18f);
        var inp = go.AddComponent<InputField>();
        var phGo = new GameObject("Placeholder"); phGo.transform.SetParent(go.transform, false);
        var pht = phGo.AddComponent<Text>();
        pht.text = ph; pht.fontSize = 15;
        pht.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        pht.color = new Color(0.4f, 0.4f, 0.5f);
        pht.alignment = TextAnchor.MiddleLeft; pht.raycastTarget = false;
        var pr = pht.GetComponent<RectTransform>();
        pr.anchorMin = Vector2.zero; pr.anchorMax = Vector2.one;
        pr.sizeDelta = Vector2.zero; pr.offsetMin = new Vector2(8, 0); pr.offsetMax = new Vector2(-8, 0);
        inp.placeholder = pht;
        var tGo = new GameObject("Text"); tGo.transform.SetParent(go.transform, false);
        var tx = tGo.AddComponent<Text>();
        tx.fontSize = 15; tx.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tx.color = Color.white; tx.alignment = TextAnchor.MiddleLeft;
        tx.raycastTarget = false; tx.supportRichText = false;
        var tr = tx.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.sizeDelta = Vector2.zero; tr.offsetMin = new Vector2(8, 0); tr.offsetMax = new Vector2(-8, 0);
        inp.textComponent = tx;
        return inp;
    }

    static Slider Sld(Transform p, string n, Color fill)
    {
        var go = new GameObject(n); go.transform.SetParent(p, false);
        var bgGo = new GameObject("Background"); bgGo.transform.SetParent(go.transform, false);
        var bg = bgGo.AddComponent<Image>(); bg.color = new Color(0.08f, 0.08f, 0.08f);
        var brt = bgGo.GetComponent<RectTransform>();
        brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one; brt.sizeDelta = Vector2.zero;
        var fa = new GameObject("Fill Area"); fa.transform.SetParent(go.transform, false);
        var far = fa.AddComponent<RectTransform>();
        far.anchorMin = Vector2.zero; far.anchorMax = Vector2.one;
        far.sizeDelta = new Vector2(-4, -4); far.anchoredPosition = new Vector2(2, 0);
        var fg = new GameObject("Fill"); fg.transform.SetParent(fa.transform, false);
        var fi = fg.AddComponent<Image>(); fi.color = fill;
        var fr = fg.GetComponent<RectTransform>();
        fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one; fr.sizeDelta = Vector2.zero;
        var sl = go.AddComponent<Slider>();
        sl.fillRect = fr; sl.targetGraphic = fi;
        sl.interactable = false; sl.minValue = 0; sl.maxValue = 1; sl.value = 1;
        return sl;
    }

    static void SetRt(GameObject go, Vector2 amin, Vector2 amax, Vector2 size)
    {
        var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        rt.anchorMin = amin; rt.anchorMax = amax; rt.sizeDelta = size; rt.anchoredPosition = Vector2.zero;
    }
    static void SetRt(Text t, Vector2 a, Vector2 b, Vector2 s) => SetRt(t.gameObject, a, b, s);
    static void SetRt(Image i, Vector2 a, Vector2 b, Vector2 s) => SetRt(i.gameObject, a, b, s);
    static void SetRt(InputField i, Vector2 a, Vector2 b, Vector2 s) => SetRt(i.gameObject, a, b, s);
    static void SetRt(Slider sl, Vector2 a, Vector2 b, Vector2 s) => SetRt(sl.gameObject, a, b, s);

    static Sprite LoadSprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/{name}.png");

    static void SetF(object obj, string field, object val)
    {
        var f = obj.GetType().GetField(field,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        if (f != null) f.SetValue(obj, val);
        else Debug.LogWarning($"Field '{field}' not found on {obj.GetType().Name}");
    }

    static void SaveScene(string name)
    {
        string path = $"Assets/Scenes/{name}.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), path);
        Debug.Log($"Scene saved: {path}");
    }
}
#endif