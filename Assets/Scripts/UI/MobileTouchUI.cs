using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobileTouchUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum ButtonType { Left, Right, Jump, Headbutt }

    ButtonType btnType;
    PlayerInputController input;

    public void Init(ButtonType type, PlayerInputController inputCtrl)
    {
        btnType = type;
        input   = inputCtrl;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (input == null) return;
        switch (btnType)
        {
            case ButtonType.Left:     input.touchLeft     = true; break;
            case ButtonType.Right:    input.touchRight    = true; break;
            case ButtonType.Jump:     input.touchJump     = true; break;
            case ButtonType.Headbutt: input.touchHeadbutt = true; break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (input == null) return;
        switch (btnType)
        {
            case ButtonType.Left:  input.touchLeft         = false; break;
            case ButtonType.Right: input.touchRight        = false; break;
            case ButtonType.Jump:  input.touchJumpReleased = true;  break;
        }
    }

    // 仮想ボタン1つを生成するファクトリ
    public static void Create(Transform parent, string label, ButtonType type,
                              Vector2 anchor, Vector2 anchoredPos,
                              PlayerInputController inputCtrl)
    {
        var go = new GameObject("TouchBtn_" + label);
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(180f, 180f);

        var img = go.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.22f);

        var touch = go.AddComponent<MobileTouchUI>();
        touch.Init(type, inputCtrl);

        // ラベルテキスト
        var textGo = new GameObject("Label");
        textGo.transform.SetParent(go.transform, false);
        var tRt = textGo.AddComponent<RectTransform>();
        tRt.anchorMin = Vector2.zero;
        tRt.anchorMax = Vector2.one;
        tRt.offsetMin = tRt.offsetMax = Vector2.zero;
        var txt = textGo.AddComponent<Text>();
        txt.text      = label;
        txt.font      = Resources.Load<Font>("NotoSansJP-Regular")
                        ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize  = 52;
        txt.color     = new Color(1f, 1f, 1f, 0.9f);
        txt.alignment = TextAnchor.MiddleCenter;
    }
}
