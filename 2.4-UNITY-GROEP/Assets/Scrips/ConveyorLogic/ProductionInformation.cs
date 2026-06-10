using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProductionInformation", menuName = "Scriptable Objects/ProductionInformation")]
public class ProductionInformation : ScriptableObject
{
    [Header("Conveyer Information")]
    public Vector3 destination;
    public int slotCount; 
    public float speed;
    public float slotDistance;

    [Header("Product Information")]
    public int productCount;
    public List<GameObject> productOptions = new List<GameObject>();

    public bool centralized;
    public List<GameObject> productList = new List<GameObject>();

    private void OnEnable()
    {
        if(centralized)
        {
            for(int i = 0; i < productCount; i++)
            {
                int ran = Random.Range(0, productOptions.Count);
                productList.Add(productOptions[ran]);
            }
        }
    }
}
