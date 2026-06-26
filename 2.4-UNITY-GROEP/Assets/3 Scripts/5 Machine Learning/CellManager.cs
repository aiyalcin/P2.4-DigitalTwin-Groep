using UnityEngine;
using System.Collections.Generic;

public class CellManager : MonoBehaviour
{
    [Header("References")]

    [Tooltip("Reward and penalty values used for reinforcement learning.")]
    [SerializeField] private ScoringSettings scoringParameters;

    [Tooltip("GameObject containing the red product drop-off zone.")]
    [SerializeField] private GameObject appleDropOffLocation;

    [Tooltip("GameObject containing the blue product drop-off zone.")]
    [SerializeField] private GameObject pearDropOffLocation;

    [Tooltip("Tracks overall training statistics and success metrics.")]
    [SerializeField] private DelegateData delegateData;

    private DropoffZoneScript appleDropoffZoneScript;
    private DropoffZoneScript pearDropoffZoneScript;

    [Tooltip("Reference to the ML-Agent GameObject.")]
    [SerializeField] private GameObject MLAgentGameObject;

    private MLAgentScript mLAgentScript;

    // Stores the previous distance to the current target for progress-based rewards.
    private float previousDistanceToTarget = Mathf.Infinity;

    // Number of boxes delivered during the current episode.
    private int boxesDelivered = 0;

    // Total number of successful deliveries during the session.
    private int correctDeliveries;

    // Total number of incorrect deliveries during the session.
    private int wrongDeliveries;

    void Start()
    {
        mLAgentScript = MLAgentGameObject.GetComponent<MLAgentScript>();
        appleDropoffZoneScript = appleDropOffLocation.GetComponent<DropoffZoneScript>();
        pearDropoffZoneScript = pearDropOffLocation.GetComponent<DropoffZoneScript>();
    }

    /// <summary>
    /// Applies the penalty for colliding with the arena boundary.
    /// </summary>
    public void BoundsHit()
    {
        mLAgentScript.AddReward(scoringParameters.OutOfBoundsPenalty);
    }


    /// <summary>
    /// Applies the penalty for colliding with the arena boundary.
    /// </summary>
    public void BoundsStay()
    {
        mLAgentScript.AddReward(scoringParameters.OutOfBoundsStayPenalty);
    }

    /// <summary>
    /// Returns the world positions of all available drop-off locations.
    /// </summary>
    public List<Vector3> GetDropOffLocations()
    {
        List<Vector3> dropOffLocations = new List<Vector3>
        {
            appleDropOffLocation.transform.position,
            pearDropOffLocation.transform.position
        };
        return dropOffLocations;
    }

    /// <summary>
    /// Clears every drop-off zone except the one that initiated the call.
    /// </summary>
    /// <param name="callingZone">The drop-off zone.</param>
    public void ClearOtherZones(DropoffZoneScript callingZone)
    {
        if (appleDropoffZoneScript != callingZone) {appleDropoffZoneScript.ClearBox();}
        if (pearDropoffZoneScript != callingZone) {pearDropoffZoneScript.ClearBox();}
    }

    /// <summary>
    /// Applies the per-step penalty and rewards movement towards the active target.
    /// Called once for every action performed by the ML-Agent.
    /// </summary>
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
        previousDistanceToTarget = currentDistanceToTarget;
    }

    /// <summary>
    /// Rewards the agent for successfully picking up a box and resets distance tracking for the next navigation objective.
    /// </summary>
    public void BoxPickedUp()
    {
        mLAgentScript.AddReward(scoringParameters.PickupBoxReward);
        ResetDistanceTracking();
    }

    /// <summary>
    /// Handles reward assignment, statistics updates, and episode progression after a delivery attempt.
    /// </summary>
    /// <param name="isCorrect">True if the box was delivered to the correct drop-off zone.</param>
    public void DropoffHit(bool isCorrect)
    {
        if (isCorrect)
        {
            correctDeliveries++;
            delegateData.RegisterSuccess();
        }
        else
        {
            wrongDeliveries++;
            delegateData.RegisterFailure();
        }

        int total = correctDeliveries + wrongDeliveries;

        float successRate = total > 0  ? ((float)correctDeliveries / total) * 100f : 0f;

        delegateData.UpdateSuccessRate(successRate);

        mLAgentScript.AddReward(isCorrect ? scoringParameters.CorrectBoxDeliveryReward : scoringParameters.WrongBoxDeliveryPenalty);
        boxesDelivered++;

        if(boxesDelivered >= scoringParameters.BoxesPerEpisode)
        {
            mLAgentScript.EndEpisode();
            boxesDelivered = 0;
        }
    }

    /// <summary>
    /// Resets the stored distance so progress rewards begin from
    /// the current position towards the next objective.
    /// </summary>
    public void ResetDistanceTracking()
    {
        previousDistanceToTarget = Mathf.Infinity;
    }
}
