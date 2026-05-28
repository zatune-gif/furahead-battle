using UnityEngine;
using UnityEngine.UI;

public class ResultOverlay : MonoBehaviour
{
    public AudioClip winBGM;
    public AudioClip loseBGM;
    [Range(0f, 1f)] public float bgmVolume = 0.7f;

    Canvas overlayCanvas;
    Text resultText;
    AudioSource bgmSource;

    void Awake() => Build();

    void Build()
    {
        if (overlayCanvas != null) return;

        var canvasGo = new GameObject("ResultCanvas");
        overlayCanvas = canvasGo.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 200;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        var bg = new GameObject("Background");
        bg.transform.SetParent(canvasGo.transform, false);
        var bgRt = bg.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        bg.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

        var textGo = new GameObject("ResultText");
        textGo.transform.SetParent(canvasGo.transform, false);
        var tRt = textGo.AddComponent<RectTransform>();
        tRt.anchorMin = tRt.anchorMax = tRt.pivot = new Vector2(0.5f, 0.5f);
        tRt.anchoredPosition = new Vector2(0f, 80f);
        tRt.sizeDelta = new Vector2(500f, 130f);
        resultText = textGo.AddComponent<Text>();
        resultText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        resultText.fontSize  = 90;
        resultText.color     = Color.yellow;
        resultText.alignment = TextAnchor.MiddleCenter;

        CreateButton(canvasGo.transform, "RETRY", -20f, SceneTransition.GoToBattle);
        CreateButton(canvasGo.transform, "TITLE", -100f, SceneTransition.GoToTitle);

        overlayCanvas.gameObject.SetActive(false);
    }

    static void CreateButton(Transform parent, string label, float yOffset, UnityEngine.Events.UnityAction action)
    {
        var go = new GameObject(label);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, yOffset);
        rt.sizeDelta = new Vector2(220f, 60f);
        go.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
        var btn = go.AddComponent<Button>();
        btn.onClick.AddListener(action);

        var textGo = new GameObject("Label");
        textGo.transform.SetParent(go.transform, false);
        var tRt = textGo.AddComponent<RectTransform>();
        tRt.anchorMin = Vector2.zero;
        tRt.anchorMax = Vector2.one;
        tRt.sizeDelta = Vector2.zero;
        var txt = textGo.AddComponent<Text>();
        txt.text      = label;
        txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize  = 26;
        txt.color     = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
    }

    public void Show(bool playerWon)
    {
        Build();
        overlayCanvas.gameObject.SetActive(true);
        resultText.text = playerWon ? "WIN!" : "LOSE...";
        PlayBGM(playerWon ? winBGM : loseBGM);
    }

    void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop        = true;
            bgmSource.playOnAwake = false;
        }
        bgmSource.clip   = clip;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }
}
