using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UIElements;

public class ConveyorLogic : MonoBehaviour
{
    [Tooltip("Reference to the central status tracker for the ML.")]
    [SerializeField] private DelegateStatus central;

    [Tooltip("Configuration for conveyor behaviour.")]
    [SerializeField] private ProductionInformation settings;

    [Tooltip("Parent transform under which all active conveyor products are organized.")]
    [SerializeField] private Transform c_ProductsRoot;

    [Tooltip("List of products currently active on the conveyor.")]
    public List<GameObject> c_Products = new List<GameObject>();

    void Update()
    {
        ForwardProducts();
    }

    /// <summary>
    /// Initializes a new conveyor cycle by clearing existing products,
    /// generating a new spawn queue, and spawning the first product.
    /// </summary>
    public void ConveyorRound()
    {
        c_Products.Clear();
        SpawnNextProduct();
    }

    /// <summary>
    /// Removes the front product from the conveyor and assigns it to a target transform.
    /// Also updates the central tracking system with the removed product.
    /// </summary>
    /// <param name="newTransform">Target transform where the product will be moved after removal.</param>
    public GameObject RemoveFromConveyor(Transform newTransform)
    {
        GameObject product = c_Products[0];

        product.transform.SetParent(newTransform);

        product.transform.localPosition = Vector3.zero;

        product.transform.localRotation = Quaternion.identity;

        c_Products.RemoveAt(0);

        central.UpdateProduct(product);
        
        return product;
    }

    /// <summary>
    /// Spawns a new product at the back of the conveyor line using the spawn position defined in settings.
    /// </summary>
    private void SpawnNextProduct()
    {
        Transform c_transform = c_ProductsRoot.transform;

        int randomIndex = Random.Range(0, settings.productOptions.Count);
        GameObject prefab = settings.productOptions[randomIndex];

        GameObject product = Instantiate(prefab, c_transform);

        product.transform.localPosition = settings.spawnPosition;
        product.transform.localRotation = Quaternion.identity;

        c_Products.Add(product);
    }

    /// <summary>
    /// Moves all active products along the conveyor towards their assigned slot positions.
    /// Also triggers spawning when space becomes available at the back of the conveyor.
    /// </summary>
    private void ForwardProducts()
    {
        if (c_Products.Count == 0)
        {
            SpawnNextProduct();
            return;
        }

        c_Products[0].transform.localPosition = Vector3.MoveTowards(c_Products[0].transform.localPosition, settings.destination, settings.speed * Time.deltaTime);

        for (int i = 1; i < c_Products.Count; i++)
        {
            Vector3 destination = settings.destination - new Vector3(0, 0, i * settings.slotDistance);

            c_Products[i].transform.localPosition = Vector3.MoveTowards(c_Products[i].transform.localPosition, destination, settings.speed * Time.deltaTime);
        }

        float lastZ = c_Products[c_Products.Count - 1].transform.localPosition.z;
        float spawnTriggerZ = settings.spawnPosition.z + settings.slotDistance;

        if (lastZ >= spawnTriggerZ)
        {
            SpawnNextProduct();
        }
    }
}
