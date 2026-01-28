using System;
using System.Numerics;
using UnityEngine;
using Unity.Mathematics;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class KugelController : MonoBehaviour
{
    [Header("Physik")]
    public float masse = 1f;
    public float gravitation = 9.81f;
    public float kugelRadius = 0.5f;

    [Header("Brett")]
    public Transform brett;
    public Vector2 brettHalbGroesseLocalXZ = new Vector2(5f, 5f);
    public bool invertiereHangkraft = true;

    [Header("Reibung (Coulomb)")]
    [Range(0f, 2f)] public float muHaft = 0.35f;   // Haftreibung (static)
    [Range(0f, 2f)] public float muGleit = 0.25f;  // Gleitreibung (kinetic)
    public float stopSpeed = 0.05f;                // darunter "klebt" die Kugel leichter

    [Header("Kollision")]
    public Transform[] waende;
    [Range(0f, 1f)] public float rueckprallWand = 0.5f;
    [Range(0f, 1f)] public float rueckprallBrett = 0.0f;

    private Vector3 position;
    private Vector3 velocity;
    private Vector3 accelleration;
    private int ticks = 0;

    private float FhaftValue;
    private float FgleitValue;

    private float KugelRadiusWorld
    {
        get
        {
            Vector3 scale = transform.lossyScale;
            return kugelRadius * Mathf.Max(scale.x, scale.y, scale.z);
        }
    }
    public void resetBall()
    {
        Vector3 startPos = new Vector3(-4.5f, 0.5f, 4.5f);

        velocity = Vector3.zero;
        ticks = 0;

        // Transform aktualisieren
        position = startPos;
        transform.position = startPos;
    }

    private void OnDisable()
    {
        TickManager.Instance.OnTick -= HandleTick;
    }

    void Start()
    {
        TickManager.Instance.OnTick += HandleTick;
        position = transform.position;
        velocity = Vector3.zero;
    }

    void HandleTick()
    {
        float dt = Time.fixedDeltaTime;
        float rWorld = KugelRadiusWorld;

        position = transform.position;

        // Gravitation (für Y)
        velocity.y += -gravitation * masse * dt;

        bool aufBrett = (brett != null && IsOverBoard(position, rWorld));

        if (aufBrett)
        {
            Vector3 normal = brett.up.normalized;

            // Hang "Beschleunigung" in Brett-Ebene (aus Gravitation projiziert)
            Vector3 gVec = Vector3.down * gravitation;
            Vector3 aHang = Vector3.ProjectOnPlane(gVec, normal); // in Brett-Ebene
            if (invertiereHangkraft) aHang = -aHang;

            // Jetzt als Kraft rechnen: F = m * a
            Vector3 Fhang = masse * aHang;

            // Reibungskraft aus Normalkraft
            Vector3 Freib = ComputeFrictionForce(normal, Fhang, dt);

            // Gesamtbeschleunigung: a = (Fhang + Freib) / m
            accelleration = (Fhang + Freib) / Mathf.Max(0.0001f, masse);

            // Nur in Brett-Ebene anwenden (Y nicht verfälschen)
            accelleration = Vector3.ProjectOnPlane(accelleration, normal);

            velocity += accelleration * dt;
        }

        Vector3 newPos = position + velocity * dt;

        newPos = BehandleWandKollisionen(position, newPos, rWorld);

        if (brett != null)
            newPos = BehandleBrettKollision(newPos, rWorld);

        position = newPos;
        transform.position = position;
        ticks += 1;
    }

    // --- Reibung als Kraft: F_f = mu * N ---
    Vector3 ComputeFrictionForce(Vector3 boardNormal, Vector3 Fhang, float dt)
    {
        // Normalkraft N = m * g * cos(theta)
        float cos = Mathf.Clamp01(Mathf.Abs(Vector3.Dot(boardNormal, Vector3.up)));
        float N = masse * gravitation * cos;

        FhaftValue = muHaft * N;
        FgleitValue = muGleit * N;

        // Tangentialgeschwindigkeit (in Brett-Ebene)
        Vector3 vT = Vector3.ProjectOnPlane(velocity, boardNormal);
        float speed = vT.magnitude;

        // Tangentiale Hangkraft
        Vector3 FhangT = Vector3.ProjectOnPlane(Fhang, boardNormal);
        float FhangMag = FhangT.magnitude;

        // 1) Haftreibung: wenn fast still und Hangkraft kleiner als Haft-Max -> komplett halten
        if (speed < stopSpeed && FhangMag < FhaftValue)
        {
            // Reibung hebt Hangkraft auf (gleich groß, entgegengesetzt)
            // => keine Beschleunigung in der Ebene
            Vector3 Fhaft = -FhangT;

            // Restliches "Zittern" weg: Tangentialvelocity auf 0 ziehen (impulsartig)
            // als Kraft angenähert: F = m * dv/dt
            Vector3 Fstop = -vT * (masse / Mathf.Max(1e-6f, dt));

            // Optional begrenzen, damit es nicht explodiert:
            float FstopMax = 5f * N;
            if (Fstop.sqrMagnitude > FstopMax * FstopMax)
                Fstop = Fstop.normalized * FstopMax;

            return Fhaft + Fstop;
        }

        // 2) Gleitreibung: immer entgegen der Bewegungsrichtung (oder entgegen Hangrichtung, falls speed ~ 0)
        Vector3 dir;
        if (speed > 1e-6f) dir = vT / speed;
        else if (FhangMag > 1e-6f) dir = FhangT / FhangMag;
        else dir = Vector3.zero;

        Vector3 Fg = -dir * FgleitValue;

        return Fg;
    }

    bool IsOverBoard(Vector3 worldPos, float rWorld)
    {
        Vector3 localPos = brett.InverseTransformPoint(worldPos);
        float buffer = rWorld * 0.5f;
        return Mathf.Abs(localPos.x) <= brettHalbGroesseLocalXZ.x + buffer &&
               Mathf.Abs(localPos.z) <= brettHalbGroesseLocalXZ.y + buffer;
    }

    // --- Wandkollisionen (Sphere vs AABB) ---
    Vector3 BehandleWandKollisionen(Vector3 oldPos, Vector3 newPos, float rWorld)
    {
        if (waende == null || waende.Length == 0)
            return newPos;

        float3 p = (float3)newPos;
        float rSq = rWorld * rWorld;

        for (int iter = 0; iter < 3; iter++)
        {
            bool hitAny = false;

            foreach (var w in waende)
            {
                if (w == null) continue;

                AABB box = AabbFromTransform(w);

                float distSq = box.DistanceSq(p);
                if (distSq < rSq)
                {
                    hitAny = true;

                    float3 closest = math.clamp(p, box.Min, box.Max);
                    float3 delta = p - closest;

                    float3 n;
                    float lenSq = math.lengthsq(delta);

                    if (lenSq > 1e-8f) n = delta / math.sqrt(lenSq);
                    else
                    {
                        float3 local = p - box.Center;
                        float3 ext = box.Extents;

                        float px = ext.x - math.abs(local.x);
                        float py = ext.y - math.abs(local.y);
                        float pz = ext.z - math.abs(local.z);

                        if (px <= py && px <= pz) n = new float3(math.sign(local.x), 0, 0);
                        else if (py <= px && py <= pz) n = new float3(0, math.sign(local.y), 0);
                        else n = new float3(0, 0, math.sign(local.z));
                    }

                    p = closest + n * rWorld;
                    p += n * 0.0005f;

                    // Slide + Bounce
                    Vector3 n3 = new Vector3(n.x, n.y, n.z);
                    float vN = Vector3.Dot(velocity, n3);
                    if (vN < 0f)
                    {
                        Vector3 vInto = vN * n3;
                        Vector3 vTang = velocity - vInto;
                        Vector3 vBounce = -vInto * rueckprallWand;
                        velocity = vTang + vBounce;
                    }
                }
            }

            if (!hitAny) break;
        }

        return new Vector3(p.x, p.y, p.z);
    }

    Vector3 BehandleBrettKollision(Vector3 pos, float rWorld)
    {
        Ray ray = new Ray(pos + Vector3.up * rWorld, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rWorld * 2f))
        {
            if (hit.transform.IsChildOf(brett))
            {
                pos = hit.point + hit.normal * rWorld;

                float vN = Vector3.Dot(velocity, hit.normal);
                if (vN < 0)
                {
                    Vector3 vNormal = vN * hit.normal;
                    Vector3 vTang = velocity - vNormal;
                    velocity = vTang - vNormal * rueckprallBrett;
                }
            }
        }

        return pos;
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, KugelRadiusWorld);
        }
    }

    public Vector3 getPosition() => position;
    public double getVelocity() => Math.Sqrt(velocity.x * velocity.x + velocity.y * velocity.y + velocity.z * velocity.z);
    public double getAccelleration() => Math.Sqrt(accelleration.x * accelleration.x + accelleration.y * accelleration.y + accelleration.z * accelleration.z);
    public int getTicks() => ticks;
    public float getFhaftValue() => FhaftValue;
    public float getFgleitValue() => FgleitValue;

    static AABB AabbFromTransform(Transform t)
    {
        var rend = t.GetComponent<Renderer>();
        if (rend != null)
        {
            Bounds b = rend.bounds;
            return new AABB { Center = (float3)b.center, Extents = (float3)b.extents };
        }

        return new AABB { Center = (float3)t.position, Extents = new float3(0.5f, 0.5f, 0.5f) };
    }
}
