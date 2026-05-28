using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PortraitGuard : MonoBehaviour
{
    static PortraitGuard instance;

    Canvas overlayCanvas;
    GameObject portraitOverlay;

    public static void EnsureExists()
    {
        if (instance != null) return;
        var go = new GameObject("PortraitGuard");
        instance = go.AddComponent<PortraitGuard>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        CreateOverlayCanvas();
        WebGLBridge.SetupAudioUnlock();
        WebGLBridge.LockLandscape();

        if (WebGLBridge.IsFBBrowser())
            ShowFBBrowserMessage();
        else if (WebGLBridge.IsMobile())
        {
            WebGLBridge.SetupMobileCanvas();
            StartCoroutine(PortraitCheckLoop());
        }
    }

    void CreateOverlayCanvas()
    {
        var canvasGo = new GameObject("PortraitGuardCanvas");
        canvasGo.transform.SetParent(transform, false);
        DontDestroyOnLoad(canvasGo);
        overlayCanvas = canvasGo.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 100;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();
    }

    void ShowFBBrowserMessage()
    {
        var overlay = CreateFullOverlay("FBOverlay", new Color(0f, 0f, 0f, 0.92f));
        var textGo = new GameObject("FBText");
        textGo.transform.SetParent(overlay.transform, false);
        var trt = textGo.AddComponent<RectTransform>();
        trt.anchorMin = trt.anchorMax = trt.pivot = new Vector2(0.5f, 0.5f);
        trt.anchoredPosition = Vector2.zero;
        trt.sizeDelta = new Vector2(900f, 300f);
        var txt = textGo.AddComponent<Text>();
        txt.text = "Facebookアプリからはゲームを表示できません。\nSafariまたはChromeで開いてください。";
        txt.font = Resources.Load<Font>("NotoSansJP-Regular") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 40;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        var outline = textGo.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        outline.effectDistance = new Vector2(2f, -2f);
    }

    IEnumerator PortraitCheckLoop()
    {
        portraitOverlay = CreateFullOverlay("PortraitOverlay", new Color(0f, 0f, 0f, 0.88f));
        var textGo = new GameObject("PortraitText");
        textGo.transform.SetParent(portraitOverlay.transform, false);
        var trt = textGo.AddComponent<RectTransform>();
        trt.anchorMin = trt.anchorMax = trt.pivot = new Vector2(0.5f, 0.5f);
        trt.anchoredPosition = Vector2.zero;
        trt.sizeDelta = new Vector2(900f, 200f);
        var txt = textGo.AddComponent<Text>();
        txt.text = "スマホを横にしてください";
        txt.font = Resources.Load<Font>("NotoSansJP-Regular") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 60;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        var outline = textGo.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        outline.effectDistance = new Vector2(3f, -3f);
        portraitOverlay.SetActive(false);

        while (true)
        {
            bool portrait = WebGLBridge.IsPortrait();
            if (portraitOverlay != null) portraitOverlay.SetActive(portrait);
            Time.timeScale = portrait ? 0f : 1f;
            yield return new WaitForSecondsRealtime(0.3f);
        }
    }

    GameObject CreateFullOverlay(string name, Color bgColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(overlayCanvas.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = bgColor;
        return go;
    }
}
