using UnityEngine;

public class GroundShadow : MonoBehaviour
{
    Transform follow;
    float stanceHeight = 3.75f;
    Vector3 baseScale;

    public void Init(Transform target, float charStanceHeight = 3.75f)
    {
        follow = target;
        stanceHeight = charStanceHeight;
        baseScale = transform.localScale;
    }

    void LateUpdate()
    {
        if (follow == null) return;
        float groundY = transform.position.y;
        transform.position = new Vector3(follow.position.x, groundY, transform.position.z);

        float height = follow.position.y - groundY;
        float excess = Mathf.Max(0f, height - stanceHeight);
        float t = Mathf.Clamp01(excess / 5f);
        float s = Mathf.Lerp(1f, 0.15f, t);
        transform.localScale = new Vector3(baseScale.x * s, baseScale.y * s, 1f);
    }
}
