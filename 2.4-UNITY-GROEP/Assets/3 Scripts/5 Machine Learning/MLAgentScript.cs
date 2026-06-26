using UnityEngine;
using UnityEngine.InputSystem;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;
public class MLAgentScript : Agent
{
    // --------------------- THESE GAME OBJECT MUST BE CHECKED IN THE TEST FUNCTION --------------------- \\
    private Rigidbody agentRigidbody; // Rigidbody component of the MLAgent
    private CellManager cellManagerScript; // Reference to the CellManager script for accessing dropoff locations
    private ConveyorLogic conveyorLogic; // Reference to the ConveyorLogic script for conveyor operations

    [Tooltip("Game object representing the box to be sorted.")]
    [SerializeField] private GameObject boxPickupLocation;

    [Tooltip("Tracks training statistics and current agent state.")]
    [SerializeField] private DelegateData delegateData;

    [Tooltip("Reference to the CellManager script for accessing dropoff locations.")]
    [SerializeField] private GameObject cellManager;

    [Tooltip("Reference to the ConveyorLogic script for conveyor operations.")]
    [SerializeField] private GameObject conveyorGameObject;

    [Tooltip("Transform representing the position where the agent holds the box.")]
    [SerializeField] private Transform boxHoldAnchor;

    // ================================================================================================== \\
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private List<Vector3> dropOffLocations;
    private Vector3 pearTargetDropOffLocation; // Position of the blue box drop-off location
    private Vector3 appleTargetDropOffLocation; // Position of the red box drop-off location

    [Header("Movement")]

    [Tooltip("Speed of the agent.")]
    [SerializeField] private float moveSpeed = 0.5f;

    [Tooltip("Rotation speed of the agent in degrees per second.")]
    [SerializeField] private float turnSpeed = 720f;

    [Tooltip("Y-axis offset applied so the model faces the movement direction correctly.")]
    [SerializeField] private float facingOffsetY = 90f;


    [Header("Input System Heuristic")]
    private InputSystem_Actions controls;

    [Header("Observations")]

    [Tooltip("Maximum expected distance used to normalize observation values.")]
    [SerializeField] private float maxDistance = 20f;

    [Header("Visualizations")]

    private LineRenderer targetLine;

    [Tooltip("Line colour while searching for a box.")]
    [SerializeField] private Color searchingColor = Color.yellow;

    [Tooltip("Line colour while carrying a box.")]
    [SerializeField] private Color carryingColor = Color.green;

    /// <summary>
    /// Represents the current task the agent is performing.
    /// </summary>
    public enum State
    {
        SearchingForBox,
        CarryingBox
    }

    public State currentState = State.SearchingForBox; // Track the current state of the agent
    private GameObject heldProduct;
    private BoxObject boxObject; // ScriptableObject containing box type and dropoff mapping
    private bool isTesting = false;  // Flag to disable pre run checks

    // Invoked whenever the agent passes a carried box to another object.
    public static event Action onBoxPassed;

    new void Awake()
    {
        controls = new InputSystem_Actions();
        targetLine = GetComponent<LineRenderer>();
        SetupLineRenderer();
        if (!Academy.Instance.IsCommunicatorOn) 
        {
            Time.timeScale = 5.0f; // Run the simulation 5x faster
        }
    }

    /// <summary>
    /// Caches component references and performs startup validation.
    /// </summary>
    public override void Initialize()
    {
        agentRigidbody = GetComponent<Rigidbody>();
        agentRigidbody.sleepThreshold = 0;
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
        startingPosition = transform.localPosition; 
        startingRotation = transform.localRotation;
        conveyorLogic.ResetConveyor();
        dropOffLocations = cellManagerScript.GetDropOffLocations();
        appleTargetDropOffLocation = dropOffLocations[0];
        pearTargetDropOffLocation = dropOffLocations[1];
        
        cellManagerScript.ResetDistanceTracking();
    }

    /// <summary>
    /// Resets the environment and agent at the start of each training episode.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        ChangeState(State.SearchingForBox);

        if (agentRigidbody != null)
        {
            agentRigidbody.linearVelocity = Vector3.zero;
            agentRigidbody.angularVelocity = Vector3.zero;
        }   

        transform.localPosition = startingPosition;
        transform.localRotation = startingRotation;

