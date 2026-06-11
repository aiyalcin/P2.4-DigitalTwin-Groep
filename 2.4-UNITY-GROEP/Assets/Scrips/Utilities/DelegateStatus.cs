using System.Collections.Generic;
using UnityEngine;

public class DelegateStatus : MonoBehaviour
{
    /// <summary>
    /// Parties that need to be tracked
    /// </summary>
    [Header("Parties")]

    [Tooltip("The dropoofs being tracked [LOCATION].")]
    public List<GameObject> dropOffs = new List<GameObject>();

    [Tooltip("The current product being tracked [LOCATION & TYPE]")]
    public GameObject product;

    [Tooltip("The humanoid being tracked. [LOCATION]")]
    public GameObject humenanoid;

    /// <summary>
    /// Statussen that are actually tracked (do not need to be filled in in inspector)
    /// </summary>
    [Header("Status")]

    [Tooltip("Current world positions of the tracked dropoffs.")]
    public List<Vector3> dropOffLocations;
    [Tooltip("Current world position of the tracked product.")]
    public Vector3 productLocation;

    [Tooltip("Type/identity of the currently tracked product.")]
    public ProductIdentityEnums.Type productType;

    [Tooltip("Current world position of the tracked humanoid.")]
    public Vector3 humenanoidLocation;

    private Vector3 lastProductPos;
    private Vector3 lastHumanoidPos;

    private void Start()
    {
        if (dropOffLocations == null)
        {
            dropOffLocations = new List<Vector3>();
        }

        if (product != null)
        {
            lastProductPos = product.transform.position;
            productLocation = lastProductPos;
        }

        if (humenanoid != null)
        {
            lastHumanoidPos = humenanoid.transform.position;
            humenanoidLocation = lastHumanoidPos;
        }

        SyncDropOffLocations();
    }

    private void Update()
    {
        UpdateLocations();
    }

    /// <summary>
    /// Updating the location whenever location changes for products and humanoids
    /// </summary>
    private void UpdateLocations()
    {
        if (product != null)
        {
            Vector3 current = product.transform.position;

            if (current != lastProductPos)
            {
                productLocation = current;
                lastProductPos = current;
            }
        }

        if (humenanoid != null)
        {
            Vector3 current = humenanoid.transform.position;

            if (current != lastHumanoidPos)
            {
                humenanoidLocation = current;
                lastHumanoidPos = current;
            }
        }

        SyncDropOffLocations();
    }

    /// <summary>
    /// Updating location for boxes
    /// </summary>
    private void SyncDropOffLocations()
    {
        for (int i = 0; i < dropOffs.Count; i++)
        {
            if (dropOffs[i] == null)
            {
                continue;
            }

            Vector3 pos = dropOffs[i].transform.position;

            if (i >= dropOffLocations.Count)
            {
                dropOffLocations.Add(pos);
            }
            else
            {
                dropOffLocations[i] = pos;
            }
        }
    }

    /// <summary>
    /// Method to register the current product being picked up
    /// </summary>
    /// <param name="newProduct"> The last picked up product from the conveyor belt</param>
    public void UpdateProduct(GameObject newProduct)
    {
        product = newProduct;

        ProductIdentity identity = product.GetComponent<ProductIdentity>();
        productType = identity.identity;
    }
}