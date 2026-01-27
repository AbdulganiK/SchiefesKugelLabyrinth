/// <summary>
/// Minimaler Board-Controller: kippt das Board um X (vorne/hinten) und Z (links/rechts)
/// entsprechend der Eingabe (WASD/Pfeiltasten).
/// </summary>
using UnityEngine;

[DisallowMultipleComponent] //Darf nur einmal angewendet werden
public class BoardController : MonoBehaviour
{
    [Header("Parameter")]
    [Tooltip("Maximaler Kippwinkel in Grad (je Achse X/Z).")]
    [Range(1f, 90f)]
    public float maxTiltDegrees = 90f;

    [Tooltip("Drehgeschwindigkeit (Grad pro Sekunde).")]
    [Min(1f)]
    public float rotationSpeedDegPerSec = 120f; // höherer Default, damit es bei 60 TPS nicht träge wirkt

    [Tooltip("Skalierung der Eingaben (Horizontal/Z und Vertical/X).")]
    public float horizontalScale = 1f;
    public float verticalScale = 1f;

    [Header("Input")]
    [Tooltip("Raw = direkter Input ohne Glättung.")]
    public bool useRawInput = true;

    [Tooltip("Nichtlineare Verstärkung: 1 = linear, >1 = sensibel bei kleinen Werten, schneller bei großen.")]
    [Range(1f, 3f)]
    public float inputExpo = 1.4f;

    [Tooltip("Zusätzlicher Faktor, um die Reaktionsgeschwindigkeit unabhängig vom Winkel zu erhöhen.")]
    [Range(0.5f, 4f)]
    public float speedMultiplier = 1.0f;

    // interner Zustand
    private Vector2 currentRotation;   // xRot (vorne/hinten), zRot (links/rechts)
    private Quaternion baseRotation;   // Startrotation als Offset

    private void Awake()
    {
        baseRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        // Sicherstellen, dass TickManager existiert
        if (TickManager.Instance != null)
            TickManager.Instance.OnTick += HandleTick;
    }

    private void OnDisable()
    {
        if (TickManager.Instance != null)
            TickManager.Instance.OnTick -= HandleTick;
    }

    public void ResetBoard()
    {
        // interne Rotationswerte zurücksetzen
        currentRotation = Vector2.zero;

        // Brett in Ausgangsrotation versetzen
        transform.localRotation = baseRotation;
    }

    private void HandleTick()
    {
        //Tick-basiertes berechnen
        float tickDt = 1f / Mathf.Max(1, TickManager.Instance.ticksPerSecond);

        // Eingaben holen
        float h = (useRawInput ? Input.GetAxisRaw("Horizontal") : Input.GetAxis("Horizontal")) * horizontalScale;
        float v = (useRawInput ? Input.GetAxisRaw("Vertical") : Input.GetAxis("Vertical")) * verticalScale;

        // Optionale exponentielle Kurve für bessere Steuerbarkeit:
        // kleine Werte feinfühlig, große Werte stärker
        h = ApplyExpo(h, inputExpo);
        v = ApplyExpo(v, inputExpo);

        // Zielwinkel
        float targetX = v * maxTiltDegrees;   // X (vorne/hinten) folgt Vertical
        float targetZ = (-h) * maxTiltDegrees; // Z (links/rechts) folgt Horizontal (invertiert intuitiver)

        // Schritt in Grad pro Tick
        float step = rotationSpeedDegPerSec * tickDt * speedMultiplier;

        // Clamped, geschmeidige Annäherung je Tick
        currentRotation.x = MoveTowardsAngle(currentRotation.x, targetX, step);
        currentRotation.y = MoveTowardsAngle(currentRotation.y, targetZ, step);

        // Sicherheits-Clamp (numerische Stabilität)
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

    private static float ApplyExpo(float v, float expo)
    {
        if (expo <= 1f) return v;
        float sign = Mathf.Sign(v);
        float abs = Mathf.Abs(v);
        return sign * Mathf.Pow(abs, expo);
    }

    public float getRotationX() => currentRotation.x;
    public float getRotationY() => currentRotation.y;
}
