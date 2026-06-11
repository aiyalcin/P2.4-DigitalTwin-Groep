using UnityEngine;

public class BoxObject : ScriptableObject
{
    // Type/index of this box (maps to a dropoff)
    public bool boxType;  // (blue / red)
    // Transform reference to the target dropoff location
    public Vector3 dropOffTargetTransform;
}
