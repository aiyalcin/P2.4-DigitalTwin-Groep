using UnityEngine;
using UnityEngine.InputSystem;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
public class MLAgentScript : Agent
{
    // --------------------- THESE GAME OBJECT MUST BE CHECKED IN THE TEST FUNCTION --------------------- \\
    Rigidbody agentRigidbody; // Rigidbody component of the MLAgent
    [SerializeField] public GameObject boxPickupObject; // Game object representing the box to be sorted
    [SerializeField] public List<GameObject> dropoffPositions = new List<GameObject>(); // Create list of possible dropoff positions and assign them in the inspector
    [SerializeField] public List<GameObject> boxPrefabs = new List<GameObject>(); // Create list of box prefabs and assign them in the inspector
    // ================================================================================================== \\

    [Header("Input")]
    [SerializeField] public InputActionAsset inputActions;
    [Header("Movement")]
    [SerializeField] public float moveSpeed = 0.5f;
    [SerializeField] public float turnSpeed = 720f;
    [SerializeField] public float facingOffsetY = 90f;

    [Header("Observations")]
    [SerializeField] private float maxDistance = 20f; // used to normalize distances
    bool holdingBox = false;
    private bool isTesting = true; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!isTesting)
        {
            if(!PrerunTests())
            {
                Application.Quit(); // Quit the application if the tests fail
                return;
            }
        }

        agentRigidbody = GetComponent<Rigidbody>();
        agentRigidbody.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
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
        else
        {
            return true;
        }
            
    }


    public override void CollectObservations(VectorSensor sensor)
    {
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Debug.Log("Action move x recieved: " + actions.ContinuousActions[0]);
        Debug.Log("Action move y recieved: " + actions.ContinuousActions[1]);
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        MoveAgent(moveX, moveZ);
    }

    void MoveAgent(float moveX, float moveZ)
    {
        transform.position += new Vector3(moveX, 0, moveZ) * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0, Mathf.Atan2(moveX, moveZ) * Mathf.Rad2Deg + facingOffsetY, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
