using UnityEngine;
using UnityEngine.InputSystem;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
public class MLAgentScript : Agent
{
    // --------------------- THESE GAME OBJECT MUST BE CHECKED IN THE TEST FUNCTION --------------------- \\
    [SerializeField] public Rigidbody agentRigidbody;
    [SerializeField] public GameObject boxPickupObject;
    [SerializeField] public List<GameObject> dropoffPositions = new List<GameObject>(); // Create list of possible dropoff positions and assign them in the inspector
    [SerializeField] public List<GameObject> boxPrefabs = new List<GameObject>(); // Create list of box prefabs and assign them in the inspector
    // ================================================================================================== \\

    struct BoxPairData
    {
        public int boxType;
        public int dropOffPositionID;
        public Vector3 dropOffPosition;
    }
    private List<BoxPairData> boxPairs = new List<BoxPairData>(); // List to hold box types and their corresponding dropoff positions
    [SerializeField] bool isTesting = true;
    public float moveSpeed = 0.5f;
    bool holdingBox = false;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Setup();

        if (!PrerunTests())
        {
            Application.Quit(); // Quit the application if the tests fail
            return;
        }

        agentRigidbody = GetComponent<Rigidbody>();
    }

    bool PrerunTests()
    {
        if(GetComponent<Rigidbody>() == null)
        {
            Debug.LogError("MLAgentScript requires a Rigidbody component.");
            return false;
        }
        if (boxPickupObject == null)
        {
            Debug.LogError("MLAgentScript requires a box pickup object.");
            return false;
        }
        if (dropoffPositions.Count == 0)
        {
            Debug.LogError("MLAgentScript requires at least one dropoff position.");
            return false;
        }
        if(boxPrefabs.Count == 0)
        {
            Debug.LogError("MLAgentScript requires at least one box prefab.");
            return false;
        }
        if(dropoffPositions.Count != boxPrefabs.Count)
        {
            Debug.LogError("The number of dropoff positions must match the number of box prefabs.");
            return false;
        }

        return true;
    }

    void Setup()
    {
        // Create box pairs based on the number of dropoff positions
        for(int i =0; i < dropoffPositions.Count; i++)
        {
            BoxPairData pairData = new BoxPairData
            {
                boxType = i,
                dropOffPositionID = i,
                dropOffPosition = dropoffPositions[i].transform.position

            };
            boxPairs.Add(pairData);
        }

        Vector3 boxPickupPosition = boxPickupObject.transform.position;


    }

    public override void CollectObservations(VectorSensor sensor)
    {
        foreach (var position in dropoffPositions)
        {
            sensor.AddObservation(position.transform.position); // Dropoff positions
            sensor.AddObservation(Vector3.Distance(transform.position, position.transform.position)); // Distance to each dropoff
        }

        sensor.AddObservation(transform.position); // Agent's position
        sensor.AddObservation(holdingBox); // Whether the agent is holding a box
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
    }

    public override void OnEpisodeBegin()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
