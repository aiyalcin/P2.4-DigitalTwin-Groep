using UnityEngine;

public class Box : MonoBehaviour
{
    // Type/index of this box (maps to a dropoff)
    public int boxType = 0;
    // ID/index of the dropoff this box should go to
    public int dropOffID = 0;
    // Transform reference to the target dropoff location (keeps mapping live if the dropoff moves)
    public Transform targetDropOff;
}
