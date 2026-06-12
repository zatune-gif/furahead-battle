using UnityEngine;

public class AIController : MonoBehaviour
{
    public AIDifficultyData difficulty;

    FighterController ai;
    FighterController player;
    float stateTimer;
    float startupTimer;
    int prevHitCount;
    Camera cam;

    // 接近中にジャンプ接近へ移行する1フレームあたりの確率
    const float JumpApproachChancePerFrame = 0.004f;

    enum AIState { Approach, Wait, Retreat, JumpApproach }
    AIState aiState = AIState.Approach;

    public void Init(FighterController aiCtrl, FighterController playerCtrl)
    {
        ai = aiCtrl;
        player = playerCtrl;
        cam = Camera.main;
        if (difficulty == null)
            difficulty = ScriptableObject.CreateInstance<AIDifficultyData>();
        prevHitCount = ai.HitCount;
        startupTimer = Random.Range(1.0f, 2.0f);
    }

    float HalfW() => cam != null ? cam.orthographicSize * cam.aspect : 8f;

    bool NearEdge(float x, float margin = 1.5f) => Mathf.Abs(x) > HalfW() - margin;

    static int TowardCenter(float x) => x > 0f ? -1 : 1;

    void Update()
    {
        if (ai == null || player == null) return;
        if (!ai.IsAlive || !player.IsAlive) return;
        if (ai.State == CharacterState.Headbutting) return;

        // ゲーム開始直後は静止（初動突進を防ぐ）
        if (startupTimer > 0f)
        {
            startupTimer -= Time.deltaTime;
            return;
        }

        if (ai.HitCount > prevHitCount)
        {
            prevHitCount = ai.HitCount;
            EnterRetreat(Random.Range(0.6f, 1.2f));
            return;
        }
        prevHitCount = ai.HitCount;

        // 水平距離のみで判定（ジャンプ中のY差を無視する）
        float dist = Mathf.Abs(ai.BodyPosition.x - player.BodyPosition.x);
        int dir = player.BodyPosition.x > ai.BodyPosition.x ? 1 : -1;
        ai.SetFacing(dir);

        switch (aiState)
        {
            case AIState.Approach:     UpdateApproach(dist, dir); break;
            case AIState.Wait:         UpdateWait(); break;
            case AIState.Retreat:      UpdateRetreat(); break;
            case AIState.JumpApproach: UpdateJumpApproach(dist, dir); break;
        }
    }

    void UpdateApproach(float dist, int dir)
    {
        float aiX = ai.BodyPosition.x;

        // 壁際では中央に向かう（頭突きで飛ばされても自力で戻れる）
        if (NearEdge(aiX))
        {
            ai.Move(TowardCenter(aiX) * difficulty.approachSpeed / 4f);
            return;
        }

        if (dist > 1.5f)
        {
            ai.Move(dir * difficulty.approachSpeed / 4f);

            if (dist < 3f && Random.value < JumpApproachChancePerFrame)
            {
                EnterJumpApproach();
                return;
            }
        }
        else
        {
            ai.Move(0);
            float roll = Random.value;
            float feintChance = 0.25f;
            float jumpChance  = 0.2f;

            if (roll < feintChance)
            {
                EnterRetreat(Random.Range(0.4f, 0.8f));
            }
            else if (roll < feintChance + jumpChance)
            {
                EnterJumpApproach();
            }
            else
            {
                float baseWait = Random.Range(difficulty.waitTimeRange.x, difficulty.waitTimeRange.y);
                stateTimer = player.HitCount < difficulty.lowHPThreshold
                    ? baseWait / difficulty.lowHPHeadbuttMultiplier
                    : baseWait;
                aiState = AIState.Wait;
            }
        }
    }

    void UpdateWait()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            stateTimer = 999f;
            ai.Headbutt();
            Invoke(nameof(ReturnToApproach), 0.8f);
        }
    }

    void UpdateRetreat()
    {
        float aiX = ai.BodyPosition.x;
        int dir = player.BodyPosition.x > ai.BodyPosition.x ? 1 : -1;

        // 壁際なら中央方向、それ以外はプレイヤーから離れる方向へ
        int retreatDir = NearEdge(aiX) ? TowardCenter(aiX) : -dir;
        ai.Move(retreatDir * difficulty.approachSpeed / 4f);

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
            ReturnToApproach();
    }

    void UpdateJumpApproach(float dist, int dir)
    {
        ai.Move(dir * difficulty.approachSpeed / 4f);
        stateTimer -= Time.deltaTime;

        if (dist < 1.2f)
        {
            ai.Move(0);
            ai.Headbutt();
            stateTimer = 999f;
            aiState = AIState.Wait;
            Invoke(nameof(ReturnToApproach), 0.8f);
        }
        else if (stateTimer <= 0f)
        {
            ReturnToApproach();
        }
    }

    void EnterRetreat(float duration)
    {
        CancelInvoke(nameof(ReturnToApproach));
        aiState = AIState.Retreat;
        stateTimer = duration;
    }

    void EnterJumpApproach()
    {
        aiState = AIState.JumpApproach;
        stateTimer = 1.0f;
        ai.Jump();
    }

    void ReturnToApproach() => aiState = AIState.Approach;
}
