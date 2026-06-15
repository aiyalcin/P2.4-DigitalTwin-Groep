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
    [SerializeField] private List<GameObject> dropOffLocations; // List of dropoff locations for the boxes
    [SerializeField] private GameObject cellManager; // Reference to the CellManager script for accessing dropoff locations
    private CellManager cellManagerScript; // Reference to the CellManager script for accessing dropoff locations
    [SerializeField] private GameObject conveyorGameObject; // Reference to the ConveyorLogic script for conveyor operations
    private ConveyorLogic conveyorLogic; // Reference to the ConveyorLogic script for conveyor operations
    // ================================================================================================== \\
    
    private ProductIdentityEnums.Type currentBoxType; // Enum to track the type of box currently held by the agent
    private Vector3 blueTargetDropOffLocation; // Position of the blue box drop-off location
    private Vector3 redTargetDropOffLocation; // Position of the red box drop-off location

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float turnSpeed = 720f;
    [SerializeField] private float facingOffsetY = 90f;

    [Header("Input System Heuristic")]
    private InputSystem_Actions controls;

    [Header("Observations")]
    [SerializeField] private float maxDistance = 20f; // used to normalize distances
    bool holdingBox = false; // Bool to track whether the agent is currently holding a box
    private GameObject heldProduct;
    private BoxObject boxObject; // ScriptableObject containing box type and dropoff mapping
    private bool isTesting = true;  // Flag to disable pre run checks

    void Awake()
    {
        controls = new InputSystem_Actions();
    }

    void Start()
    {
        agentRigidbody = GetComponent<Rigidbody>();
        agentRigidbody.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        conveyorLogic = conveyorGameObject.GetComponent<ConveyorLogic>();
        cellManagerScript = cellManager.GetComponent<CellManager>();

        if (!isTesting)
        {
            if(!PrerunTests())
            {
                Application.Quit(); // Quit the application if the tests fail
                return;
            }
        }

        conveyorLogic.ConveyorRound();
        
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        controls.Enable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        controls.Disable();
    }

    bool PrerunTests()
    {
        if(GetComponent<Rigidbody>() == null)
        {
            Debug.LogError("MLAgentScript requires a Rigidbody component.");
            return false;
        }
        if(cellManager == null)
        {
            Debug.LogError("MLAgentScript requires a reference to the CellManager.");
            return false;
        }
        if(conveyorGameObject == null)
        {
            Debug.LogError("MLAgentScript requires a reference to the Conveyor GameObject.");
            return false;
        }
        if (boxPickupLocation == null)
        {
            Debug.LogError("MLAgentScript requires a box pickup location object.");
            return false;
        }
        if (GetComponent<Rigidbody>() == null)
        {
            Debug.LogError("MLAgentScript requires a Rigidbody component.");
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
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        if (Mathf.Abs(moveX) > 0.01f || Mathf.Abs(moveZ) > 0.01f)
        {
            MoveAgent(moveX, moveZ);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        
        // Read the 2D input vector (WASD) from the New Input System
        Vector2 inputVector = controls.Player.Move.ReadValue<Vector2>();
        continuousActionsOut[0] = -inputVector.y; // mapped to moveX
        continuousActionsOut[1] = inputVector.x; // mapped to moveZ
    }

    void MoveAgent(float moveX, float moveZ)
    {
        Vector3 moveDir = new Vector3(moveX, 0, moveZ);
    
        if (moveDir.magnitude > 1f)
        {
            moveDir.Normalize();
        }

        transform.position += moveDir * moveSpeed * Time.deltaTime;

        // Rotation for aesthetics
        if (moveDir.sqrMagnitude > 0.001f) 
        {
            // Calculate the target angle based purely on the input direction
            float targetAngle = Mathf.Atan2(moveX, moveZ) * Mathf.Rad2Deg + facingOffsetY;
            Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);

            // Smoothly interpolate towards the target rotation using turnSpeed
            transform.rotation = Quaternion.RotateTowards(
            transform.rotation, 
            targetRotation, 
            turnSpeed * Time.deltaTime
            );
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        // 1. Universal log to see if ANY trigger contact is happening
        Debug.Log($"Trigger entered with object named: {collider.gameObject.name} | Tag: {collider.gameObject.tag}");

        if(collider.gameObject.CompareTag("PickupZoneTrigger") && !holdingBox) // Pickup box logic
        {
            Debug.Log("Pickup zone tag matched! Attempting to grab box...");
            holdingBox = true;

            if (conveyorLogic != null)
            {
                heldProduct = conveyorLogic.RemoveFromConveyor(agentRigidbody.transform);

            }
            else
            {
                Debug.LogError("Conveyor reference is missing on the script!");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