        if (heldProduct != null)
        {
            Destroy(heldProduct);
            heldProduct = null;
        }
        if (conveyorLogic != null)
        {
            conveyorLogic.ResetConveyor(); 
        }

        if (cellManagerScript != null)
        {
            cellManagerScript.ResetDistanceTracking();
        }
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

    /// <summary>
    /// Verifies that all required components and references are assigned.
    /// </summary>
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
        if (boxHoldAnchor == null)
        {
            Debug.LogError("MLAgentScript requires a box hold anchor transform.");
            return false;
        }
        if (GetComponent<Rigidbody>() == null)
        {
            Debug.LogError("MLAgentScript requires a Rigidbody component.");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Supplies observations to the machine learning model each decision step.
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Global observations.
        sensor.AddObservation(currentState == State.CarryingBox ? 1f : 0f); // Whether the agent is holding a box.

        // Relative pickup observation is always present so the vector size stays fixed.
        sensor.AddObservation((boxPickupLocation.transform.position - transform.position) / maxDistance);
        sensor.AddObservation(Vector3.Distance(transform.position, boxPickupLocation.transform.position) / maxDistance);

        // Always expose both drop-off locations so the policy can compare them.
        sensor.AddObservation((appleTargetDropOffLocation - transform.position) / maxDistance);
        sensor.AddObservation((pearTargetDropOffLocation - transform.position) / maxDistance);

        // When a box is held, also expose the correct target drop-off.
        if (currentState == State.CarryingBox)
        {
            sensor.AddObservation((boxObject.dropOffTargetVector3 - transform.position) / maxDistance);
            sensor.AddObservation(Vector3.Distance(transform.position, boxObject.dropOffTargetVector3) / maxDistance);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
        }
    }

    /// <summary>
    /// Executes the actions produced by the trained policy.
    /// </summary>
    public override void OnActionReceived(ActionBuffers actions) // Called when the agent receives an action from the policy or heuristic
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        if (Mathf.Abs(moveX) > 0.01f || Mathf.Abs(moveZ) > 0.01f)
        {
            MoveAgent(moveX, moveZ);
        }

