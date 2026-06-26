using System;
using UnityEngine;


[CreateAssetMenu(fileName = "ScoringSettings", menuName = "Settings/Scoring")]
public class ScoringSettings : ScriptableObject
{
    [Header("General scoring")]

    [Tooltip("Penalty applied per action of the agent")]
    public float StepPenalty = -0.001f;

    [Tooltip("Penalty for delivering box at wrong location")]
    public float WrongBoxDeliveryPenalty = -1f;

    [Tooltip("Reward for delivering box at correct location")]
    public float CorrectBoxDeliveryReward = 1f;

    [Tooltip("Reward for successfully picking up a box")]
    public float PickupBoxReward = 1f;

    [Tooltip("Penalty for going out of bounds")]
    public float OutOfBoundsPenalty = -0.1f;

    [Tooltip("Penalty for staying out of bounds")]
    public float OutOfBoundsStayPenalty = -0.01f;


    [Header("Target proximity scoring")]

    [Tooltip("Scale at which proximity to target gives points")]
    public float ProgressRewardScale = 0.1f;


    [Header("ML Agent training parameters")]

    [Tooltip("Episode length in scored boxes")]
    public int BoxesPerEpisode = 10;
}