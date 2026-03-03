using UnityEngine;

[DisallowMultipleComponent]
public class BoardController : MonoBehaviour
{
    [Header("Parameter")]
    [Tooltip("Maximaler Kippwinkel in Grad (je Achse X/Z).")]
    [Range(1f, 90f)]
    public float maxTiltDegrees = 90f;

    [Tooltip("Drehgeschwindigkeit (Grad pro Sekunde).")]
    [Min(1f)]
    public float rotationSpeedDegPerSec = 120f;

    public float horizontalScale = 1f;
    public float verticalScale = 1f;

    [Header("Input")]
    public bool useRawInput = true;

    [Range(1f, 3f)]
    public float inputExpo = 1.4f;

    [Range(0.5f, 4f)]
    public float speedMultiplier = 1.0f;

    private Vector2 currentRotation;
    private Quaternion baseRotation; 

    private void Awake()
    {
        baseRotation = transform.localRotation;
    }

    private void OnEnable()
    {
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
        currentRotation = Vector2.zero;
        transform.localRotation = baseRotation;
    }

    private void HandleTick()
    {
        float tickDt = 1f / Mathf.Max(1, TickManager.Instance.ticksPerSecond);

        //Eingabe
        float h = (useRawInput ? Input.GetAxisRaw("Horizontal") : Input.GetAxis("Horizontal")) * horizontalScale;
        float v = (useRawInput ? Input.GetAxisRaw("Vertical") : Input.GetAxis("Vertical")) * verticalScale;

        h = ApplyExpo(h, inputExpo);
        v = ApplyExpo(v, inputExpo);

        // Zielwinkel
        float targetX = v * maxTiltDegrees;   
        float targetZ = (-h) * maxTiltDegrees; 

        // Schritt in Grad pro Tick
        float step = rotationSpeedDegPerSec * tickDt * speedMultiplier;

        currentRotation.x = MoveTowardsAngle(currentRotation.x, targetX, step);
        currentRotation.y = MoveTowardsAngle(currentRotation.y, targetZ, step);

        currentRotation.x = Mathf.Clamp(currentRotation.x, -maxTiltDegrees, +maxTiltDegrees);
        currentRotation.y = Mathf.Clamp(currentRotation.y, -maxTiltDegrees, +maxTiltDegrees);

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
