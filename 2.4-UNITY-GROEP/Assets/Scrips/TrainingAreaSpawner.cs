using UnityEngine;

public class TrainingAreaSpawner : MonoBehaviour
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

    void Start()
    {
        if (areaPrefab == null)
        {
            Debug.LogError("TrainingAreaSpawner: areaPrefab is not assigned.");
            return;
        }

        SpawnGrid();
    }

    void SpawnGrid()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 pos = new Vector3(r * spacing, 0f, c * spacing);
                Quaternion rot = Quaternion.identity;
                GameObject instance = Instantiate(areaPrefab, pos, rot, transform);

                // Give each instance a unique name for easier debugging
                instance.name = $"{areaPrefab.name}_r{r}_c{c}";
            }
        }
    }
}