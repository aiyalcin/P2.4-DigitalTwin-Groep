using UnityEngine;
using System.Collections.Generic;

public class CellManager : MonoBehaviour
{
    [SerializeField] private ScoringParameters scoringParameters;
    [SerializeField] private Transform redDropOffLocation;
    [SerializeField] private Transform blueDropOffLocation;
    [SerializeField] private GameObject MLAgentGameObject;
    private MLAgentScript mLAgentScript;
    private float previousDistanceToTarget = Mathf.Infinity;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mLAgentScript = MLAgentGameObject.GetComponent<MLAgentScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BoundsHit()
    {
        mLAgentScript.AddReward(scoringParameters.OutOfBoundsPenalty);
        mLAgentScript.EndEpisode();
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

    public void ActionRecievedCall()
    {
        mLAgentScript.AddReward(scoringParameters.StepPenalty);

        float currentDistanceToTarget = Vector3.Distance(mLAgentScript.transform.position, mLAgentScript.GetActiveTargetPosition());

        if (previousDistanceToTarget == Mathf.Infinity)
        {
            previousDistanceToTarget = currentDistanceToTarget;
            return;
        }

        float distanceDelta = previousDistanceToTarget - currentDistanceToTarget;
        mLAgentScript.AddReward(distanceDelta * scoringParameters.ProgressRewardScale);
        Debug.Log($"Applied reward: {distanceDelta * scoringParameters.ProgressRewardScale}");
        previousDistanceToTarget = currentDistanceToTarget;
    }

    public void BoxPickedUp()
    {
        mLAgentScript.AddReward(scoringParameters.PickupBoxReward);
        Debug.Log($"Applied reward: {scoringParameters.PickupBoxReward}");
    }

    public void DropoffHit(bool isCorrect)
    {
        mLAgentScript.AddReward(isCorrect ? scoringParameters.CorrectBoxDeliveryReward : scoringParameters.WrongBoxDeliveryPenalty);
        Debug.Log($"Applied reward: " + (isCorrect ? scoringParameters.CorrectBoxDeliveryReward : scoringParameters.WrongBoxDeliveryPenalty));
        mLAgentScript.EndEpisode();
    }

    public void ResetDistanceTracking()
    {
        previousDistanceToTarget = Mathf.Infinity;
    }
}
