using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ConveyorLogic : MonoBehaviour
{
    [Tooltip("Configuration for conveyor behaviour.")]
    [SerializeField] private FactorySettings settings;

    [Tooltip("Parent transform under which all active conveyor products are organized.")]
    [SerializeField] private Transform c_ProductsRoot;

    [Tooltip("List of products currently active on the conveyor.")]
    public List<GameObject> c_Products = new List<GameObject>();

    [SerializeField] private DelegateData delegateData;

    void Update()
    {
        ForwardProducts();
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

        GetCurrentProduct();

        c_Products.RemoveAt(0);
        
        return product;
    }

    public void GetCurrentProduct()
    {
        GameObject product = c_Products[0];

        ProductIdentity productI = product.GetComponent<ProductIdentity>();

        delegateData.SetCurrentProduct(productI.identity.ToString());
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
    public void ResetConveyor()
    {
        // 1. Loop through the list and physically destroy every product in the scene
        foreach (GameObject product in c_Products)
        {
            if (product != null)
            {
                Destroy(product);
            }
        }

        // 2. Now it is safe to clear the C# tracking list
        c_Products.Clear();

        // 3. Spawn the first product to restart the cycle
        SpawnNextProduct();
    }
}
