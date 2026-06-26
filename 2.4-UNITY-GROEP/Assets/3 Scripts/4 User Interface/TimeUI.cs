using System;
using TMPro;
using UnityEngine;

public class TimeUI : MonoBehaviour
{
    [Tooltip("Text field displaying the elapsed time since the session started.")]
    [SerializeField] private TextMeshProUGUI time;

    private float startTime;

    private void Awake()
    {
        startTime = Time.unscaledTime;
    }

    /// <summary>
    /// Updates the time display every frame. Uses unscaled time so the clock
    /// remains accurate even when Time.timeScale is raised during training.
    /// </summary>
    private void Update()
    {
        float elapsed = Time.unscaledTime - startTime;
        TimeSpan ts = TimeSpan.FromSeconds(elapsed);
        time.text = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}:{ts.Milliseconds:000}";
    }
}