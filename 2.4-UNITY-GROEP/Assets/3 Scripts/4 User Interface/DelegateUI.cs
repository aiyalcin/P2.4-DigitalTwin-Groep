using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DelegateUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI succesRateText;
    [SerializeField] private TextMeshProUGUI succesStreakText;

    [SerializeField] private TextMeshProUGUI currentActionText;
    [SerializeField] private TextMeshProUGUI currentProductText;

    [SerializeField] private GameObject currentDelegate;


    private void OnEnable()
    {
        DelegateData.OnChange += Change;
    }

    private void OnDisable()
    {
        DelegateData.OnChange -= Change;
    }

    private void Change()
    {
        UpdateUI();
    }

    public void UpdateCurrentDelegate(GameObject gameObject)
    {
        currentDelegate = gameObject;
        UpdateUI();
    }

    public void UpdateUI()
    {
        DelegateData data = currentDelegate.GetComponent<DelegateData>();

        if(data == null ) { return; }

        succesRateText.text = data.successRate.ToString() + "%";
        succesStreakText.text = data.successStreak.ToString();

        currentActionText.text = data.currentAction.ToString();
        currentProductText.text = data.currentProduct.ToString();
    }
}
