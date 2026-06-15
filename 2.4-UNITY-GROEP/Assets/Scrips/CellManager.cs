using UnityEngine;
using System.Collections.Generic;

public class CellManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> dropOffLocationObjects; // List of dropoff locations for the boxes
    [SerializeField] private List<GameObject> boxPrefabs; 
    [SerializeField] private GameObject conveyorGameObject;
    private ConveyorLogic conveyorLogic;
    [SerializeField] private GameObject MLAgentGameObject;
    private MLAgentScript mlAgentScript;
    private BoxObject boxObject;


    public DropOffObject dropOffObject;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        conveyorLogic = conveyorGameObject.GetComponent<ConveyorLogic>();
        mlAgentScript = MLAgentGameObject.GetComponent<MLAgentScript>();
    }

    public void CreateCell(Vector3 position, int boxVariationAmount)
    {
        //
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }
}
