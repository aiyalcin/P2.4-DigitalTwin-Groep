using UnityEngine;
using System.Collections.Generic;

public class CellManager : MonoBehaviour
{
    [SerializeField] private ScoringSettings scoringParameters;
    [SerializeField] private GameObject redDropOffLocation;
    [SerializeField] private GameObject blueDropOffLocation;
    [SerializeField] private DelegateData delegateData;
    private DropoffZoneScript redDropoffZoneScript;
    private DropoffZoneScript blueDropoffZoneScript;
    [SerializeField] private GameObject MLAgentGameObject;
    private MLAgentScript mLAgentScript;
    private float previousDistanceToTarget = Mathf.Infinity;
    private int boxesDelivered = 0;
    private float totalStepPenalty = 0f;
    private float totalDistanceReward = 0f;
    private float totalBoxPickupReward = 0f;
    private float totalDropoffReward = 0f;
    private float totalOutOfBoundsPenalty = 0f;
    private float TotalReward = 0f;
    private int correctDeliveries;
    private int wrongDeliveries;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mLAgentScript = MLAgentGameObject.GetComponent<MLAgentScript>();
        redDropoffZoneScript = redDropOffLocation.GetComponent<DropoffZoneScript>();
        blueDropoffZoneScript = blueDropOffLocation.GetComponent<DropoffZoneScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void BoundsHit()
    {
        mLAgentScript.AddReward(scoringParameters.OutOfBoundsPenalty);
    }

    public void BoundsStay()
    {
        mLAgentScript.AddReward(scoringParameters.OutOfBoundsStayPenalty);
    }

    public List<Vector3> GetDropOffLocations()
    {
        List<Vector3> dropOffLocations = new List<Vector3>
        {
            redDropOffLocation.transform.position,
            blueDropOffLocation.transform.position
        };
        return dropOffLocations;
    }

    public void ClearOtherZones(DropoffZoneScript callingZone)
    {
        if (redDropoffZoneScript != callingZone) {redDropoffZoneScript.ClearBox();}
        if (blueDropoffZoneScript != callingZone) {blueDropoffZoneScript.ClearBox();}
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
        previousDistanceToTarget = currentDistanceToTarget;
    }

    public void BoxPickedUp()
    {
        mLAgentScript.AddReward(scoringParameters.PickupBoxReward);
        ResetDistanceTracking();
    }

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

    public void ResetDistanceTracking()
    {
        previousDistanceToTarget = Mathf.Infinity;
    }
}
