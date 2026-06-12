using UnityEngine;

public class FighterController : MonoBehaviour
{
    [Header("Parameters")]
    public float moveSpeed = 7f;
    public float headbuttForce = 20f;
    public float jumpForce = 32f;
    public float headbuttCooldown = 0.3f;

    [Header("Audio")]
    public AudioClip jumpSE;
    public AudioClip headbuttSE;

    // 挙動チューニング定数
    const float JumpLockoutSec     = 0.5f;   // ジャンプ連打防止
    const float HeadbuttDurationSec = 0.4f;  // 頭突きモーション時間
    const float FallGravityScale   = 2.5f;   // 落下時の重力倍率（キレのある着地）
    const float KnockbackSpeedX    = 10f;
    const float KnockbackSpeedY    = 6f;
    const float UprightRecoverSec  = 0.25f;  // 頭突き後に直立へ戻す時間
    const float JumpCutMultiplier  = 0.4f;   // ジャンプボタン早離し時の上昇減衰

    AudioSource audioSource;

    public CharacterState State { get; private set; } = CharacterState.Idle;
    public int HitCount { get; private set; } = 0;
    public bool IsAlive => HitCount < CombatSystem.HitsToDown;
    public Vector2 BodyPosition => bodyRb != null ? bodyRb.position : (Vector2)transform.position;

    Rigidbody2D bodyRb;
    Rigidbody2D headRb;
    CharacterBody charBody;
    float cooldownTimer;
    float jumpLockout;
    int facingDir = 1;

    public void Init(Rigidbody2D bodyRigidbody, Rigidbody2D headRigidbody = null)
    {
        bodyRb = bodyRigidbody;
        headRb = headRigidbody;
        charBody = GetComponent<CharacterBody>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
        if (jumpLockout  > 0f) jumpLockout  -= Time.deltaTime;

        if (bodyRb != null)
            bodyRb.gravityScale = bodyRb.linearVelocity.y < 0f ? FallGravityScale : 1f;
    }

    public bool CanJump => bodyRb != null && jumpLockout <= 0f && Mathf.Abs(bodyRb.linearVelocity.y) < 0.2f;

    public void Jump()
    {
        if (!CanJump || State == CharacterState.Headbutting) return;
        jumpLockout = JumpLockoutSec;
        bodyRb.linearVelocity = new Vector2(bodyRb.linearVelocity.x, jumpForce);
        if (jumpSE != null) audioSource.PlayOneShot(jumpSE);
    }

    public void Move(float direction)
    {
        if (State == CharacterState.Headbutting) return;
        bodyRb.linearVelocity = new Vector2(direction * moveSpeed, bodyRb.linearVelocity.y);
        if (Mathf.Abs(direction) > 0.01f)
        {
            int newDir = direction > 0 ? 1 : -1;
            if (newDir != facingDir) SetFacing(newDir);
            TransitionTo(CharacterState.Moving);
        }
        else
        {
            TransitionTo(CharacterState.Idle);
        }
    }

    public void Headbutt()
    {
        if (State == CharacterState.Headbutting || cooldownTimer > 0f) return;
        TransitionTo(CharacterState.Headbutting);

        bodyRb.constraints = RigidbodyConstraints2D.None;
        bodyRb.linearVelocity = new Vector2(facingDir * headbuttForce * 0.6f, 0f);
        bodyRb.AddTorque(-facingDir * 15f, ForceMode2D.Impulse);

        if (headRb != null)
            headRb.AddForce(new Vector2(facingDir * 12f, -6f), ForceMode2D.Impulse);

        cooldownTimer = headbuttCooldown;
        if (headbuttSE != null) audioSource.PlayOneShot(headbuttSE);
        Invoke(nameof(EndHeadbutt), HeadbuttDurationSec);
    }

    public void TakeDamage(int attackerDir)
    {
        HitCount++;
        if (!IsAlive)
            Die();
        else
            ApplyKnockback(attackerDir);
    }

    public void Die()
    {
        CancelInvoke();
        if (bodyRb == null) return;
        bodyRb.constraints = RigidbodyConstraints2D.None;
        bodyRb.gravityScale = 3f;
        bodyRb.AddTorque(-facingDir * 10f, ForceMode2D.Impulse);
        bodyRb.linearVelocity = new Vector2(-facingDir * 2f, 3f);
        enabled = false;
    }

    void ApplyKnockback(int attackerDir)
    {
        if (bodyRb == null) return;
        bodyRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        bodyRb.linearVelocity = new Vector2(attackerDir * KnockbackSpeedX, KnockbackSpeedY);
    }

    public void CutJump()
    {
        if (bodyRb != null && bodyRb.linearVelocity.y > 0f)
            bodyRb.linearVelocity = new Vector2(bodyRb.linearVelocity.x, bodyRb.linearVelocity.y * JumpCutMultiplier);
    }

    public int FacingDir => facingDir;

    public void SetFacing(int direction)
    {
        facingDir = direction;
        if (charBody != null) charBody.ApplyFacing(direction);
    }

    void TransitionTo(CharacterState next) => State = next;

    void EndHeadbutt()
    {
        if (bodyRb != null)
            bodyRb.linearVelocity = Vector2.zero;
        TransitionTo(CharacterState.Idle);
        StartCoroutine(RestoreUpright());
    }

    System.Collections.IEnumerator RestoreUpright()
    {
        if (bodyRb == null) yield break;
        float elapsed = 0f;
        float duration = UprightRecoverSec;
        float startAngle = bodyRb.rotation;
        bodyRb.angularVelocity = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bodyRb.rotation = Mathf.LerpAngle(startAngle, 0f, elapsed / duration);
            yield return null;
        }
        bodyRb.rotation = 0f;
        bodyRb.angularVelocity = 0f;
        bodyRb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}
