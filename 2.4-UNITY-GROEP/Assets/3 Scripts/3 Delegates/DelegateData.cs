using System;
using System.Collections.Generic;
using UnityEngine;

public class DelegateData : MonoBehaviour
{
    [Tooltip("How many data points need to be remembered for the succes history.")]
    [SerializeField] private float successRateHistoryLength;

    // current percentage of succeses against failures
    public float successRate = 0;
    // current streak of succes after eachother
    public int successStreak = 0;

    // what the humenoide is currently doing
    public string currentAction = "No Action";

    // what the current product on the conveyor is
    public string currentProduct = "none";

    // a history of succesrated in the length of 
    public List<float> successRates = new List<float>();

    public static event Action OnChange;

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

    public void RegisterSuccess()
    {
        successStreak++;
        OnChange?.Invoke();
    }

    public void RegisterFailure()
    {
        successStreak = 0;
        OnChange?.Invoke();
    }

    public void SetCurrentAction(string action)
    {
        currentAction = action;
        OnChange?.Invoke();
    }

    public void SetCurrentProduct(string product)
    {
        currentProduct = product;
        OnChange?.Invoke();
    }
}