using UnityEngine;
using UnityEngine.Events;

public class WinZone : MonoBehaviour
{
    [Header("Erkennung")]
    [SerializeField] private string ballTag = "PlayerBall";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(ballTag))
        {
            HandleWin();
        }
    }

    private void HandleWin()
    {
        Debug.Log("[WinZone] Gewonnen!");
    }
}

