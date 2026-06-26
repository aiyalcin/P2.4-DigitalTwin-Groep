using UnityEngine;

public class BoxObject
{
    public ProductIdentityEnums.Type boxType;
    public Vector3 dropOffTargetVector3;

    public BoxObject(ProductIdentityEnums.Type type)
    {
        boxType = type;
        dropOffTargetVector3 = Vector3.zero;
    }
}
