using UnityEngine;
using System;

public class TickManager : MonoBehaviour
{

    public static TickManager Instance { get; private set; }

    [Header("Tick Settings")]
    public int ticksPerSecond = 60;

    public GameObject UIDocument;
    private UI UIController;

    private float tickInterval;
    private float tickTimer;
    private bool paused = false;

    public event Action OnTick;

    private void Awake()
    {
        Instance = this;
        tickInterval = 1f / ticksPerSecond;
        UIController = UIDocument.GetComponent<UI>();
    }

    private void Update()
    {
        if (paused) return;

        tickTimer += Time.deltaTime;

        while (tickTimer >= tickInterval)
        {
            tickTimer -= tickInterval;
            OnTick?.Invoke();
        }
    }

    public void Pause() => paused = true;
    public void Resume() => paused = false;
    public void TogglePause()
    {
        paused = !paused;
        UIController.setLogText("Simulation " + (paused ? "pausiert" : "gestartet"));
    }

    public bool IsPaused() => paused;
}
