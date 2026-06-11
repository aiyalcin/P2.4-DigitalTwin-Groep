using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// General settings for conveyor-based production systems
/// </summary>
[CreateAssetMenu(fileName = "ProductionInformation", menuName = "Scriptable Objects/ProductionInformation")]
public class ProductionInformation : ScriptableObject
{
    [Header("Conveyor Information")]

    [Tooltip("Local position representing the final destination.")]
    public Vector3 destination;

    [Tooltip("Local position representing the spawn destination.")]
    public Vector3 spawnPosition;

    [Tooltip("Maximum number of products that can exist on the conveyor at the same time.")]
    public int slotCount;

    [Tooltip("Movement speed at which products travel along the conveyor.")]
    public float speed;

    [Tooltip("Distance between each product slot on the conveyor.")]
    public float slotDistance;

    [Header("Product Information")]

    [Tooltip("Total number of products to be generated in a production cycle.")]
    public int productCount;

    [Tooltip("List of possible product prefabs that can be randomly selected for spawning.")]
    public List<GameObject> productOptions = new List<GameObject>();

    [Tooltip("If enabled, a single shared product list is used instead of individual generation per delegate.")]
    public bool centralized;

    [Tooltip("Pre-generated central product list used when centralized mode is enabled.")]
    public List<GameObject> productList = new List<GameObject>();

    private void OnEnable()
    {
        if (centralized)
        {
            for (int i = 0; i < productCount; i++)
            {
                int ran = Random.Range(0, productOptions.Count);
                productList.Add(productOptions[ran]);
            }
        }

        spawnPosition = destination - new Vector3(0, 0, (slotCount + 1) * slotDistance);
    }
}