        cellManagerScript.ActionRecievedCall();
    }

    /// <summary>
    /// Changes the current agent state and resets distance tracking.
    /// </summary>
    private void ChangeState(State newState) // Helper function to change the agent's state and reset relevant tracking variables for reward shaping
    {
        currentState = newState;
        delegateData.SetCurrentAction(newState.ToString());
        cellManagerScript.ResetDistanceTracking();
    }

    /// <summary>
    /// Returns the position of the agent's current navigation target.
    /// </summary>
    public Vector3 GetActiveTargetPosition()
    {
        return currentState == State.CarryingBox
            ? boxObject.dropOffTargetVector3
            : boxPickupLocation.transform.position;
    }

    /// <summary>
    /// Used for manual control of the agent during testing or debugging
    /// </summary>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        
        // Read the 2D input vector (WASD) from the New Input System
        Vector2 inputVector = controls.Player.Move.ReadValue<Vector2>();
        continuousActionsOut[0] = -inputVector.y;
        continuousActionsOut[1] = inputVector.x;
    }

    /// <summary>
    /// Moves and rotates the agent based on the provided movement input.
    /// </summary>
    /// <param name="moveX">Horizontal movement input.</param>
    /// <param name="moveZ">Vertical movement input.</param>
    void MoveAgent(float moveX, float moveZ) 
    {
        Vector3 moveDir = new Vector3(moveX, 0, moveZ);

        if (moveDir.magnitude > 1f)
        {
            moveDir.Normalize();
        }

        // Set velocity directly instead of MovePosition to let physics handle wall collisions naturally
        Vector3 targetVelocity = moveDir * moveSpeed;
        // Keep the current Y velocity (which should be 0 anyway) just in case
        targetVelocity.y = agentRigidbody.linearVelocity.y; 
        agentRigidbody.linearVelocity = targetVelocity;

        // Rotation for aesthetics
        if (moveDir.sqrMagnitude > 0.001f) 
        {
            float targetAngle = Mathf.Atan2(moveX, moveZ) * Mathf.Rad2Deg + facingOffsetY;
            Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);

            agentRigidbody.MoveRotation(Quaternion.RotateTowards(
                agentRigidbody.rotation,
                targetRotation,
                turnSpeed * Time.fixedDeltaTime
            ));
        }
    }

    /// <summary>
    /// Handles box pickup when the agent enters the pickup trigger while searching for a box.
    /// </summary>
    /// <param name="collider">The trigger collider that the agent entered.</param>
    void OnTriggerEnter(Collider collider)
    {
        GameObject collidingGameObject = collider.gameObject;
        if(collidingGameObject.CompareTag("PickupZoneTrigger") && currentState == State.SearchingForBox) // Pickup box logic
        {
            heldProduct = conveyorLogic.RemoveFromConveyor(boxHoldAnchor);
            Rigidbody boxRb = heldProduct.GetComponent<Rigidbody>();
            if (boxRb != null) {
                boxRb.isKinematic = true; // Stops the engine from calculating physics for the box
            }

            Collider boxCollider = heldProduct.GetComponent<Collider>();
            if (boxCollider != null) {
                boxCollider.enabled = false; // Prevents overlapping with the Agent
            }
            ProductIdentityEnums.Type boxType = heldProduct.GetComponent<ProductIdentity>().identity;
            AssignDropoff(boxType);

            ChangeState(State.CarryingBox);
            cellManagerScript.BoxPickedUp();
        }
        
    }

    /// <summary>
    /// Detects collisions with the arena boundaries and applies the corresponding penalty.
    /// </summary>
    /// <param name="collision">Information about the collision.</param>
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Bounds"))
        {
            Debug.Log("Bounds HIT");
            cellManagerScript.BoundsHit();
        }
    }

    /// <summary>
    /// Applies a continuous penalty while the agent remains in contact with the arena boundaries.
    /// </summary>
    /// <param name="collision">Information about the ongoing collision.</param>
    void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.CompareTag("Bounds"))
        {
            Debug.Log("Bounds STAY");
            cellManagerScript.BoundsStay();
        }
    }

    /// <summary>
    /// Transfers the currently held box to a new parent object and returns it.
    /// </summary>
    /// <param name="newParent">The transform that will become the box's new parent.</param>
    /// <returns>The box that was being carried by the agent.</returns>
    public GameObject PassBox(Transform newParent)
    {
        onBoxPassed?.Invoke();

        GameObject droppedProduct = heldProduct;

        droppedProduct.transform.SetParent(newParent, false);
        droppedProduct.transform.localPosition = Vector3.zero;
        droppedProduct.transform.localRotation = Quaternion.identity;

        heldProduct = null;
        ChangeState(State.SearchingForBox);

        return droppedProduct;
    }

    /// <summary>
    /// Assigns the correct drop-off target based on the type of box being carried.
    /// </summary>
    /// <param name="boxType">The type of box that was picked up.</param>
    void AssignDropoff(ProductIdentityEnums.Type boxType)
    {
        if(boxType == ProductIdentityEnums.Type.Apples)
        {
            boxObject = new BoxObject(boxType) {dropOffTargetVector3 = appleTargetDropOffLocation};
        }
        else if(boxType == ProductIdentityEnums.Type.Pears)
        {
            boxObject = new BoxObject(boxType) {dropOffTargetVector3 = pearTargetDropOffLocation};
        }
    }

    void Update()
    {
        DrawLineToTarget();
    }

    /// <summary>
    /// Configures the LineRenderer used to visualize the agent's current target.
    /// </summary>
    private void SetupLineRenderer()
    {
        targetLine.positionCount = 2;
        targetLine.startWidth = 0.05f;
        targetLine.endWidth = 0.05f;
        
        // Use a simple built-in unlit material so it doesn't look pink/missing
        targetLine.material = new Material(Shader.Find("Sprites/Default")); 
    }

    /// <summary>
    /// Draws a line from the agent to its current navigation target.
    /// </summary>
    private void DrawLineToTarget()
    {
        if (targetLine == null) return;

        // Set the line colors based on what the agent is currently doing
        Color currentColor = (currentState == State.CarryingBox) ? carryingColor : searchingColor;
        targetLine.startColor = currentColor;
        targetLine.endColor = currentColor;

        // Origin point: The agent's current position
        targetLine.SetPosition(0, transform.position);

        // Destination point: The active target position based on state
        targetLine.SetPosition(1, GetActiveTargetPosition());
    }
}
