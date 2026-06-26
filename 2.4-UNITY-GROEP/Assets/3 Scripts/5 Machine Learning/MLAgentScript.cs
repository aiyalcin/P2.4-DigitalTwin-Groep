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
    Rigidbody agentRigidbody; // Rigidbody component of the MLAgent
    [SerializeField] private GameObject boxPickupLocation; // Game object representing the box to be sorted
    [SerializeField] private DelegateData delegateData;
    [SerializeField] private GameObject cellManager; // Reference to the CellManager script for accessing dropoff locations
    private CellManager cellManagerScript; // Reference to the CellManager script for accessing dropoff locations
    [SerializeField] private GameObject conveyorGameObject; // Reference to the ConveyorLogic script for conveyor operations
    private ConveyorLogic conveyorLogic; // Reference to the ConveyorLogic script for conveyor operations
    [SerializeField] private Transform boxHoldAnchor; // Transform representing the position where the agent holds the box

    // ================================================================================================== \\
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private List<Vector3> dropOffLocations;
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
    [Header("Visualizations")]
    private LineRenderer targetLine;
    [SerializeField] private Color searchingColor = Color.yellow;
    [SerializeField] private Color carryingColor = Color.green;
    public enum State
    {
        SearchingForBox,
        CarryingBox
    }
    public State currentState = State.SearchingForBox; // Track the current state of the agent
    private GameObject heldProduct;
    private BoxObject boxObject; // ScriptableObject containing box type and dropoff mapping
    private bool isTesting = false;  // Flag to disable pre run checks

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
        redTargetDropOffLocation = dropOffLocations[0];
        blueTargetDropOffLocation = dropOffLocations[1];
        
        cellManagerScript.ResetDistanceTracking();
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Episode Began!");
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


    public override void CollectObservations(VectorSensor sensor)
    {
        // Global observations.
        sensor.AddObservation(currentState == State.CarryingBox ? 1f : 0f); // Whether the agent is holding a box.

        // Relative pickup observation is always present so the vector size stays fixed.
        sensor.AddObservation((boxPickupLocation.transform.position - transform.position) / maxDistance);
        sensor.AddObservation(Vector3.Distance(transform.position, boxPickupLocation.transform.position) / maxDistance);

        // Always expose both drop-off locations so the policy can compare them.
        sensor.AddObservation((redTargetDropOffLocation - transform.position) / maxDistance);
        sensor.AddObservation((blueTargetDropOffLocation - transform.position) / maxDistance);

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

    private void ChangeState(State newState) // Helper function to change the agent's state and reset relevant tracking variables for reward shaping
    {
        currentState = newState;
        delegateData.SetCurrentAction(newState.ToString());
        cellManagerScript.ResetDistanceTracking();
    }

    public Vector3 GetActiveTargetPosition()
    {
        return currentState == State.CarryingBox
            ? boxObject.dropOffTargetVector3
            : boxPickupLocation.transform.position;
    }

    public override void Heuristic(in ActionBuffers actionsOut) // Used for manual control of the agent during testing or debugging
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        
        // Read the 2D input vector (WASD) from the New Input System
        Vector2 inputVector = controls.Player.Move.ReadValue<Vector2>();
        continuousActionsOut[0] = -inputVector.y;
        continuousActionsOut[1] = inputVector.x;
    }

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

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Bounds"))
        {
            Debug.Log("Bounds HIT");
            cellManagerScript.BoundsHit();
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.CompareTag("Bounds"))
        {
            Debug.Log("Bounds STAY");
            cellManagerScript.BoundsStay();
        }
    }

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

    void AssignDropoff(ProductIdentityEnums.Type boxType)
    {
        if(boxType == ProductIdentityEnums.Type.Red)
        {
            boxObject = new BoxObject(boxType) {dropOffTargetVector3 = redTargetDropOffLocation};
        }
        else if(boxType == ProductIdentityEnums.Type.Blue)
        {
            boxObject = new BoxObject(boxType) {dropOffTargetVector3 = blueTargetDropOffLocation};
        }
    }

    void Update()
    {
        DrawLineToTarget();
    }

    private void SetupLineRenderer()
    {
        targetLine.positionCount = 2;
        targetLine.startWidth = 0.05f;
        targetLine.endWidth = 0.05f;
        
        // Use a simple built-in unlit material so it doesn't look pink/missing
        targetLine.material = new Material(Shader.Find("Sprites/Default")); 
    }

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
