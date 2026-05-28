using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Announcement (auto-found if blank)")]
    public Text announcementText;

    void Awake()
    {
        GameObject.Find("PlayerHPBar")?.SetActive(false);
        GameObject.Find("CPUHPBar")?.SetActive(false);

        if (announcementText == null)
            announcementText = GameObject.Find("AnnouncementText")?.GetComponent<Text>();
        if (announcementText == null)
            announcementText = CreateAnnouncementText();
        announcementText.text = "";
    }

    Text CreateAnnouncementText()
    {
        var canvasGo = new GameObject("AnnouncementCanvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGo.AddComponent<CanvasScaler>();

        // 暗い帯背景
        var band = new GameObject("AnnouncementBand");
        band.transform.SetParent(canvas.transform, false);
        var bandRt = band.AddComponent<RectTransform>();
        bandRt.anchorMin = new Vector2(0f, 0.4f);
        bandRt.anchorMax = new Vector2(1f, 0.6f);
        bandRt.offsetMin = bandRt.offsetMax = Vector2.zero;
        band.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

        var go = new GameObject("AnnouncementText");
        go.transform.SetParent(canvas.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0.35f);
        rt.anchorMax = new Vector2(1f, 0.65f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var txt = go.AddComponent<Text>();
        txt.font            = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize        = 120;
        txt.color           = new Color(0.75f, 0.5f, 1f);
        txt.alignment       = TextAnchor.MiddleCenter;
        txt.verticalOverflow   = VerticalWrapMode.Overflow;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        var outline = go.AddComponent<Outline>();
        outline.effectColor    = new Color(0f, 0f, 0f, 0.8f);
        outline.effectDistance = new Vector2(3f, -3f);
        return txt;
    }

    public IEnumerator ShowFightAnnouncement()
    {
        if (announcementText != null)
        {
            announcementText.color = new Color(0.75f, 0.5f, 1f);
            announcementText.text  = "FIGHT!";
        }
        yield return new WaitForSeconds(2f);
        if (announcementText != null) announcementText.text = "";
    }
}
