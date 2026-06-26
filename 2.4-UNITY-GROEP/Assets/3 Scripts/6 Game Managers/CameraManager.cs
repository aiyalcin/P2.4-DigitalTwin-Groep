using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Camera Targets")]

    [Tooltip("Reference transform for the global bird's-eye overview position.")]
    [SerializeField] private Transform birdView;

    [Tooltip("All focus points the camera can cycle through (factory delegates).")]
    [SerializeField] private List<Transform> factoryPoints = new();

    [Header("Cinemachine Components")]

    [Tooltip("Main Cinemachine camera used for all transitions.")]
    [SerializeField] private CinemachineCamera cam;

    [Tooltip("Follow offset controller for camera positioning.")]
    [SerializeField] private CinemachineFollow followCam1;

    [Tooltip("Rotation composer controlling target framing.")]
    [SerializeField] private CinemachineRotationComposer followCam2;

    [Header("UI References")]

    [Tooltip("Button used to toggle delegate details panel.")]
    [SerializeField] private GameObject detailsButton;

    [Tooltip("Panel displaying delegate-related information.")]
    [SerializeField] private GameObject detailsPanel;

    [Tooltip("Text field showing current delegate or overview label.")]
    [SerializeField] private TextMeshProUGUI delegateInformation;

    [Tooltip("External UI controller handling delegate-specific updates.")]
    [SerializeField] private DelegateUI delegateUI;

    // Current index of the active camera focus target
    private int currentIndex = 0;

    private void Start()
    {
        if (birdView != null)
        {
            factoryPoints.Insert(0, birdView);
        }

        FocusCurrent();
    }

    /// <summary>
    /// Applies cinemachine logic for next focus point + updates UI
    /// </summary>
    public void FocusCurrent()
    {
        if (factoryPoints.Count == 0)
        {
            return;
        }

        Transform target = factoryPoints[currentIndex];

        cam.Follow = target;
        cam.LookAt = target;

        if (followCam1 != null)
        {
            if (currentIndex == 0)
            {
                followCam1.FollowOffset = new Vector3(0f, 90f, 0f);
            }
            else
            {
                followCam1.FollowOffset = new Vector3(0f, 20f, -25f);
            }
        }

        if (followCam2 != null)
        {
            if (currentIndex == 0)
            {
                followCam2.TargetOffset = Vector3.zero;
            }
            else
            {
                followCam2.TargetOffset = new Vector3(0f, 1.5f, 0f);
            }
        }

        if (currentIndex == 0)
        {
            detailsButton.SetActive(false);
            detailsPanel.SetActive(false);
            delegateInformation.text = "General Overview";

            cam.Lens.FieldOfView = 90f;

        }
        else
        {
            detailsButton.SetActive(true);
            detailsPanel.SetActive(true);
            delegateInformation.text = "Delegate: " + currentIndex.ToString();

            cam.Lens.FieldOfView = 35f;

            delegateUI.UpdateCurrentDelegate(target.parent.parent.gameObject);
        }
    }

    /// <summary>
    /// Toggles details panel for delegate
    /// </summary>
    public void TogglePanel()
    {
        detailsPanel.SetActive(!detailsPanel.activeSelf);
    }


    /// <summary>
    /// Goes to the next focus point of the camera
    /// </summary>
    public void NextIndex()
    {
        if (factoryPoints.Count == 0)
        {
            return;
        }

        currentIndex = currentIndex + 1;

        if (currentIndex >= factoryPoints.Count)
        {
            currentIndex = 0;
        }

        FocusCurrent();
    }

    /// <summary>
    /// Goes to the previous focus point of the camera
    /// </summary>
    public void PrevIndex()
    {
        if (factoryPoints.Count == 0)
        {
            return;
        }

        currentIndex = currentIndex - 1;

        if (currentIndex < 0)
        {
            currentIndex = factoryPoints.Count - 1;
        }

        FocusCurrent();
    }


    /// <summary>
    /// Registers the focus points for the camera focusses
    /// </summary>
    /// <param name="focus">The transfoms of the focus points</param>
    public void RegisterFactoryFocus(Transform focus)
    {
        if (focus == null)
        {
            return;
        }

        factoryPoints.Add(focus);
    }
}