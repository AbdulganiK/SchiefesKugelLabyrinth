using UnityEngine;

public class KugelController : MonoBehaviour
{
    [Header("Physik")]
    public float masse = 1f;
    public float gravitation = 9.81f;

    // WICHTIG: Das ist jetzt der LOKALE Radius (Unity-Sphere = 0.5)
    public float kugelRadius = 0.5f;

    [Header("Brett")]
    public Transform brett;
    public Vector2 brettHalbGroesseLocalXZ = new Vector2(5f, 5f);
    public float brettOberflaechenOffset = 0f;
    public bool invertiereHangkraft = true;

    [Header("Reibung")]
    public float stillSpeed = 0.001f;

    [Header("Stoß")]
    [Range(0f, 1f)] public float rueckprallWand = 0.0f;
    [Range(0f, 1f)] public float rueckprallBrett = 0.0f;

    [Header("Kollisionen")]
    public Transform[] waende;
    public int solverIterations = 2;

    private Vector3 position;
    private Vector3 velocity;

    // ✅ World-Radius automatisch aus Scale
    private float KugelRadiusWorld =>
        kugelRadius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);

    void Start()
    {
        position = transform.position;
        velocity = Vector3.zero;
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        float rWorld = KugelRadiusWorld;

        bool ueberBrett = IsOverBoard(position, rWorld);

        Vector2 Fhang2 = Vector2.zero;
        float alphaDeg = 0f;

        if (ueberBrett)
        {
            float winkelLinksRechts = GetSignedAngle(brett.localEulerAngles.z);
            float winkelVorZurueck  = -GetSignedAngle(brett.localEulerAngles.x);

            Fhang2 = Physik.berechneHangabtriebskraft(masse, gravitation, winkelLinksRechts, winkelVorZurueck);
            if (invertiereHangkraft) Fhang2 = -Fhang2;

            float cosAlpha = Mathf.Clamp(Vector3.Dot(brett.up.normalized, Vector3.up), -1f, 1f);
            alphaDeg = Mathf.Acos(cosAlpha) * Mathf.Rad2Deg;
        }

        // Gravitation (freier Fall)
        velocity.y += -gravitation * dt;

        // X/Z nur wenn Kontakt/über Brett
        if (ueberBrett && IsOnBoardPlane(position, rWorld))
        {
            Vector2 vXZ = new Vector2(velocity.x, velocity.z);

            float FhaftMax = Physik.berechneHaftReibungsKraft(Physik.HAFT_REIBUNG_KOEF, masse, gravitation, alphaDeg);
            float Froll     = Physik.berechneRollReibungsKraft(Physik.ROLL_REIBUNG_KOEF, masse, gravitation, alphaDeg);

            if (vXZ.magnitude < stillSpeed && Fhang2.magnitude <= FhaftMax)
            {
                vXZ = Vector2.zero;
            }
            else
            {
                Vector2 dir =
                    (vXZ.sqrMagnitude > 1e-6f) ? -vXZ.normalized :
                    (Fhang2.sqrMagnitude > 1e-6f) ? -Fhang2.normalized :
                    Vector2.zero;

                Vector2 Freib2 = dir * Froll;
                Vector2 a2 = (Fhang2 + Freib2) / masse;
                vXZ += a2 * dt;
            }

            velocity.x = vXZ.x;
            velocity.z = vXZ.y;
        }

        // Integration
        position += velocity * dt;

        // Kollisionen lösen
        for (int i = 0; i < solverIterations; i++)
        {
            ResolveWallCollisions_OBB(ref position, ref velocity, rWorld);
            ResolveBoardPlane(ref position, ref velocity, rWorld);
        }

        transform.position = position;
    }

    // -------------------- Brett --------------------

    bool IsOverBoard(Vector3 worldPos, float rWorld)
    {
        Vector3 lp = brett.InverseTransformPoint(worldPos);

        // Radius in Board-Localspace (grob)
        float sXZ = Mathf.Max(Mathf.Abs(brett.lossyScale.x), Mathf.Abs(brett.lossyScale.z));
        float rLocal = (sXZ > 1e-6f) ? (rWorld / sXZ) : rWorld;

        float mx = brettHalbGroesseLocalXZ.x + rLocal;
        float mz = brettHalbGroesseLocalXZ.y + rLocal;

        return Mathf.Abs(lp.x) <= mx && Mathf.Abs(lp.z) <= mz;
    }

    bool IsOnBoardPlane(Vector3 worldPos, float rWorld)
    {
        Vector3 n = brett.up.normalized;
        Vector3 p0 = brett.position + n * brettOberflaechenOffset;
        float dist = Vector3.Dot(worldPos - p0, n);
        return dist <= rWorld + 0.002f;
    }

    void ResolveBoardPlane(ref Vector3 pos, ref Vector3 vel, float rWorld)
    {
        if (!IsOverBoard(pos, rWorld)) return;

        Vector3 n = brett.up.normalized;
        Vector3 p0 = brett.position + n * brettOberflaechenOffset;

        float dist = Vector3.Dot(pos - p0, n);

        if (dist < rWorld)
        {
            float push = rWorld - dist;
            pos += n * push;

            float vN = Vector3.Dot(vel, n);
            if (vN < 0f)
            {
                float vNnach = Physik.berechneStoßMitStarrenWand(vN) * rueckprallBrett;

                Vector3 vNormal = vN * n;
                Vector3 vTang = vel - vNormal;
                vel = vTang + (vNnach * n);
            }
        }
    }

    // -------------------- Wände (FIX: OBB statt Renderer.bounds AABB) --------------------

    void ResolveWallCollisions_OBB(ref Vector3 pos, ref Vector3 vel, float rWorld)
    {
        if (waende == null) return;

        for (int i = 0; i < waende.Length; i++)
        {
            Transform w = waende[i];
            if (w == null) continue;
            if (!w.gameObject.activeInHierarchy) continue;

            if (!TryGetWallLocalBounds(w, out Bounds localBounds))
                continue;

            // Kugelzentrum in Wall-Localspace
            Vector3 pLocal = w.InverseTransformPoint(pos);

            // Radius in Wall-Localspace (konservativ)
            float s = Mathf.Max(Mathf.Abs(w.lossyScale.x), Mathf.Abs(w.lossyScale.y), Mathf.Abs(w.lossyScale.z));
            if (s < 1e-6f) continue;
            float rLocal = rWorld / s;

            Vector3 min = localBounds.min;
            Vector3 max = localBounds.max;

            Vector3 closest = new Vector3(
                Mathf.Clamp(pLocal.x, min.x, max.x),
                Mathf.Clamp(pLocal.y, min.y, max.y),
                Mathf.Clamp(pLocal.z, min.z, max.z)
            );

            Vector3 delta = pLocal - closest;
            float dist2 = delta.sqrMagnitude;

            if (dist2 < rLocal * rLocal)
            {
                float dist = Mathf.Sqrt(dist2);

                Vector3 nLocal = (dist > 1e-6f)
                    ? (delta / dist)
                    : GuessNormalLocal(pLocal, min, max);

                float pushLocal = rLocal - dist;

                // zurück in Worldspace
                Vector3 nWorld = w.TransformDirection(nLocal).normalized;
                float pushWorld = pushLocal * s;

                pos += nWorld * pushWorld;

                float vN = Vector3.Dot(vel, nWorld);
                if (vN < 0f)
                {
                    float vNnach = -vN * rueckprallWand; // 0 => kein Abprall, 1 => voller Abprall

                    vel = (vel - vN * nWorld) + (vNnach * nWorld);
                }
            }
        }
    }

    bool TryGetWallLocalBounds(Transform w, out Bounds b)
    {
        // ✅ Local bounds (nicht Renderer.bounds!)
        MeshFilter mf = w.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            b = mf.sharedMesh.bounds; // local space
            return true;
        }

        // Fallback (für primitive cube ohne meshfilter wäre ungewöhnlich)
        b = new Bounds(Vector3.zero, Vector3.one);
        return true;
    }

    Vector3 GuessNormalLocal(Vector3 p, Vector3 min, Vector3 max)
    {
        float dxMin = Mathf.Abs(p.x - min.x);
        float dxMax = Mathf.Abs(max.x - p.x);
        float dyMin = Mathf.Abs(p.y - min.y);
        float dyMax = Mathf.Abs(max.y - p.y);
        float dzMin = Mathf.Abs(p.z - min.z);
        float dzMax = Mathf.Abs(max.z - p.z);

        float m = dxMin; Vector3 n = Vector3.left;
        if (dxMax < m) { m = dxMax; n = Vector3.right; }
        if (dyMin < m) { m = dyMin; n = Vector3.down; }
        if (dyMax < m) { m = dyMax; n = Vector3.up; }
        if (dzMin < m) { m = dzMin; n = Vector3.back; }
        if (dzMax < m) { m = dzMax; n = Vector3.forward; }
        return n;
    }

    // -------------------- Utils --------------------

    float GetSignedAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
