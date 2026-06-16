using UnityEngine;
using System.Collections.Generic;

public class CellManager : MonoBehaviour
{
    [SerializeField] private Transform redDropOffLocation;
    [SerializeField] private Transform blueDropOffLocation;

    [SerializeField] private GameObject conveyorGameObject;
    private ConveyorLogic conveyorLogic;
    [SerializeField] private GameObject MLAgentGameObject;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<Vector3> GetDropOffLocations()
    {
        List<Vector3> dropOffLocations = new List<Vector3>
        {
            redDropOffLocation.position,
            blueDropOffLocation.position
        };
        return dropOffLocations;
    }

    
}
