using UnityEngine;

public class BoxObject
{
    public int boxType;
    public Vector3 dropOffTargetTransform;

    public BoxObject(int type)
    {
        boxType = type;
        dropOffTargetTransform = Vector3.zero;
    }
}
