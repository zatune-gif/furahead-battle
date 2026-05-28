using UnityEngine;

public class CharacterBody : MonoBehaviour
{
    public GameObject head;
    public GameObject body;
    public GameObject armL;
    public GameObject armR;
    public GameObject legL;
    public GameObject legR;

    SpriteRenderer bodySr, headSr, armLSr, armRSr, legLSr, legRSr;

    public static CharacterBody Build(PartData[] parts, Vector3 position, Color tint)
    {
        var root = new GameObject("Character");
        root.transform.position = position;
        root.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        var cb = root.AddComponent<CharacterBody>();

        cb.body = CreatePart(parts[1], root.transform, Vector2.zero, tint);
        cb.body.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        cb.body.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        cb.bodySr = cb.body.GetComponent<SpriteRenderer>();
        cb.bodySr.sortingOrder = 3;

        cb.head = CreateLimb(parts[0], cb.body, new Vector2(0f, 0.86f), tint,
                              angDamp: 0.5f, mass: 0.6f,
                              anchorY: 0.42f, limitAngle: 20f,
                              limbScale: new Vector3(1.0f, 1.0f, 1f),
                              useCircle: true);
        cb.headSr = cb.head.GetComponent<SpriteRenderer>();
        cb.headSr.sortingOrder = 4;

        cb.armL = CreateLimb(parts[2], cb.body, new Vector2(-0.55f, -0.1f), tint,
                              angDamp: 0.25f, mass: 0.6f,
                              anchorY: 0.3f, limitAngle: 50f,
                              limbScale: new Vector3(0.65f, 1.2f, 1f));
        cb.armLSr = cb.armL.GetComponent<SpriteRenderer>();
        cb.armLSr.sortingOrder = 2;

        cb.armR = CreateLimb(parts[3], cb.body, new Vector2(0.55f, -0.1f), tint,
                              angDamp: 0.25f, mass: 0.6f,
                              anchorY: 0.3f, limitAngle: 50f,
                              limbScale: new Vector3(0.65f, 1.2f, 1f));
        cb.armRSr = cb.armR.GetComponent<SpriteRenderer>();
        cb.armRSr.sortingOrder = 2;

        cb.legL = CreateLimb(parts[4], cb.body, new Vector2(-0.25f, -0.73f), tint,
                              angDamp: 0.35f, mass: 0.7f,
                              anchorY: 0.29f, limitAngle: 40f,
                              limbScale: new Vector3(0.65f, 1.25f, 1f));
        cb.legLSr = cb.legL.GetComponent<SpriteRenderer>();
        cb.legLSr.sortingOrder = 1;

        cb.legR = CreateLimb(parts[5], cb.body, new Vector2(0.25f, -0.73f), tint,
                              angDamp: 0.35f, mass: 0.7f,
                              anchorY: 0.29f, limitAngle: 40f,
                              limbScale: new Vector3(0.65f, 1.25f, 1f));
        cb.legRSr = cb.legR.GetComponent<SpriteRenderer>();
        cb.legRSr.sortingOrder = 1;

        IgnoreSelfCollisions(cb);
        return cb;
    }

    static GameObject CreatePart(PartData data, Transform parent, Vector2 offset, Color tint,
                                  bool useCircle = false)
    {
        var go = new GameObject(data.partType.ToString());
        go.transform.SetParent(parent);
        go.transform.localPosition = (Vector3)(offset + data.anchorOffset);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = data.sprite;
        sr.color = tint;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.angularDamping = 5f;
        rb.mass = 1f;

        if (useCircle)
        {
            var circle = go.AddComponent<CircleCollider2D>();
            circle.sharedMaterial = new PhysicsMaterial2D("HeadSlide") { friction = 0f, bounciness = 0f };
        }
        else
        {
            go.AddComponent<BoxCollider2D>();
        }

        return go;
    }

    static GameObject CreateLimb(PartData data, GameObject anchor, Vector2 offset, Color tint,
                                  float angDamp = 0.4f, float mass = 0.8f,
                                  float anchorY = 0f, float limitAngle = 45f,
                                  Vector3? limbScale = null,
                                  bool useCircle = false)
    {
        var go = CreatePart(data, anchor.transform, offset, tint, useCircle);
        if (limbScale.HasValue) go.transform.localScale = limbScale.Value;

        var rb = go.GetComponent<Rigidbody2D>();
        rb.angularDamping = angDamp;
        rb.mass = mass;

        var joint = go.AddComponent<HingeJoint2D>();
        joint.connectedBody = anchor.GetComponent<Rigidbody2D>();
        joint.anchor = new Vector2(0f, anchorY);
        joint.autoConfigureConnectedAnchor = false;
        Vector2 worldPivot = go.transform.TransformPoint(new Vector3(0f, anchorY, 0f));
        joint.connectedAnchor = anchor.transform.InverseTransformPoint(worldPivot);

        joint.useLimits = true;
        var limits = new JointAngleLimits2D { min = -limitAngle, max = limitAngle };
        joint.limits = limits;

        return go;
    }

    static void IgnoreSelfCollisions(CharacterBody cb)
    {
        var cols = new[]
        {
            cb.body.GetComponent<Collider2D>(),
            cb.head.GetComponent<Collider2D>(),
            cb.armL.GetComponent<Collider2D>(),
            cb.armR.GetComponent<Collider2D>(),
            cb.legL.GetComponent<Collider2D>(),
            cb.legR.GetComponent<Collider2D>(),
        };
        for (int i = 0; i < cols.Length; i++)
            for (int j = i + 1; j < cols.Length; j++)
                Physics2D.IgnoreCollision(cols[i], cols[j]);
    }

    public void ApplyFacing(int dir)
    {
        bool flip = dir < 0;
        bodySr.flipX = flip;
        headSr.flipX = flip;
        armLSr.flipX = true;
        armRSr.flipX = false;
        legLSr.flipX = true;
        legRSr.flipX = false;
    }
}
