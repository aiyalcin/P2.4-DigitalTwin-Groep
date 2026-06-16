using UnityEngine;

public class BoxObject
{
    public ProductIdentityEnums.Type boxType;
    public Vector3 dropOffTargetTransform;

    public BoxObject(ProductIdentityEnums.Type type)
    {
        boxType = type;
        dropOffTargetTransform = Vector3.zero;
    }
}
