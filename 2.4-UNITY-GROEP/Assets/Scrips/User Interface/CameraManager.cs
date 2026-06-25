using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Transform birdView;
    public List<Transform> factoryPoints = new List<Transform>();

    public CinemachineCamera cam;
    public CinemachineFollow followCam1;
    public CinemachineRotationComposer followCam2;

    public GameObject detailsButton;
    public GameObject detailsPanel;
    public TextMeshProUGUI delegateInformation;

    public DelegateUI delegateUI;

    private int currentIndex = 0;

    private void Start()
    {
        if (birdView != null)
        {
            factoryPoints.Insert(0, birdView);
        }

        FocusCurrent();
    }

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

            delegateUI.UpdateCurrentDelegate(target.parent.gameObject);
        }
    }

    public void TogglePanel()
    {
        detailsPanel.SetActive(!detailsPanel.activeSelf);
    }

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

    public void ShowFactoryByIndex(int index)
    {
        if (index < 0)
        {
            return;
        }

        if (index >= factoryPoints.Count)
        {
            return;
        }

        currentIndex = index;
        FocusCurrent();
    }

    public void RegisterFactoryFocus(Transform focus)
    {
        if (focus == null)
        {
            return;
        }

        factoryPoints.Add(focus);
    }
}