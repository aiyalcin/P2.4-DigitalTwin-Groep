using System;
using UnityEngine;
using TMPro;

public class DelegateUI : MonoBehaviour
{
    [Tooltip("Text field displaying the current success rate percentage.")]
    [SerializeField] private TextMeshProUGUI succesRateText;

    [Tooltip("Text field displaying the current success streak.")]
    [SerializeField] private TextMeshProUGUI succesStreakText;

    [Tooltip("Text field displaying the agent's current action.")]
    [SerializeField] private TextMeshProUGUI currentActionText;

    [Tooltip("Text field displaying the current product type on the conveyor.")]
    [SerializeField] private TextMeshProUGUI currentProductText;

    [Tooltip("The GameObject holding the DelegateData component to display. Can be set via CameraManager at runtime.")]
    [SerializeField] private GameObject currentDelegate;

    private void OnEnable()
    {
        DelegateData.OnChange += Change;
    }

    private void OnDisable()
    {
        DelegateData.OnChange -= Change;
    }

    /// <summary>
    /// Called when DelegateData.OnChange fires. Triggers a UI refresh.
    /// </summary>
    private void Change()
    {
        UpdateUI();
    }

    /// <summary>
    /// Sets the active delegate whose data is displayed and immediately refreshes the UI.
    /// Called by CameraManager when the camera switches to a different factory cell.
    /// </summary>
    /// <param name="gameObject">The factory cell GameObject containing a DelegateData component.</param>
    public void UpdateCurrentDelegate(GameObject gameObject)
    {
        currentDelegate = gameObject;
        UpdateUI();
    }

    /// <summary>
    /// Reads all tracked values from the current delegate's DelegateData and updates all text fields.
    /// Does nothing if no delegate is assigned or if it has no DelegateData component.
    /// </summary>
    public void UpdateUI()
    {
        if (currentDelegate == null) { return; }
        DelegateData data = currentDelegate.GetComponent<DelegateData>();
        if (data == null) { return; }

        succesRateText.text = data.successRate + "%";
        succesStreakText.text = data.successStreak.ToString();
        currentActionText.text = data.currentAction;
        currentProductText.text = data.currentProduct;
    }
}