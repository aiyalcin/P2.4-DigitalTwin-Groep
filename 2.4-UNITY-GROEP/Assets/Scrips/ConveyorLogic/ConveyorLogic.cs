using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UIElements;

public class ConveyorLogic : MonoBehaviour
{
    [SerializeField] private ProductionInformation settings;
    [SerializeField] private Transform c_ProductsRoot;
    [SerializeField] private List<GameObject> q_Products = new List<GameObject>();
    [SerializeField] private List<GameObject> c_Products = new List<GameObject>();

    private Vector3 spawnPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnPosition = settings.destination - new Vector3(0, 0, settings.slotCount * settings.slotDistance);
        ConveyorRound();
    }

    // Update is called once per frame
    void Update()
    {
        ForwardProducts();
    }

    public void ConveyorRound()
    {
        q_Products.Clear();
        q_Products = GenerateList();

        c_Products.Clear();
    }

    public void RemoveFromConveyor(Transform newTransform)
    {
        c_Products[0].transform.SetParent(newTransform);
        c_Products[0].transform.localPosition = Vector3.zero;
        c_Products[0].transform.localRotation = Quaternion.identity;

        c_Products.RemoveAt(0);
    }

    private void SpawnNextProduct()
    {
        Transform c_transform = c_ProductsRoot.gameObject.transform;

        if (q_Products.Count == 0) { return; }

        GameObject product = Instantiate(q_Products[0], c_transform);

        product.transform.localPosition = spawnPosition;
        product.transform.localRotation = Quaternion.identity;

        c_Products.Add(product);
        q_Products.RemoveAt(0);
    }

    private void ForwardProducts()
    {
        if (c_Products.Count == 0) { return; }

        c_Products[0].transform.localPosition = Vector3.MoveTowards(c_Products[0].transform.localPosition, settings.destination, settings.speed * Time.deltaTime);

        for (int i = 1; i < c_Products.Count; i++)
        {
            Vector3 destination = settings.destination - new Vector3(0, 0, i * settings.slotDistance);

            c_Products[i].transform.localPosition = Vector3.MoveTowards(c_Products[i].transform.localPosition, destination, settings.speed * Time.deltaTime);
        }
    }

    private List<GameObject> GenerateList()
    {
        if(settings.centralized)
        {
            return settings.productList;
        }

        List<GameObject> productList = new List<GameObject>();

        for (int i = 0; i < settings.productCount; i++)
        {
            int ran = Random.Range(0, settings.productOptions.Count);
            productList.Add(settings.productOptions[ran]);
        }

        return productList;
    }
}
