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

    [Tooltip("Where in te hiearchy it exists")]
    public Transform spawnHiearchy;

    [Tooltip("The delegate that serves as an example that needs to be hidden at the start")]
    public GameObject ogPrefab;

    public static int totalDelagets = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (areaPrefab == null)
        {
            Debug.LogError("TrainingAreaSpawner: areaPrefab is not assigned.");
            return;
        }

        SpawnCells();
        DisableOriginal();

        totalDelagets = rows * cols;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SpawnCells()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 pos = new Vector3(r * spacing, 0f, c * spacing);
                Quaternion rot = Quaternion.identity;

                Transform parent = spawnHiearchy != null ? spawnHiearchy : transform;

                GameObject instance = Instantiate(areaPrefab, pos, rot, parent);

                instance.name = $"{areaPrefab.name}_r{r}_c{c}";
            }
        }
    }

    void DisableOriginal()
    {
        ogPrefab.SetActive(false);
    }
}
