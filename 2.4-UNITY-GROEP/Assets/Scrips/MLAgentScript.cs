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
    [SerializeField] private GameObject boxPickupLocation; // Game object representing the box to be sorted
    [SerializeField] private BoxObject boxObject; // ScriptableObject containing box type and dropoff mapping
    // ================================================================================================== \\

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float turnSpeed = 720f;
    [SerializeField] private float facingOffsetY = 90f;

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
        if (boxPickupLocation == null)
        {
            Debug.LogError("MLAgentScript requires a box pickup location object.");
            return false;
        }
        else
        {
            return true;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Global observations
        sensor.AddObservation(transform.position); // Agent's position
        sensor.AddObservation(holdingBox ? 1 : 0); // Whether the agent is holding a box (1 for true, 0 for false)

        //Situational observations
        if(holdingBox)
        {
            sensor.AddObservation(Vector3.Distance(transform.position, boxObject.dropOffTargetTransform) / maxDistance); // Normalized distance to the box pickup location (to encourage dropping off the box)
            sensor.AddObservation(boxObject.dropOffTargetTransform); // Dropoff location for the currently held box
        }
        else
        {
            sensor.AddObservation(boxPickupLocation.transform.position); // Box pickup location
            sensor.AddObservation(Vector3.Distance(transform.position, boxPickupLocation.transform.position) / maxDistance); // Normalized distance to the box pickup location 
        }
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
