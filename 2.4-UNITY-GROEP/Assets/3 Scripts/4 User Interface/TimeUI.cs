using System;
using TMPro;
using UnityEngine;

public class TimeUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI time;

    private float startTime;

    private void Awake()
    {
        startTime = Time.unscaledTime;
    }

    private void Update()
    {
        float elapsed = Time.unscaledTime - startTime;

        TimeSpan ts = TimeSpan.FromSeconds(elapsed);

        time.text = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}:{ts.Milliseconds:000}";
    }
}