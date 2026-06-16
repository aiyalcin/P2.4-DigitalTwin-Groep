using UnityEngine;

public class PickupZoneScript : MonoBehaviour
{
    [SerializeField] private GameObject mlAgentGameObject;
    [SerializeField] private int boxTypeID;
    private MLAgentScript mlAgentScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mlAgentScript = mlAgentGameObject.GetComponent<MLAgentScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
