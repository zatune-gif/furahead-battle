using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("References")]
    public HUDController hud;
    public HitEffectController hitEffect;
    public ResultOverlay resultOverlay;

    [Header("Part Arrays")]
    public PartData[] headParts;
    public PartData[] bodyParts;
    public PartData[] armLParts;
    public PartData[] armRParts;
    public PartData[] legLParts;
    public PartData[] legRParts;

    [Header("Backgrounds")]
    public BackgroundSettings backgroundSettings;

    [Header("AI")]
    public AIDifficultyData aiDifficulty;

    [Header("Audio BGM")]
    public AudioClip battleBGM;
    public AudioClip winBGM;
    public AudioClip loseBGM;
    [Range(0f, 1f)] public float bgmVolume = 0.7f;

    [Header("Audio SE")]
    public AudioClip headbuttSE;
    public AudioClip jumpSE;
    public AudioClip hitSE;

    AudioSource bgmSource;
    bool bgmIntent;

    FighterController playerCtrl;
    FighterController cpuCtrl;
    AIController aiCtrl;
    PlayerInputController inputCtrl;

    void Awake()
    {
        Instance = this;
        PortraitGuard.EnsureExists();
        if (hud      == null) hud      = FindAnyObjectByType<HUDController>();
        if (hitEffect == null) hitEffect = FindAnyObjectByType<HitEffectController>();
        if (resultOverlay == null)
            resultOverlay = FindAnyObjectByType<ResultOverlay>(FindObjectsInactive.Include);
        if (resultOverlay == null)
        {
            var go = new GameObject("ResultOverlay");
            resultOverlay = go.AddComponent<ResultOverlay>();
        }
        resultOverlay.winBGM    = winBGM;
        resultOverlay.loseBGM   = loseBGM;
        resultOverlay.bgmVolume = bgmVolume;
        if (hitEffect != null) hitEffect.hitSE = hitSE;
        if (backgroundSettings == null)
            backgroundSettings = Resources.Load<BackgroundSettings>("BackgroundSettings");
    }

    void Start() => StartCoroutine(InitBattle());

    IEnumerator InitBattle()
    {
        PlayBGM();
        CreateBackground();
        CreateBoundaries();
        int[] saved = CharacterSaveData.Load();

        PartData[] playerParts = BuildPartArray(saved);
        PartData[] cpuParts = BuildDefaultParts();

        var playerBody = CharacterBody.Build(playerParts, new Vector3(-3f, 0f, 0f), Color.white);
        var cpuBody    = CharacterBody.Build(cpuParts,    new Vector3( 3f, 0f, 0f), new Color(1f, 0.7f, 0.7f));

        playerCtrl = playerBody.gameObject.AddComponent<FighterController>();
        playerCtrl.headbuttSE = headbuttSE;
        playerCtrl.jumpSE     = jumpSE;
        playerCtrl.Init(playerBody.body.GetComponent<Rigidbody2D>(), playerBody.head.GetComponent<Rigidbody2D>());
        playerCtrl.SetFacing(1);

        cpuCtrl = cpuBody.gameObject.AddComponent<FighterController>();
        cpuCtrl.headbuttSE = headbuttSE;
        cpuCtrl.jumpSE     = jumpSE;
        cpuCtrl.Init(cpuBody.body.GetComponent<Rigidbody2D>(), cpuBody.head.GetComponent<Rigidbody2D>());
        cpuCtrl.SetFacing(-1);

        inputCtrl = playerBody.gameObject.AddComponent<PlayerInputController>();
        inputCtrl.Init(playerCtrl);
        inputCtrl.enabled = false;

        aiCtrl = cpuBody.gameObject.AddComponent<AIController>();
        aiCtrl.difficulty = aiDifficulty;
        aiCtrl.Init(cpuCtrl, playerCtrl);
        aiCtrl.enabled = false;

        var detector = playerBody.head.AddComponent<HeadbuttHitDetector>();
        detector.Init(playerCtrl, cpuCtrl);
        var playerHead = playerBody.head.transform;
        detector.OnHit.AddListener(() => OnHit(playerHead.position, isPlayerAttack: true));

        var detectorCPU = cpuBody.head.AddComponent<HeadbuttHitDetector>();
        detectorCPU.Init(cpuCtrl, playerCtrl);
        var cpuHead = cpuBody.head.transform;
        detectorCPU.OnHit.AddListener(() => OnHit(cpuHead.position, isPlayerAttack: false));

        HideFloor();
        CreateGroundShadow(playerBody.body.transform);
        CreateGroundShadow(cpuBody.body.transform);

        CreateControlsUI();

        yield return new WaitForSeconds(0.5f);
        yield return hud.ShowFightAnnouncement();

        inputCtrl.enabled = true;
        if (GameSettings.EnableAI) aiCtrl.enabled = true;
    }

    void CreateControlsUI()
    {
        if (WebGLBridge.IsMobile())
            CreateMobileTouchUI();
        else
            CreateKeyboardHintsUI();
    }

    void CreateKeyboardHintsUI()
    {
        var canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        var panel = new GameObject("ControlsPanel");
        panel.transform.SetParent(canvas.transform, false);
        var panelRt = panel.AddComponent<RectTransform>();
        panelRt.anchorMin = panelRt.anchorMax = panelRt.pivot = new Vector2(0f, 1f);
        panelRt.anchoredPosition = new Vector2(8, -8);
        panelRt.sizeDelta = new Vector2(210, 90);
        panel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

        var go = new GameObject("ControlsText");
        go.transform.SetParent(panel.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(8, 4);
        rt.offsetMax = new Vector2(-8, -4);
        var txt = go.AddComponent<Text>();
        txt.text     = $"移動     : ← / →\nジャンプ : ↑\n頭突き   : Space\n{CombatSystem.HitsToDown}回当てると勝利！";
        txt.fontSize = 15;
        txt.color    = Color.white;
        txt.font     = Resources.Load<Font>("NotoSansJP-Regular")
                       ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    void CreateMobileTouchUI()
    {
        WebGLBridge.LockLandscape();

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        var canvasGo = new GameObject("TouchCanvas");
        var canvas   = canvasGo.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        var t = canvasGo.transform;
        MobileTouchUI.Create(t, "←", MobileTouchUI.ButtonType.Left,
            new Vector2(0f, 0f), new Vector2(100f, 100f), inputCtrl);
        MobileTouchUI.Create(t, "→", MobileTouchUI.ButtonType.Right,
            new Vector2(0f, 0f), new Vector2(300f, 100f), inputCtrl);
        MobileTouchUI.Create(t, "↑", MobileTouchUI.ButtonType.Jump,
            new Vector2(1f, 0f), new Vector2(-300f, 100f), inputCtrl);
        MobileTouchUI.Create(t, "攻", MobileTouchUI.ButtonType.Headbutt,
            new Vector2(1f, 0f), new Vector2(-100f, 100f), inputCtrl);
    }

    void OnHit(Vector3 position, bool isPlayerAttack)
    {
        if (hitEffect != null) hitEffect.PlayHitEffect(position, isPlayerAttack);
        CheckRoundEnd();
    }

    void CheckRoundEnd()
    {
        if (!playerCtrl.IsAlive)  EndRound(false);
        else if (!cpuCtrl.IsAlive) EndRound(true);
    }

    void EndRound(bool playerWon)
    {
        if (inputCtrl != null) inputCtrl.enabled = false;
        if (aiCtrl != null) aiCtrl.enabled = false;
        StopBGM();
        StartCoroutine(ShowResult(playerWon));
    }

    void Update()
    {
        if (bgmIntent && bgmSource != null && !bgmSource.isPlaying)
            bgmSource.Play();
    }

    void PlayBGM()
    {
        if (battleBGM == null) return;
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.clip        = battleBGM;
        bgmSource.loop        = true;
        bgmSource.volume      = bgmVolume;
        bgmSource.playOnAwake = false;
        bgmSource.Play();
        bgmIntent = true;
    }

    void StopBGM()
    {
        bgmIntent = false;
        if (bgmSource != null && bgmSource.isPlaying)
            bgmSource.Stop();
    }

    IEnumerator ShowResult(bool playerWon)
    {
        yield return new WaitForSeconds(0.5f);
        resultOverlay.Show(playerWon);
    }

    void CreateBoundaries()
    {
        var cam = Camera.main;
        if (cam == null) return;
        float h = cam.orthographicSize;
        float w = h * cam.aspect;
        CreateWall(-(w + 0.5f), h * 3f);
        CreateWall(  w + 0.5f,  h * 3f);
    }

    void CreateBackground()
    {
        var cam = Camera.main;
        if (cam == null) return;

        float camH = cam.orthographicSize * 2f;
        float camW = camH * cam.aspect;

        int bgIdx = CharacterSaveData.LoadBg();
        var sprite = backgroundSettings != null ? backgroundSettings.GetByIndex(bgIdx) : null;
        if (sprite != null)
        {
            var go = new GameObject("Background");
            go.transform.position = new Vector3(0f, 0f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -10;
            var bounds = sprite.bounds;
            go.transform.localScale = new Vector3(camW / bounds.size.x, camH / bounds.size.y, 1f);
            go.AddComponent<DynamicBackground>();
        }

        // 背景を少し暗くするオーバーレイ
        var overlay = new GameObject("BgOverlay");
        overlay.transform.position = new Vector3(0f, 0f, 0.9f);
        var overlaySprite = overlay.AddComponent<SpriteRenderer>();
        overlaySprite.sprite = GetWhitePixelSprite();
        overlaySprite.color = new Color(0f, 0f, 0f, 0.55f);
        overlaySprite.sortingOrder = -9;
        overlay.transform.localScale = new Vector3(camW + 2f, camH + 2f, 1f);
        overlay.AddComponent<DynamicBackground>().padding = 2f;
    }

    static Sprite GetWhitePixelSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    static void HideFloor()
    {
        var floor = GameObject.Find("Floor");
        if (floor == null) return;
        foreach (var sr in floor.GetComponentsInChildren<SpriteRenderer>())
            sr.enabled = false;
        foreach (var img in floor.GetComponentsInChildren<UnityEngine.UI.Image>())
            img.enabled = false;
    }

    static void CreateGroundShadow(Transform bodyTransform)
    {
        float groundY = -3.75f;
        var floor = GameObject.Find("Floor");
        if (floor != null)
        {
            var col = floor.GetComponentInChildren<BoxCollider2D>();
            if (col != null) groundY = col.bounds.max.y + 0.02f;
        }

        var go = new GameObject("GroundShadow");
        go.transform.position = new Vector3(bodyTransform.position.x, groundY, 0.5f);

        var tex = new Texture2D(32, 16);
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float dx = (x - 16f) / 16f;
                float dy = (y - 8f) / 8f;
                float dist = dx * dx + dy * dy;
                float alpha = dist < 1f ? Mathf.Lerp(0.5f, 0f, dist) : 0f;
                tex.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
            }
        }
        tex.Apply();

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 16), new Vector2(0.5f, 0.5f));
        sr.sortingOrder = -8;
        go.transform.localScale = new Vector3(3f, 1.2f, 1f);

        float stanceHeight = bodyTransform.position.y - groundY;
        go.AddComponent<GroundShadow>().Init(bodyTransform, stanceHeight);
    }

    void CreateWall(float x, float height)
    {
        var go = new GameObject("Boundary");
        go.transform.position = new Vector3(x, 0f, 0f);
        var box = go.AddComponent<BoxCollider2D>();
        box.size = new Vector2(1f, height * 2f);
        box.sharedMaterial = new PhysicsMaterial2D { friction = 0f, bounciness = 0f };
    }

    PartData[] BuildPartArray(int[] indices) => new PartData[]
    {
        headParts[Mathf.Clamp(indices[0], 0, headParts.Length - 1)],
        bodyParts[Mathf.Clamp(indices[1], 0, bodyParts.Length - 1)],
        armLParts[Mathf.Clamp(indices[2], 0, armLParts.Length - 1)],
        armRParts[Mathf.Clamp(indices[3], 0, armRParts.Length - 1)],
        legLParts[Mathf.Clamp(indices[4], 0, legLParts.Length - 1)],
        legRParts[Mathf.Clamp(indices[5], 0, legRParts.Length - 1)],
    };

    PartData[] BuildDefaultParts() => new PartData[]
    {
        headParts[Random.Range(0, headParts.Length)],
        bodyParts[Random.Range(0, bodyParts.Length)],
        armLParts[Random.Range(0, armLParts.Length)],
        armRParts[Random.Range(0, armRParts.Length)],
        legLParts[Random.Range(0, legLParts.Length)],
        legRParts[Random.Range(0, legRParts.Length)],
    };
}
