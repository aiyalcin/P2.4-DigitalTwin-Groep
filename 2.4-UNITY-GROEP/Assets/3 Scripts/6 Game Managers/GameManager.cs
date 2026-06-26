using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Environment")]

    [Tooltip("Settings for the environment.")]
    [SerializeField] private EnvironmentSettings environmentSettings;

    [Tooltip("The parent object the delegates are spawned under.")]
    public Transform spawnHierarchy;

    [Tooltip("The camera manager needed to notify the camera for the locations of the delegates to view.")]
    [SerializeField] private CameraManager cameraManager;

    [HideInInspector]
    public static int totalDelagets = 0; // The total number of delegates (factory cells) spawned in the scene.

    private void Awake()
    {
        if (environmentSettings.areaPrefab == null)
        {
            Debug.LogError("GameManager: areaPrefab is not assigned.");
            return;
        }

        cameraManager = FindFirstObjectByType<CameraManager>();

        SpawnCells();

        totalDelagets = environmentSettings.rows * environmentSettings.cols;
    }

    /// <summary>
    /// Spawns a grid of factory cells based on the configured environment settings.
    /// Each spawned cell is assigned a unique name and its CameraFocus transform,
    /// if present, is registered with the CameraManager.
    /// </summary>
    private void SpawnCells()
    {
        Transform parent = spawnHierarchy != null ? spawnHierarchy : transform;

        for (int r = 0; r < environmentSettings.rows; r++)
        {
            for (int c = 0; c < environmentSettings.cols; c++)
            {
                Vector3 pos = new Vector3(
                    r * environmentSettings.spacing,
                    0f,
                    c * environmentSettings.spacing);

                GameObject instance = Instantiate(
                    environmentSettings.areaPrefab,
                    pos,
                    Quaternion.identity,
                    parent);

                instance.name = $"{environmentSettings.areaPrefab.name}_r{r}_c{c}";

                Transform focusParent = instance.transform.Find("SYSTEMS");
                Transform focus = focusParent.transform.Find("CameraFocus");

                if (focus != null)
                {
                    if (cameraManager != null)
                    {
                        cameraManager.RegisterFactoryFocus(focus);
                    }
                }
                else
                {
                    Debug.LogWarning($"{instance.name} has no CameraFocus child.");
                }
            }
        }
    }
}