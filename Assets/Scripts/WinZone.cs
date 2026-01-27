using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class WinZone : MonoBehaviour
{
    [Header("Erkennung (optional)")]
    [SerializeField] private string ballTag = "PlayerBall"; // optional

    [Header("Referenzen")]
    [SerializeField] private ResetController resetController; // GameManager → Reset Controller hier reinziehen

    private Collider _col;
    private Rigidbody _rb;


    private void Reset()
    {
        // Wird automatisch aufgerufen, wenn du das Script hinzufügst
        _col = GetComponent<Collider>();
        if (_col != null) _col.isTrigger = true;

        _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }
    }

    private void Awake()
    {
        // Referenzen sicherstellen (falls im Inspector nicht gesetzt)
        _col = GetComponent<Collider>();
        _rb = GetComponent<Rigidbody>();

        if (_col != null) _col.isTrigger = true;
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
            _rb.interpolation = RigidbodyInterpolation.None;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var ball = other.GetComponentInParent<KugelController>();
        if (ball != null)
        {
            Debug.Log("<color=#00FF00><b><size=20>[WIN] Gewonnen!</size></b></color>");
            resetController?.ResetAll();
        }
    }
}