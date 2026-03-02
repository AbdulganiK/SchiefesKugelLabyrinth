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
    [Range(0f, 2f)] public float muHaft = 0.35f;   
    [Range(0f, 2f)] public float muGleit = 0.25f;  
    public float stopSpeed = 0.05f;               

    [Header("Kollision")]
    public Transform[] waende;
    [Range(0f, 1f)] public float rueckprallWand = 0.5f;
    [Range(0f, 1f)] public float rueckprallBrett = 0.0f;
    
    [Header("Kraftpfeile")]
    public Transform xArrow;
    public Transform yArrow;
    public Transform zArrow;

    private Vector3 position;
    private Vector3 velocity;
    private Vector3 accelleration;
    private int ticks = 0;
    public Vector3 fHang;

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

            Vector3 gVec = Vector3.down * gravitation;
            float tiltAngle = Mathf.Acos(Mathf.Clamp01(Vector3.Dot(normal, Vector3.up)));
            float gTangential = gravitation * Mathf.Sin(tiltAngle);
            
            Vector3 tiltDirection = Vector3.Cross(normal, Vector3.up);
            if (tiltDirection.sqrMagnitude > 1e-6f)
                tiltDirection = Vector3.Cross(tiltDirection, normal).normalized;
            else
                tiltDirection = Vector3.ProjectOnPlane(Vector3.forward, normal).normalized;
            
            Vector3 aHang = tiltDirection * gTangential;
            if (invertiereHangkraft) aHang = -aHang;

            Vector3 Fhang = masse * aHang;
            fHang = Fhang;

            Vector3 Freib = ComputeFrictionForce(normal, Fhang, dt);

            accelleration = (Fhang + Freib) / Mathf.Max(0.0001f, masse);

            accelleration = accelleration - Vector3.Dot(accelleration, normal) * normal;

            velocity += accelleration * dt;
        }

        Vector3 newPos = position + velocity * dt;

        newPos = BehandleWandKollisionen(position, newPos, rWorld);

        if (brett != null)
            newPos = BehandleBrettKollision(newPos, rWorld);

        position = newPos;
        transform.position = position;

        transform.rotation = brett.transform.rotation;
        
        Vector3 fHangLocal = brett.InverseTransformDirection(fHang);
        
        xArrow.localPosition = new Vector3(fHangLocal.x, 0f, 0f);
        xArrow.localScale = new Vector3(0.25f, fHangLocal.x, 0.25f);

        yArrow.localPosition = new Vector3(0f, -fHangLocal.y*10000000, 0f);
        yArrow.localScale = new Vector3(0.25f, -fHangLocal.y*10000000, 0.25f);

        zArrow.localPosition = new Vector3(0f, 0f, fHangLocal.z);
        zArrow.localScale = new Vector3(0.25f, fHangLocal.z, 0.25f);
        
        ticks += 1;
    }

    Vector3 ComputeFrictionForce(Vector3 boardNormal, Vector3 Fhang, float dt)
    {
        float tiltAngle = Mathf.Acos(Mathf.Clamp01(Vector3.Dot(boardNormal, Vector3.up)));
        
        float N = masse * gravitation * Mathf.Cos(tiltAngle);

        FhaftValue = muHaft * N;
        FgleitValue = muGleit * N;

        Vector3 vT = velocity - Vector3.Dot(velocity, boardNormal) * boardNormal;
        float speed = vT.magnitude;

        Vector3 FhangT = Fhang - Vector3.Dot(Fhang, boardNormal) * boardNormal;
        float FhangMag = FhangT.magnitude;

        if (speed < stopSpeed && FhangMag < FhaftValue)
        {
            
            Vector3 Fhaft = -FhangT;

           
            Vector3 Fstop = -vT * (masse / Mathf.Max(1e-6f, dt));

            float FstopMax = 5f * N;
            if (Fstop.sqrMagnitude > FstopMax * FstopMax)
                Fstop = Fstop.normalized * FstopMax;

            return Fhaft + Fstop;
        }

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
