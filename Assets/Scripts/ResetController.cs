using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class ResetController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Ball/Kugel-Controller. Wird automatisch gesucht, wenn leer.")]
    public KugelController kugel;

    [Tooltip("Brett-Controller. Wird automatisch gesucht, wenn leer.")]
    public BoardController brett;

    [Header("Reset-Optionen")]
    [Tooltip("TickManager beim Reset kurz pausieren (empfohlen).")]
    public bool pauseTickManagerDuringReset = true;

    [Tooltip("Kugel auf diese Position setzen.")]
    public Vector3 ballSpawnPosition = new Vector3(-4.5f, 0.5f, 4.5f);

    [Tooltip("Brett auf die gespeicherte BaseRotation (Startrotation) zurücksetzen.")]
    public bool resetBoardToBaseRotation = true;

    [Tooltip("Falls nicht BaseRotation: Brett auf Quaternion.identity setzen (komplett flach).")]
    public bool useIdentityIfNotBase = false;

    [Header("Hotkey")]
    [Tooltip("Taste für Reset. Optional.")]
    public KeyCode resetKey = KeyCode.R;

    [Header("Events")]
    public UnityEvent OnBeforeReset;
    public UnityEvent OnAfterReset;

    private void Awake()
    {
        // Wenn nicht im Inspector gesetzt, automatisch finden
        if (kugel == null) kugel = FindObjectOfType<KugelController>();
        if (brett == null) brett = FindObjectOfType<BoardController>();
    }

    private void Update()
    {
        if (resetKey != KeyCode.None && Input.GetKeyDown(resetKey))
        {
            ResetAll();
        }
    }

    /// <summary>
    /// Setzt Kugel und Brett in einem Schritt zurück.
    /// </summary>
    public void ResetAll()
    {
        OnBeforeReset?.Invoke();

        bool wasPaused = false;
        var tm = TickManager.Instance;

       
        if (pauseTickManagerDuringReset && tm != null && !tm.IsPaused())
        {
            tm.Pause();
            wasPaused = true;
        }

        // --- Kugel zurücksetzen ---
        if (kugel != null)
        {
            kugel.transform.position = ballSpawnPosition;

            
            var resetMethod = typeof(KugelController).GetMethod("ResetBall");
            if (resetMethod != null)
            {
                resetMethod.Invoke(kugel, null);
            }
            else
            {
            }
        }
        else
        {
            Debug.LogWarning("[ResetController] Kein KugelController gefunden.");
        }

        // Brett zurücksetzen
        if (brett != null)
        {
            if (resetBoardToBaseRotation)
            {
                var resetBoardMethod = typeof(BoardController).GetMethod("ResetBoard");
                if (resetBoardMethod != null)
                {
                    resetBoardMethod.Invoke(brett, null);
                }
                else
                {
                    brett.transform.localRotation = Quaternion.identity;
                }
            }
            else
            {
                brett.transform.localRotation = useIdentityIfNotBase ? Quaternion.identity : brett.transform.localRotation;
            }
        }
        else
        {
            Debug.LogWarning("[ResetController] Kein BoardController gefunden.");
        }

        if (pauseTickManagerDuringReset && tm != null && wasPaused)
        {
            tm.Resume();
        }

        OnAfterReset?.Invoke();
    }
}

