using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "FactorySettings", menuName = "Settings/Factory")]
public class FactorySettings : ScriptableObject
{
    [Header("Conveyor Information")]

    [Tooltip("Local position representing the final destination.")]
    public Vector3 destination;

    [Tooltip("Local position representing the spawn destination.")]
    public Vector3 spawnPosition;

    [Tooltip("Maximum number of products that can exist on the conveyor at the same time.")]
    public int slotCount;

    [Tooltip("Movement speed at which products travel along the conveyor.")]
    public float speed;

    [Tooltip("Distance between each product slot on the conveyor.")]
    public float slotDistance;


    [Header("Product Information")]

    [Tooltip("List of possible product prefabs that can be randomly selected for spawning.")]
    public List<GameObject> productOptions = new List<GameObject>();
}