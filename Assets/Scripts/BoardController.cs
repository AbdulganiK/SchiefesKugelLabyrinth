using UnityEngine;


/// <summary>
/// Minimaler Board-Controller: kippt das Board um X (vorne/hinten) und Z (links/rechts)
/// entsprechend der Eingabe (WASD/Pfeiltasten).
/// </summary>
[DisallowMultipleComponent] //Darf nur einmal angewendet werden
public class BoardController : MonoBehaviour
{
    [Header("Parameter")]
    [Tooltip("Maximaler Kippwinkel in Grad (je Achse X/Z).")]
    [Range(1f, 45f)]
    public float maxTiltDegrees = 45f;

    [Tooltip("Drehgeschwindigkeit (Grad pro Sekunde).")]
    [Min(1f)]
    public float rotationSpeedDegPerSec = 3f;

    [Tooltip("Skalierung der Eingaben (Horizontal/Z und Vertical/X).")]
    public float horizontalScale = 1f;
    public float verticalScale = 1f;

    // interner Zustand
    private Vector2 currentRotation;   // xRot (vorne/hinten), zRot (links/rechts)
    private Quaternion baseRotation; // Startrotation als Offset

    void Awake()
    {
        baseRotation = transform.localRotation;
    }

    void Update()
    {
        // Eingaben: Horizontal = A/D oder Pfeil Links/Rechts; Vertical = W/S oder Pfeil Hoch/Runter
        float h = Input.GetAxis("Horizontal") * horizontalScale;
        float v = Input.GetAxis("Vertical") * verticalScale;

        // Zielwinkeländerung pro Frame (gleichmäßig mit deltaTime)
        float step = rotationSpeedDegPerSec * Time.deltaTime;

        // X (vorne/hinten) folgt Vertical v
        currentRotation.x = MoveTowardsAngle(currentRotation.x, v * maxTiltDegrees, step);

        // Z (links/rechts) folgt Horizontal h
        // Negatives Vorzeichen macht die Steuerung meist intuitiver: rechts kippen bei positivem h
        float targetZ = (-h) * maxTiltDegrees;
        currentRotation.y = MoveTowardsAngle(currentRotation.y, targetZ, step);

        // Sicherheits-Clamp
        currentRotation.x = Mathf.Clamp(currentRotation.x, -maxTiltDegrees, +maxTiltDegrees);
        currentRotation.y = Mathf.Clamp(currentRotation.y, -maxTiltDegrees, +maxTiltDegrees);

        // Rotation anwenden: X und Z kippen, Y bleibt unverändert
        Quaternion localTilt = Quaternion.Euler(currentRotation.x, 0f, currentRotation.y);
        transform.localRotation = baseRotation * localTilt;
    }

    private static float MoveTowardsAngle(float current, float target, float maxDelta)
    {
        if (Mathf.Approximately(current, target)) return target;
        return (current < target)
            ? Mathf.Min(current + maxDelta, target)
            : Mathf.Max(current - maxDelta, target);
    }

    public float getRotationX()
    {
        return currentRotation.x;
    }

    public float getRotationY()
    {
        return currentRotation.y;
    }
}
