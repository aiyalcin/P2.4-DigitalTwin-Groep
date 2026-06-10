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
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string actionMapName = "Player";
    [SerializeField] private string moveActionName = "Move";

    [Header("Movement")]
    [SerializeField] public float moveSpeed = 0.5f;
    [SerializeField] public float turnSpeed = 720f;
    [SerializeField] public float facingOffsetY = 90f;
    [Header("Observations")]
    [SerializeField] private float maxDistance = 20f; // used to normalize distances
    private InputAction moveAction;
    bool holdingBox = false;
    private Transform heldTarget = null;
    private int heldDropOffID = -1;

    
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

    void Setup()
    {   
        
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 1) Dropoff positions: for each dropoff provide agent-local position (3) and normalized distance (1)
        foreach (var position in dropoffPositions)
        {
            Vector3 localPos = transform.InverseTransformPoint(position.transform.position);
            sensor.AddObservation(localPos); // 3 floats
            float dist = Vector3.Distance(transform.position, position.transform.position);
            sensor.AddObservation(dist / maxDistance); // 1 float
        }

        // 2) holding flag
        sensor.AddObservation(holdingBox ? 1f : 0f);

        // 3) Held-target block (5 floats). If not holding, pad with zeros.
        if (holdingBox && heldTarget != null)
        {
            Vector3 localHeldTarget = transform.InverseTransformPoint(heldTarget.position);
            sensor.AddObservation(localHeldTarget);
            float heldDist = Vector3.Distance(transform.position, heldTarget.position);
            sensor.AddObservation(heldDist / maxDistance);
            float normalizedID = (dropoffPositions.Count > 1 && heldDropOffID >= 0) ? (float)heldDropOffID / (dropoffPositions.Count - 1) : 0f;
            sensor.AddObservation(normalizedID);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }

        // 4) Nearest-box block (5 floats). If holding, pad with zeros.
        if (!holdingBox)
        {
            Box nearest = null;
            float nearestDist = float.MaxValue;
            var boxes = FindObjectsOfType<Box>();
            foreach (var b in boxes)
            {
                if (b == null) continue;
                float d = Vector3.Distance(transform.position, b.transform.position);
                if (d < nearestDist)
                {
                    nearestDist = d;
                    nearest = b;
                }
            }

            if (nearest != null)
            {
                Vector3 localNearest = transform.InverseTransformPoint(nearest.transform.position);
                sensor.AddObservation(localNearest);
                sensor.AddObservation(nearestDist / maxDistance);
                float normalizedID = (dropoffPositions.Count > 1) ? (float)nearest.dropOffID / (dropoffPositions.Count - 1) : 0f;
                sensor.AddObservation(normalizedID);
            }
            else
            {
                sensor.AddObservation(Vector3.zero);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
        }
        else
        {
            // pad
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }

        // 5) Agent velocity (x,z)
        if (agentRigidbody != null)
        {
            sensor.AddObservation(agentRigidbody.velocity.x);
            sensor.AddObservation(agentRigidbody.velocity.z);
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector2 moveInput = new Vector2(actions.ContinuousActions[0], actions.ContinuousActions[1]);
        MoveAgent(moveInput);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        Vector2 moveInput = Vector2.zero;

        if (moveAction != null)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }

        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = moveInput.x;
        continuousActions[1] = moveInput.y;
    }

    public override void OnEpisodeBegin()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Call this with the Box component when the agent picks up a box
    public void PickupBox(Box box)
    {
        if (box == null) return;
        holdingBox = true;
        heldDropOffID = box.dropOffID;
        heldTarget = box.targetDropOff != null ? box.targetDropOff : (dropoffPositions.Count > box.dropOffID ? dropoffPositions[box.dropOffID].transform : null);
    }

    // Call when the agent drops/releases the box
    public void ReleaseBox()
    {
        holdingBox = false;
        heldDropOffID = -1;
        heldTarget = null;
    }

    private void MoveAgent(Vector2 moveInput)
    {
        Vector3 moveDirection = new Vector3(-moveInput.y, 0f, moveInput.x);

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        Vector3 movement = moveDirection * moveSpeed * Time.fixedDeltaTime;
        agentRigidbody.MovePosition(agentRigidbody.position + movement);

        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up) * Quaternion.Euler(0f, facingOffsetY, 0f);
            Quaternion nextRotation = Quaternion.RotateTowards(agentRigidbody.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
            agentRigidbody.MoveRotation(nextRotation);
        }
    }

}
