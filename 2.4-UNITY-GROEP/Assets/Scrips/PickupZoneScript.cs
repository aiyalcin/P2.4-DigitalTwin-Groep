using UnityEngine;

public class PickupZoneScript : MonoBehaviour
{
    [SerializeField] private ConveyorLogic conveyor;
    [SerializeField] private MLAgentScript mlAgentScript;
    [SerializeField] private CellManager cellManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MLAgent"))
        {
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
