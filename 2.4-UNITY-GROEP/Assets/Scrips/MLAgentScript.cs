using UnityEngine;
using UnityEngine.InputSystem;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using Unity.VisualScripting;
public class MLAgentScript : Agent
{
    // --------------------- THESE GAME OBJECT MUST BE CHECKED IN THE TEST FUNCTION --------------------- \\
    Rigidbody agentRigidbody; // Rigidbody component of the MLAgent
    [SerializeField] private GameObject boxPickupLocation; // Game object representing the box to be sorted
    [SerializeField] private GameObject cellManager; // Reference to the CellManager script for accessing dropoff locations
    private CellManager cellManagerScript; // Reference to the CellManager script for accessing dropoff locations
    [SerializeField] private GameObject conveyorGameObject; // Reference to the ConveyorLogic script for conveyor operations
    private ConveyorLogic conveyorLogic; // Reference to the ConveyorLogic script for conveyor operations
    // ================================================================================================== \\
    

    [SerializeField] private GameObject debugSphere; // A sphere used for debugging purposes to visualize the target drop-off location

    private List<Vector3> dropOffLocations;
    private Vector3 blueTargetDropOffLocation; // Position of the blue box drop-off location
    private Vector3 redTargetDropOffLocation; // Position of the red box drop-off location

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float turnSpeed = 720f;
    [SerializeField] private float facingOffsetY = 90f;
    [SerializeField] private Transform boxHoldAnchor; 

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
        agentRigidbody.constraints |= RigidbodyConstraints.FreezePositionY;
        agentRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        agentRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
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
        dropOffLocations = cellManagerScript.GetDropOffLocations();
        redTargetDropOffLocation = dropOffLocations[0];
        blueTargetDropOffLocation = dropOffLocations[1];

        GameObject debugSphereInstanceR; // Instance of the debug sphere to visualize the target drop-off location
        GameObject debugSphereInstanceB; // Instance of the debug sphere to visualize the target drop-off location
        Debug.Log($"Red Drop-off Location: {redTargetDropOffLocation}, Blue Drop-off Location: {blueTargetDropOffLocation}");
        debugSphereInstanceR = Instantiate(debugSphere, redTargetDropOffLocation, Quaternion.identity);
        debugSphereInstanceB = Instantiate(debugSphere, blueTargetDropOffLocation, Quaternion.identity);
        Debug.Log($"Debug sphere instantiated at: {redTargetDropOffLocation}");
        Debug.Log($"Debug sphere instantiated at: {blueTargetDropOffLocation}");
        Destroy(debugSphereInstanceR, 5f); // Destroy the debug sphere after 5 seconds to prevent clutter
        Destroy(debugSphereInstanceB, 5f); // Destroy the debug sphere after 5 seconds to prevent clutter
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
        // Global observations.
        sensor.AddObservation(holdingBox ? 1f : 0f); // Whether the agent is holding a box.

        // Relative pickup observation is always present so the vector size stays fixed.
        sensor.AddObservation((boxPickupLocation.transform.position - transform.position) / maxDistance);
        sensor.AddObservation(Vector3.Distance(transform.position, boxPickupLocation.transform.position) / maxDistance);

        // Always expose both drop-off locations so the policy can compare them.
        sensor.AddObservation((redTargetDropOffLocation - transform.position) / maxDistance);
        sensor.AddObservation((blueTargetDropOffLocation - transform.position) / maxDistance);

        // When a box is held, also expose the correct target drop-off.
        if (holdingBox)
        {
            sensor.AddObservation((boxObject.dropOffTargetTransform - transform.position) / maxDistance);
            sensor.AddObservation(Vector3.Distance(transform.position, boxObject.dropOffTargetTransform) / maxDistance);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions) // Called when the agent receives an action from the policy or heuristic
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        if (Mathf.Abs(moveX) > 0.01f || Mathf.Abs(moveZ) > 0.01f)
        {
            MoveAgent(moveX, moveZ);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut) // Used for manual control of the agent during testing or debugging
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

        agentRigidbody.MovePosition(agentRigidbody.position + moveDir * moveSpeed * Time.fixedDeltaTime);

        // Rotation for aesthetics
        if (moveDir.sqrMagnitude > 0.001f) 
        {
            // Calculate the target angle based purely on the input direction
            float targetAngle = Mathf.Atan2(moveX, moveZ) * Mathf.Rad2Deg + facingOffsetY;
            Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);

            // Smoothly interpolate towards the target rotation using turnSpeed
            agentRigidbody.MoveRotation(Quaternion.RotateTowards(
                agentRigidbody.rotation,
                targetRotation,
                turnSpeed * Time.fixedDeltaTime
            ));
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        // 1. Universal log to see if ANY trigger contact is happening
        Debug.Log($"Trigger entered with object named: {collider.gameObject.name} | Tag: {collider.gameObject.tag}");
        GameObject collidingGameObject = collider.gameObject;
        if(collidingGameObject.CompareTag("PickupZoneTrigger") && !holdingBox) // Pickup box logic
        {
            Debug.Log("Pickup zone tag matched! Attempting to grab box...");
            heldProduct = conveyorLogic.RemoveFromConveyor(agentRigidbody.transform);
            heldProduct.transform.SetParent(boxHoldAnchor);
            heldProduct.transform.localPosition = Vector3.zero;
            heldProduct.transform.localRotation = Quaternion.identity;
            AssignDropoff(heldProduct.GetComponent<ProductIdentity>().identity);
            holdingBox = true;
        }
        if(collidingGameObject.CompareTag("DropoffZoneTrigger") && holdingBox) // Dropoff box logic
        {
            Debug.Log("Dropoff zone tag matched! Attempting to drop box...");
            Debug.Log("Agent is close enough to the drop-off location. Dropping box...");
            if(collidingGameObject.GetComponent<DropoffZoneScript>().CheckDropoff(heldProduct.GetComponent<ProductIdentity>().identity))
            {
                AddReward(1.0f);
                Debug.Log("Correct box dropped! Reward given.");
            }
            else
            {
                Debug.Log("Incorrect box dropped! Penalty applied.");
                AddReward(-1.0f);
            }
            Destroy(heldProduct);
            heldProduct = null;
            holdingBox = false;
            Debug.Log("Box dropping done!");
        }
    }

    void AssignDropoff(ProductIdentityEnums.Type boxType)
    {
        if(boxType == ProductIdentityEnums.Type.Red)
        {
            boxObject = new BoxObject(boxType) {dropOffTargetTransform = redTargetDropOffLocation};
        }
        else if(boxType == ProductIdentityEnums.Type.Blue)
        {
            boxObject = new BoxObject(boxType) {dropOffTargetTransform = blueTargetDropOffLocation};
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
