using UnityEngine;

public class DynamicBackground : MonoBehaviour
{
    public float padding = 0f;

    void LateUpdate()
    {
        var cam = Camera.main;
        if (cam == null) return;
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        float camH = cam.orthographicSize * 2f;
        float camW = camH * cam.aspect;
        var bounds = sr.sprite.bounds;
        transform.localScale = new Vector3(
            (camW + padding) / bounds.size.x,
            (camH + padding) / bounds.size.y,
            1f);
    }
}
