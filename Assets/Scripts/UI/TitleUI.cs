using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleUI : MonoBehaviour
{
    public Button startButton;
    public Button charaCreateButton;
    public BackgroundSettings backgroundSettings;

    [Header("Audio")]
    public AudioClip titleBGM;
    [Range(0f, 1f)] public float bgmVolume = 0.7f;

    Text aiToggleLabel;
    Text bgmToggleLabel;
    AudioSource bgmSource;

    void Update()
    {
        if (bgmSource != null && GameSettings.EnableBGM && !bgmSource.isPlaying)
            bgmSource.Play();
    }

    void Awake()
    {
        foreach (var tmp in FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Exclude))
        {
            if (tmp.GetComponentInParent<Button>() != null) continue;
            tmp.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        PortraitGuard.EnsureExists();

        if (backgroundSettings == null)
            backgroundSettings = Resources.Load<BackgroundSettings>("BackgroundSettings");

        PlayBGM();
        CreateTitleBackground();
        StyleSceneTitleText();

        if (startButton == null || charaCreateButton == null)
            BuildUI();
        else
            CreateToggles(startButton.transform.parent);

        if (startButton       != null) startButton.onClick.AddListener(SceneTransition.GoToBattle);
        if (charaCreateButton != null) charaCreateButton.onClick.AddListener(SceneTransition.GoToCharacterSelect);
    }

    void BuildUI()
    {
        var canvasGo = new GameObject("TitleCanvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        CreateTitle(canvas.transform);
        startButton       = CreateButton(canvas.transform, "GAME START",       20f);
        charaCreateButton = CreateButton(canvas.transform, "CHARACTER CREATE", -60f);
        CreateToggles(canvas.transform);
    }

    static void StyleSceneTitleText()
    {
        var canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        var band = new GameObject("SceneTitleBand");
        band.transform.SetParent(canvas.transform, false);
        var bandRt = band.AddComponent<RectTransform>();
        bandRt.anchorMin = bandRt.anchorMax = bandRt.pivot = new Vector2(0.5f, 0.5f);
        bandRt.anchoredPosition = new Vector2(0f, 160f);
        bandRt.sizeDelta = new Vector2(680f, 90f);
        band.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

        var titleGo = new GameObject("TitleJP");
        titleGo.transform.SetParent(canvas.transform, false);
        var titleRt = titleGo.AddComponent<RectTransform>();
        titleRt.anchorMin = titleRt.anchorMax = titleRt.pivot = new Vector2(0.5f, 0.5f);
        titleRt.anchoredPosition = new Vector2(0f, 160f);
        titleRt.sizeDelta = new Vector2(680f, 90f);
        var txt = titleGo.AddComponent<Text>();
        txt.text      = "ふらふら頭突き対戦ゲーム";
        txt.font      = Resources.Load<Font>("NotoSansJP-Regular") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize  = 36;
        txt.color     = Color.yellow;
        txt.alignment = TextAnchor.MiddleCenter;
    }

    void CreateTitleBackground()
    {
        var cam = Camera.main;
        if (cam == null) return;

        float camH = cam.orthographicSize * 2f;
        float camW = camH * cam.aspect;

        var sprite = backgroundSettings != null ? backgroundSettings.GetByIndex(CharacterSaveData.LoadBg()) : null;
        if (sprite != null)
        {
            var go = new GameObject("TitleBackground");
            go.transform.position = new Vector3(0f, 0f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -10;
            var bounds = sprite.bounds;
            go.transform.localScale = new Vector3(camW / bounds.size.x, camH / bounds.size.y, 1f);
            go.AddComponent<DynamicBackground>();
        }

        var overlay = new GameObject("BgOverlay");
        overlay.transform.position = new Vector3(0f, 0f, 0.9f);
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        var overlaySr = overlay.AddComponent<SpriteRenderer>();
        overlaySr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        overlaySr.color = new Color(0f, 0f, 0f, 0.65f);
        overlaySr.sortingOrder = -9;
        overlay.transform.localScale = new Vector3(camW + 2f, camH + 2f, 1f);
        overlay.AddComponent<DynamicBackground>().padding = 2f;
    }

    static void CreateTitle(Transform parent)
    {
        var band = new GameObject("TitleBand");
        band.transform.SetParent(parent, false);
        var bandRt = band.AddComponent<RectTransform>();
        bandRt.anchorMin = bandRt.anchorMax = bandRt.pivot = new Vector2(0.5f, 0.5f);
        bandRt.anchoredPosition = new Vector2(0f, 140f);
        bandRt.sizeDelta = new Vector2(720f, 110f);
        band.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

        var go = new GameObject("TitleText");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, 140f);
        rt.sizeDelta = new Vector2(700f, 100f);
        var txt = go.AddComponent<Text>();
        txt.text      = "ふらふら頭突き対戦ゲーム";
        txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize  = 48;
        txt.color     = Color.yellow;
        txt.alignment = TextAnchor.MiddleCenter;
    }

    void CreateToggles(Transform parent)
    {
        CreateAIToggle(parent);
        CreateBGMToggle(parent);
    }

    void CreateAIToggle(Transform parent)
    {
        var go = new GameObject("AIToggle");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, -148f);
        rt.sizeDelta = new Vector2(220f, 50f);
        go.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        var btn = go.AddComponent<Button>();

        var textGo = new GameObject("Label");
        textGo.transform.SetParent(go.transform, false);
        var tRt = textGo.AddComponent<RectTransform>();
        tRt.anchorMin = Vector2.zero;
        tRt.anchorMax = Vector2.one;
        tRt.sizeDelta = Vector2.zero;
        aiToggleLabel = textGo.AddComponent<Text>();
        aiToggleLabel.font      = Resources.Load<Font>("NotoSansJP-Regular") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        aiToggleLabel.fontSize  = 22;
        aiToggleLabel.color     = Color.white;
        aiToggleLabel.alignment = TextAnchor.MiddleCenter;

        RefreshAILabel();
        btn.onClick.AddListener(() =>
        {
            GameSettings.EnableAI = !GameSettings.EnableAI;
            RefreshAILabel();
        });
    }

    void CreateBGMToggle(Transform parent)
    {
        var go = new GameObject("BGMToggle");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, -210f);
        rt.sizeDelta = new Vector2(220f, 50f);
        go.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        var btn = go.AddComponent<Button>();

        var textGo = new GameObject("Label");
        textGo.transform.SetParent(go.transform, false);
        var tRt = textGo.AddComponent<RectTransform>();
        tRt.anchorMin = Vector2.zero;
        tRt.anchorMax = Vector2.one;
        tRt.sizeDelta = Vector2.zero;
        bgmToggleLabel = textGo.AddComponent<Text>();
        bgmToggleLabel.font      = Resources.Load<Font>("NotoSansJP-Regular") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        bgmToggleLabel.fontSize  = 22;
        bgmToggleLabel.color     = Color.white;
        bgmToggleLabel.alignment = TextAnchor.MiddleCenter;

        RefreshBGMLabel();
        btn.onClick.AddListener(() =>
        {
            GameSettings.EnableBGM = !GameSettings.EnableBGM;
            RefreshBGMLabel();
            if (bgmSource != null)
            {
                if (GameSettings.EnableBGM) bgmSource.Play();
                else bgmSource.Stop();
            }
        });
    }

    void RefreshAILabel()
    {
        if (aiToggleLabel != null)
            aiToggleLabel.text = GameSettings.EnableAI ? "CPU攻撃 : ON" : "CPU攻撃 : OFF";
    }

    void RefreshBGMLabel()
    {
        if (bgmToggleLabel != null)
            bgmToggleLabel.text = GameSettings.EnableBGM ? "BGM : ON" : "BGM : OFF";
    }

    static Button CreateButton(Transform parent, string label, float yOffset)
    {
        var go = new GameObject(label);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, yOffset);
        rt.sizeDelta = new Vector2(280f, 60f);
        go.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.92f);
        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f);
        btn.colors = colors;

        var textGo = new GameObject("Label");
        textGo.transform.SetParent(go.transform, false);
        var tRt = textGo.AddComponent<RectTransform>();
        tRt.anchorMin = Vector2.zero;
        tRt.anchorMax = Vector2.one;
        tRt.sizeDelta = Vector2.zero;
        var txt = textGo.AddComponent<Text>();
        txt.text      = label;
        txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize  = 24;
        txt.color     = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;

        return btn;
    }

    void PlayBGM()
    {
        if (titleBGM == null) return;
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.clip      = titleBGM;
        bgmSource.loop      = true;
        bgmSource.volume    = bgmVolume;
        bgmSource.playOnAwake = false;
        if (GameSettings.EnableBGM) bgmSource.Play();
    }
}
