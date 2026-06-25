using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Prefab & layout")]
    [Tooltip("Prefab containing the training area and agents")]
    public GameObject areaPrefab;

    [Tooltip("Number of areas along the X axis (rows)")]
    public int rows = 4;

    [Tooltip("Number of areas along the Z axis (cols)")]
    public int cols = 4;

    [Tooltip("Spacing between area centers")]
    public float spacing = 20f;

    [Tooltip("Where in the hierarchy it exists")]
    public Transform spawnHiearchy;

    [Tooltip("The delegate that serves as an example that needs to be hidden at the start")]
    public GameObject ogPrefab;

    public static int totalDelagets = 0;

    private CameraManager cameraManager;

    private void Awake()
    {
        if (areaPrefab == null)
        {
            Debug.LogError("GameManager: areaPrefab is not assigned.");
            return;
        }

        cameraManager = FindFirstObjectByType<CameraManager>();

        SpawnCells();
        DisableOriginal();

        totalDelagets = rows * cols;
    }

    private void SpawnCells()
    {
        Transform parent = spawnHiearchy != null ? spawnHiearchy : transform;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 pos = new Vector3(r * spacing, 0f, c * spacing);

                GameObject instance = Instantiate(areaPrefab, pos, Quaternion.identity, parent);

                instance.name = $"{areaPrefab.name}_r{r}_c{c}";

                Transform focus = instance.transform.Find("CameraFocus");

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

    private void DisableOriginal()
    {
        if (ogPrefab != null)
        {
            ogPrefab.SetActive(false);
        }
    }
}