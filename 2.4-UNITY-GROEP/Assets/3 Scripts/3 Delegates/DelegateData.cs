using System;
using System.Collections.Generic;
using UnityEngine;

public class DelegateData : MonoBehaviour
{
    [Tooltip("How many data points are stored in the success rate history before old ones are removed.")]
    [SerializeField] private float successRateHistoryLength;

    /// <summary>Current percentage of correct deliveries against total deliveries.</summary>
    [HideInInspector] public float successRate = 0;

    /// <summary>Number of consecutive correct deliveries since the last failure.</summary>
    [HideInInspector] public int successStreak = 0;

    /// <summary>Label describing what the agent is currently doing.</summary>
    [HideInInspector] public string currentAction = "No Action";

    /// <summary>Label describing the product type currently at the front of the conveyor.</summary>
    [HideInInspector] public string currentProduct = "none";

    /// <summary>Rolling history of success rates capped at successRateHistoryLength entries.</summary>
    private List<float> successRates = new List<float>();

    /// <summary>Fired whenever any tracked value changes. Subscribe to keep the UI in sync.</summary>
    public static event Action OnChange;

    /// <summary>
    /// Updates the success rate, appends it to the history and trims the oldest entry if the history exceeds its maximum length.
    /// </summary>
    /// <param name="newSuccessRate">The new success rate to store, expressed as a percentage.</param>
    public void UpdateSuccessRate(float newSuccessRate)
    {
        successRate = newSuccessRate;
        successRates.Add(newSuccessRate);
        if (successRates.Count > successRateHistoryLength)
        {
            successRates.RemoveAt(0);
        }
        OnChange?.Invoke();
    }

    /// <summary>
    /// Increments the success streak by one and notifies listeners.
    /// </summary>
    public void RegisterSuccess()
    {
        successStreak++;
        OnChange?.Invoke();
    }

    /// <summary>
    /// Resets the success streak to zero and notifies listeners.
    /// </summary>
    public void RegisterFailure()
    {
        successStreak = 0;
        OnChange?.Invoke();
    }

    /// <summary>
    /// Updates the current action label and notifies listeners.
    /// </summary>
    /// <param name="action">A string describing the agent's current state, e.g. "SearchingForBox".</param>
    public void SetCurrentAction(string action)
    {
        currentAction = action;
        OnChange?.Invoke();
    }

    /// <summary>
    /// Updates the current product label and notifies listeners.
    /// </summary>
    /// <param name="product">A string representing the product type currently at the front of the conveyor.</param>
    public void SetCurrentProduct(string product)
    {
        currentProduct = product;
        OnChange?.Invoke();
    }
}