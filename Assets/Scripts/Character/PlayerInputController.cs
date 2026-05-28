using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    FighterController ctrl;

    // タッチボタンから設定されるフラグ
    [HideInInspector] public bool touchLeft;
    [HideInInspector] public bool touchRight;
    [HideInInspector] public bool touchJump;
    [HideInInspector] public bool touchJumpReleased;
    [HideInInspector] public bool touchHeadbutt;

    public void Init(FighterController controller) => ctrl = controller;

    void Update()
    {
        if (ctrl == null) return;

        float h = Input.GetAxisRaw("Horizontal");
        if (touchLeft)  h = -1f;
        if (touchRight) h =  1f;
        ctrl.Move(h);

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.X) || touchHeadbutt)
            ctrl.Headbutt();

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || touchJump)
            ctrl.Jump();

        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow) || touchJumpReleased)
            ctrl.CutJump();

        // 1フレームイベントをリセット
        touchHeadbutt     = false;
        touchJump         = false;
        touchJumpReleased = false;
    }
}
