using UnityEngine;
using System;

public class TickManager : MonoBehaviour
{

    public static TickManager Instance { get; private set; }

    [Header("Tick Settings")]
    public int ticksPerSecond = 60;

    private float tickInterval;
    private float tickTimer;
    private bool paused = false;

    public event Action OnTick;

    private void Awake()
    {
        Instance = this;
        tickInterval = 1f / ticksPerSecond;
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
    public void TogglePause() => paused = !paused;

    public bool IsPaused() => paused;
}
