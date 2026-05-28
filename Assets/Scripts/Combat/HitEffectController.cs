using System.Collections;
using UnityEngine;

public class HitEffectController : MonoBehaviour
{
    public AudioClip hitSE;
    AudioSource audioSource;

    Texture2D flashTex;
    Texture2D starTex;
    // 8方向の単位ベクトルをAwakeで事前計算
    readonly Vector3[] starDirs = new Vector3[8];

    void Awake()
    {
        flashTex = BuildFlashTex();
        starTex  = BuildStarTex();
        for (int i = 0; i < 8; i++)
        {
            float a = i * 45f * Mathf.Deg2Rad;
            starDirs[i] = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f);
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 1f;
        audioSource.spatialBlend = 0f;
        audioSource.priority = 0;
        audioSource.bypassEffects = true;
        audioSource.bypassListenerEffects = true;
        audioSource.bypassReverbZones = true;
    }

    public void PlayHitEffect(Vector3 position, bool isPlayerAttack = true)
    {
        if (hitSE != null)
        {
            audioSource.PlayOneShot(hitSE, 1f);
            audioSource.PlayOneShot(hitSE, 1f);
            audioSource.PlayOneShot(hitSE, 1f);
        }
        StartCoroutine(SpawnHitEffect(position, isPlayerAttack));
    }

    IEnumerator SpawnHitEffect(Vector3 pos, bool isPlayerAttack)
    {
        Color flashStart = isPlayerAttack ? Color.white               : new Color(1f, 0.25f, 0f);
        Color flashEnd   = isPlayerAttack ? new Color(1f, 0.9f, 0f, 0f) : new Color(0.6f, 0f, 0f, 0f);

        var flashSr = Spawn("HitFlash", pos, flashTex, 64, 64, flashStart, 20, 1.0f);

        var starSrs = new SpriteRenderer[8];
        for (int i = 0; i < 8; i++)
        {
            Color sc = isPlayerAttack ? PlayerStarColor(i) : NpcStarColor(i);
            starSrs[i] = Spawn("HitStar", pos + starDirs[i] * 0.15f, starTex, 32, 32, sc, 21, 0.4f);
        }

        float t = 0f, dur = 0.4f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float ratio = t / dur;

            if (flashSr != null)
            {
                flashSr.transform.localScale = Vector3.one * Mathf.Lerp(1.0f, 3.8f, ratio);
                flashSr.color = Color.Lerp(flashStart, flashEnd, ratio);
            }

            for (int i = 0; i < 8; i++)
            {
                if (starSrs[i] == null) continue;
                starSrs[i].transform.position   = pos + starDirs[i] * Mathf.Lerp(0.15f, 1.8f, ratio);
                starSrs[i].transform.localScale  = Vector3.one * Mathf.Lerp(0.4f, 0.08f, ratio);
                var c = starSrs[i].color;
                starSrs[i].color = new Color(c.r, c.g, c.b, 1f - ratio);
            }

            yield return null;
        }

        if (flashSr != null) Destroy(flashSr.gameObject);
        foreach (var sr in starSrs) if (sr != null) Destroy(sr.gameObject);
    }

    SpriteRenderer Spawn(string goName, Vector3 pos, Texture2D tex, int w, int h,
                         Color color, int order, float scale)
    {
        var go = new GameObject(goName);
        go.transform.position   = pos;
        go.transform.localScale = Vector3.one * scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
        sr.color        = color;
        sr.sortingOrder = order;
        return sr;
    }

    static Texture2D BuildFlashTex()
    {
        var tex    = new Texture2D(64, 64);
        var center = new Vector2(32f, 32f);
        for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center) / 32f;
                float a = d < 0.9f ? Mathf.Lerp(1f, 0f, d / 0.9f) : 0f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        return tex;
    }

    static Texture2D BuildStarTex()
    {
        var tex    = new Texture2D(32, 32);
        var center = new Vector2(16f, 16f);
        for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center) / 16f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, d < 0.7f ? 1f : 0f));
            }
        tex.Apply();
        return tex;
    }

    static Color PlayerStarColor(int i)
    {
        Color[] c = {
            Color.yellow, new Color(1f,0.5f,0f), Color.white, Color.cyan,
            Color.yellow, new Color(1f,0.9f,0.2f), Color.white, new Color(1f,0.8f,0f)
        };
        return c[i % c.Length];
    }

    static Color NpcStarColor(int i)
    {
        Color[] c = {
            new Color(1f,0.15f,0f), new Color(1f,0.4f,0f),  new Color(0.9f,0f,0f),   new Color(1f,0.25f,0f),
            new Color(1f,0f,0.1f),  new Color(1f,0.3f,0f),  new Color(0.8f,0f,0.05f), new Color(1f,0.1f,0f)
        };
        return c[i % c.Length];
    }
}
