using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
public class MLAgentScript : Agent
{
    Rigidbody agentRigidbody;
    public override void OnEpisodeBegin()
    {
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agentRigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // Set the agent's rotation to face the direction of movement
        if(agentRigidbody != null && agentRigidbody.linearVelocity.magnitude > 0.1f) // Check if the agent is moving to avoid jittering when stationary
        {
            agentRigidbody.rotation = Quaternion.LookRotation(agentRigidbody.linearVelocity);
        }
    }
}
