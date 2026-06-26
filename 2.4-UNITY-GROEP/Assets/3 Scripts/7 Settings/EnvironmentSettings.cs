using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentSettings", menuName = "Settings/Environment")]
public class EnvironmentSettings : ScriptableObject
{
    [Header("Prefab & Layout")]

    [Tooltip("Prefab containing the training area and agents.")]
    public GameObject areaPrefab;

    [Tooltip("Number of areas to spawn along the X axis.")]
    public int rows = 4;

    [Tooltip("Number of areas to spawn along the Z axis.")]
    public int cols = 4;

    [Tooltip("Distance between the centers of adjacent areas.")]
    public float spacing = 20f;
}