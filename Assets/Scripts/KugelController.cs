using UnityEngine;

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
    
    [Header("Kollision")]
    public Transform[] waende;
    [Range(0f, 1f)] public float rueckprallWand = 0.0f;
    [Range(0f, 1f)] public float rueckprallBrett = 0.0f;

    private Vector3 position;
    private Vector3 velocity;
    
    // World-Radius mit Skalierung
    private float KugelRadiusWorld {
        get {
            Vector3 scale = transform.lossyScale;
            return kugelRadius * Mathf.Max(scale.x, scale.y, scale.z);
        }
    }

    void Start()
    {
        position = transform.position;
        velocity = Vector3.zero;
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        float rWorld = KugelRadiusWorld;

        // Gravitation
        velocity.y += -gravitation * dt;

        // Bewegung auf Brett
        if (brett != null && IsOverBoard(position, rWorld))
        {
            // Winkel des Bretts (vereinfacht)
            Vector3 euler = brett.eulerAngles;
            float winkelX = NormalisiereWinkel(euler.x);
            float winkelZ = NormalisiereWinkel(euler.z);
            
            // Hangabtriebskraft
            Vector2 Fhang = BerechneHangabtriebskraft(winkelX, winkelZ);
            if (invertiereHangkraft) Fhang = -Fhang;
            
            // Auf X/Z anwenden
            Vector2 vXZ = new Vector2(velocity.x, velocity.z);
            Vector2 aXZ = Fhang / masse;
            vXZ += aXZ * dt;
            
            velocity.x = vXZ.x;
            velocity.z = vXZ.y;
        }

        // Vorhersage der neuen Position
        Vector3 newPos = position + velocity * dt;
        
        // Kollisionserkennung für Wände
        newPos = BehandleWandKollisionen(position, newPos, rWorld);
        
        // Kollision mit Brett
        if (brett != null)
        {
            newPos = BehandleBrettKollision(newPos, rWorld);
        }

        // Geschwindigkeit basierend auf tatsächlicher Bewegung aktualisieren
        velocity = (newPos - position) / dt;
        position = newPos;
        
        // Anwenden
        transform.position = position;
    }

    bool IsOverBoard(Vector3 worldPos, float rWorld)
    {
        Vector3 localPos = brett.InverseTransformPoint(worldPos);
        
        // Mit Radius puffern
        float buffer = rWorld * 0.5f;
        return Mathf.Abs(localPos.x) <= brettHalbGroesseLocalXZ.x + buffer &&
               Mathf.Abs(localPos.z) <= brettHalbGroesseLocalXZ.y + buffer;
    }

    Vector2 BerechneHangabtriebskraft(float winkelX, float winkelZ)
    {
        float Fx = masse * gravitation * Mathf.Sin(winkelZ * Mathf.Deg2Rad);
        float Fz = masse * gravitation * Mathf.Sin(winkelX * Mathf.Deg2Rad);
        return new Vector2(Fx, Fz);
    }

    Vector3 BehandleWandKollisionen(Vector3 oldPos, Vector3 newPos, float rWorld)
    {
        if (waende == null || waende.Length == 0)
            return newPos;

        Vector3 rayDir = (newPos - oldPos).normalized;
        float distance = Vector3.Distance(oldPos, newPos);
        
        // Raycast von alter zu neuer Position
        RaycastHit[] hits = Physics.SphereCastAll(
            oldPos, rWorld, rayDir, distance + rWorld
        );

        foreach (RaycastHit hit in hits)
        {
            // Prüfen ob es eine unserer Wände ist
            bool isWall = false;
            foreach (Transform wand in waende)
            {
                if (wand != null && hit.transform.IsChildOf(wand))
                {
                    isWall = true;
                    break;
                }
            }
            
            if (!isWall) continue;

            // Normale berechnen
            Vector3 normal = hit.normal;
            
            // Position korrigieren
            float penetration = rWorld - hit.distance;
            if (penetration > 0)
            {
                newPos += normal * penetration;
            }
            
            // Geschwindigkeit reflektieren
            float vN = Vector3.Dot(velocity, normal);
            if (vN < 0)
            {
                velocity = Vector3.Reflect(velocity, normal) * rueckprallWand;
            }
        }

        return newPos;
    }

    Vector3 BehandleBrettKollision(Vector3 pos, float rWorld)
    {
        // Strahl nach unten zum Brett
        Ray ray = new Ray(pos + Vector3.up * rWorld, Vector3.down);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, rWorld * 2f))
        {
            if (hit.transform.IsChildOf(brett))
            {
                // Kugel auf Oberfläche positionieren
                pos = hit.point + hit.normal * rWorld;
                
                // Geschwindigkeit anpassen
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

    float NormalisiereWinkel(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    // Hilfsmethode für visuelle Debugging
    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, KugelRadiusWorld);
        }
    }
}