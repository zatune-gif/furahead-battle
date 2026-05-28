using UnityEngine;
using UnityEngine.Events;

public class HeadbuttHitDetector : MonoBehaviour
{
    public UnityEvent OnHit = new UnityEvent();

    FighterController owner;
    FighterController opponent;
    float lastHitTime = -999f;

    public void Init(FighterController ownerCtrl, FighterController opponentCtrl)
    {
        owner = ownerCtrl;
        opponent = opponentCtrl;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (owner.State != CharacterState.Headbutting) return;

        if (collision.transform.root.GetComponent<FighterController>() != opponent) return;

        if (Time.time - lastHitTime < 0.3f) return;
        lastHitTime = Time.time;

        opponent.TakeDamage(owner.FacingDir);
        OnHit?.Invoke();
    }
}
