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

    [Tooltip("DelegateData used to report the current product type to the UI.")]
    [SerializeField] private DelegateData delegateData;

    void Update()
    {
        ForwardProducts();
    }

    /// <summary>
    /// Removes the first product from the conveyor, parents it to the given transform and returns it.
    /// </summary>
    /// <param name="newTransform">The transform the product will be parented to after removal.</param>
    /// <returns>The removed product GameObject.</returns>
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

    /// <summary>
    /// Reads the type of the next product in the queue and reports it to DelegateData for UI display.
    /// </summary>
    public void GetCurrentProduct()
    {
        GameObject product = c_Products[0];

        ProductIdentity productI = product.GetComponent<ProductIdentity>();

        delegateData.SetCurrentProduct(productI.identity.ToString());
    }

    /// <summary>
    /// Spawns a random product prefab at the configured spawn position and adds it to the conveyor queue.
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
    /// Moves all products toward their target slot each frame. Spawns a new product when the last one moves far enough forward.
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


    /// <summary>
    /// Destroys all active conveyor products, clears the queue and spawns a fresh first product. Called at the start of each episode.
    /// </summary>
    public void ResetConveyor()
    {
        foreach (GameObject product in c_Products)
        {
            if (product != null)
            {
                Destroy(product);
            }
        }

        c_Products.Clear();

        SpawnNextProduct();
    }
}
