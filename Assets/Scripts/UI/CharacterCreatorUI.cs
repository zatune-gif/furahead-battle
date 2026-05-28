using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterCreatorUI : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip selectBGM;
    [Range(0f, 1f)] public float bgmVolume = 0.7f;

    [Header("Part Lists (auto-loaded if empty)")]
    public PartData[] headParts;
    public PartData[] bodyParts;
    public PartData[] armLParts;
    public PartData[] armRParts;
    public PartData[] legLParts;
    public PartData[] legRParts;
    public BackgroundSettings backgroundSettings;

    int[] indices = new int[6];
    int bgIndex;
    SpriteRenderer[] previews = new SpriteRenderer[6];
    SpriteRenderer bgSr;
    Text[] partLabels = new Text[6];
    Text bgLabel;
    AudioSource bgmSource;

    static readonly string[] SlotNames = { "HEAD", "BODY", "ARM L", "ARM R", "LEG L", "LEG R" };

    static readonly Vector2[] PreviewOffsets =
    {
        new Vector2( 0f,    1.2f),
        new Vector2( 0f,    0f),
        new Vector2(-0.55f, -0.1f),
        new Vector2( 0.55f, -0.1f),
        new Vector2(-0.25f,-0.7f),
        new Vector2( 0.25f,-0.7f),
    };

    PartData[][] AllParts => new[] { headParts, bodyParts, armLParts, armRParts, legLParts, legRParts };

    void Start()
    {
        PortraitGuard.EnsureExists();

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();
        }

        foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
            canvas.gameObject.SetActive(false);

        PlayBGM();
        AutoLoadParts();

        indices = CharacterSaveData.Load();
        bgIndex = CharacterSaveData.LoadBg();

        CreateBackground();
        CreatePreviewCharacter();
        CreateSelectorUI();

        UpdateAllPreviews();
        UpdateBGLabel();
    }

    void AutoLoadParts()
    {
        var allParts = Resources.LoadAll<PartData>("Parts");
        headParts  = FilterParts(allParts, "Head");
        bodyParts  = FilterParts(allParts, "Body");
        armLParts  = FilterParts(allParts, "Arm");
        armRParts  = FilterParts(allParts, "ArmR");
        legLParts  = FilterParts(allParts, "Leg");
        legRParts  = FilterParts(allParts, "LegR");
        if (backgroundSettings == null)
            backgroundSettings = Resources.Load<BackgroundSettings>("BackgroundSettings");
    }

    static PartData[] FilterParts(PartData[] all, string prefix)
    {
        var list = new List<PartData>();
        foreach (var p in all)
            if (p.name.StartsWith(prefix + "_"))
                list.Add(p);
        list.Sort((a, b) => string.Compare(a.name, b.name));
        return list.ToArray();
    }

    void CreateBackground()
    {
        var sprite = backgroundSettings?.GetByIndex(bgIndex);
        if (sprite == null) return;
        var cam = Camera.main;
        if (cam == null) return;
        var go = new GameObject("Background");
        go.transform.position = new Vector3(0f, 0f, 1f);
        bgSr = go.AddComponent<SpriteRenderer>();
        bgSr.sprite = sprite;
        bgSr.sortingOrder = -10;
        float camH = cam.orthographicSize * 2f;
        float camW = camH * cam.aspect;
        go.transform.localScale = new Vector3(camW / sprite.bounds.size.x, camH / sprite.bounds.size.y, 1f);
        go.AddComponent<DynamicBackground>();
    }

    void CreatePreviewCharacter()
    {
        var cam = Camera.main;
        float cx = cam != null ? -cam.orthographicSize * cam.aspect * 0.38f : -3f;

        for (int i = 0; i < 6; i++)
        {
            var go = new GameObject($"Preview_{SlotNames[i]}");
            go.transform.position = new Vector3(cx + PreviewOffsets[i].x, PreviewOffsets[i].y, 0f);
            go.transform.localScale = Vector3.one * 1.4f;
            previews[i] = go.AddComponent<SpriteRenderer>();
            previews[i].sortingOrder = i < 2 ? 4 - i : i < 4 ? 2 : 1;
            if (i == 2 || i == 4) previews[i].flipX = true;
        }
    }

    void CreateSelectorUI()
    {
        var canvasGo = new GameObject("SelectorCanvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        // 右半分パネル (8行: 6パーツ + BG + ボタン)
        var panel = MakeRect("Panel", canvas.transform);
        SetAnchors(panel, 0.50f, 0.03f, 0.99f, 0.97f);
        panel.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

        float rowH = 1f / 8f;
        for (int i = 0; i < 6; i++)
        {
            float yMax = 1f - rowH * i;
            CreatePartRow(panel, i, yMax - rowH + 0.005f, yMax - 0.005f);
        }
        {
            float yMax = 1f - rowH * 6;
            CreateBGRow(panel, yMax - rowH + 0.005f, yMax - 0.005f);
        }
        CreateConfirmButton(panel, 0.005f, rowH - 0.005f);

        // タイトルバー
        var title = MakeRect("Title", canvas.transform);
        SetAnchors(title, 0.01f, 0.88f, 0.49f, 0.97f);
        title.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);
        var titleLabel = MakeRect("TitleText", title.transform);
        SetAnchors(titleLabel, 0f, 0f, 1f, 1f);
        var titleTxt = titleLabel.gameObject.AddComponent<Text>();
        titleTxt.text      = "CHARACTER CREATE";
        titleTxt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleTxt.fontSize  = 24;
        titleTxt.color     = Color.yellow;
        titleTxt.alignment = TextAnchor.MiddleCenter;
    }

    void CreatePartRow(RectTransform parent, int slot, float yMin, float yMax)
    {
        var row = MakeRect($"Row{slot}", parent.transform);
        SetAnchors(row, 0.01f, yMin, 0.99f, yMax);

        var lbl = MakeRect("Lbl", row.transform);
        SetAnchors(lbl, 0f, 0f, 0.32f, 1f);
        var ltxt = lbl.gameObject.AddComponent<Text>();
        ltxt.text = SlotNames[slot]; ltxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        ltxt.fontSize = 16; ltxt.color = Color.white; ltxt.alignment = TextAnchor.MiddleCenter;

        int s = slot;
        CreateBtn(row.transform, "<", 0.32f, 0f, 0.47f, 1f, () => ChangeIndex(s, -1));

        var numRt = MakeRect("Num", row.transform);
        SetAnchors(numRt, 0.47f, 0f, 0.68f, 1f);
        partLabels[slot] = numRt.gameObject.AddComponent<Text>();
        partLabels[slot].font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        partLabels[slot].fontSize = 15; partLabels[slot].color = Color.white;
        partLabels[slot].alignment = TextAnchor.MiddleCenter;

        CreateBtn(row.transform, ">", 0.68f, 0f, 0.83f, 1f, () => ChangeIndex(s, 1));
    }

    void CreateBGRow(RectTransform parent, float yMin, float yMax)
    {
        var row = MakeRect("BGRow", parent.transform);
        SetAnchors(row, 0.01f, yMin, 0.99f, yMax);

        var lbl = MakeRect("Lbl", row.transform);
        SetAnchors(lbl, 0f, 0f, 0.32f, 1f);
        var ltxt = lbl.gameObject.AddComponent<Text>();
        ltxt.text = "BG"; ltxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        ltxt.fontSize = 16; ltxt.color = Color.cyan; ltxt.alignment = TextAnchor.MiddleCenter;

        CreateBtn(row.transform, "<", 0.32f, 0f, 0.47f, 1f, () => ChangeBGIndex(-1));

        var numRt = MakeRect("Num", row.transform);
        SetAnchors(numRt, 0.47f, 0f, 0.68f, 1f);
        bgLabel = numRt.gameObject.AddComponent<Text>();
        bgLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        bgLabel.fontSize = 15; bgLabel.color = Color.white;
        bgLabel.alignment = TextAnchor.MiddleCenter;

        CreateBtn(row.transform, ">", 0.68f, 0f, 0.83f, 1f, () => ChangeBGIndex(1));
    }

    void CreateConfirmButton(RectTransform parent, float yMin, float yMax)
    {
        var rt = MakeRect("Start", parent.transform);
        SetAnchors(rt, 0.05f, yMin, 0.95f, yMax);
        rt.gameObject.AddComponent<Image>().color = new Color(0.1f, 0.55f, 0.1f);
        rt.gameObject.AddComponent<Button>().onClick.AddListener(OnConfirm);
        var labelRt = MakeRect("L", rt.transform);
        SetAnchors(labelRt, 0f, 0f, 1f, 1f);
        var txt = labelRt.gameObject.AddComponent<Text>();
        txt.text = "BATTLE START"; txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 20; txt.color = Color.white; txt.alignment = TextAnchor.MiddleCenter;
    }

    void CreateBtn(Transform parent, string label, float x0, float y0, float x1, float y1,
                   UnityEngine.Events.UnityAction action)
    {
        var rt = MakeRect(label, parent);
        SetAnchors(rt, x0, y0, x1, y1);
        rt.offsetMin = new Vector2(2, 2); rt.offsetMax = new Vector2(-2, -2);
        rt.gameObject.AddComponent<Image>().color = new Color(0.25f, 0.25f, 0.35f, 0.9f);
        rt.gameObject.AddComponent<Button>().onClick.AddListener(action);
        var tRt = MakeRect("T", rt.transform);
        SetAnchors(tRt, 0f, 0f, 1f, 1f);
        var txt = tRt.gameObject.AddComponent<Text>();
        txt.text = label; txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 18; txt.color = Color.white; txt.alignment = TextAnchor.MiddleCenter;
    }

    static RectTransform MakeRect(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.AddComponent<RectTransform>();
    }

    static void SetAnchors(RectTransform rt, float x0, float y0, float x1, float y1)
    {
        rt.anchorMin = new Vector2(x0, y0);
        rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    void ChangeIndex(int slot, int delta)
    {
        var parts = AllParts[slot];
        if (parts == null || parts.Length == 0) return;
        indices[slot] = (indices[slot] + delta + parts.Length) % parts.Length;
        UpdatePreview(slot);
    }

    void ChangeBGIndex(int delta)
    {
        if (backgroundSettings == null || backgroundSettings.Count == 0) return;
        bgIndex = (bgIndex + delta + backgroundSettings.Count) % backgroundSettings.Count;
        if (bgSr != null) bgSr.sprite = backgroundSettings.GetByIndex(bgIndex);
        UpdateBGLabel();
    }

    void UpdatePreview(int slot)
    {
        var parts = AllParts[slot];
        if (parts == null || parts.Length == 0) return;
        indices[slot] = Mathf.Clamp(indices[slot], 0, parts.Length - 1);
        if (previews[slot] != null) previews[slot].sprite = parts[indices[slot]].sprite;
        if (partLabels[slot] != null) partLabels[slot].text = $"{indices[slot] + 1}/{parts.Length}";
    }

    void UpdateAllPreviews()
    {
        for (int i = 0; i < 6; i++) UpdatePreview(i);
    }

    void UpdateBGLabel()
    {
        if (bgLabel == null || backgroundSettings == null) return;
        bgLabel.text = backgroundSettings.Count > 0
            ? $"{bgIndex + 1}/{backgroundSettings.Count}"
            : "0/0";
    }

    void OnConfirm()
    {
        CharacterSaveData.Save(indices);
        CharacterSaveData.SaveBg(bgIndex);
        SceneTransition.GoToBattle();
    }

    void Update()
    {
        if (bgmSource != null && !bgmSource.isPlaying)
            bgmSource.Play();
    }

    void PlayBGM()
    {
        if (selectBGM == null) return;
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.clip        = selectBGM;
        bgmSource.loop        = true;
        bgmSource.volume      = bgmVolume;
        bgmSource.playOnAwake = false;
        bgmSource.Play();
    }
}
