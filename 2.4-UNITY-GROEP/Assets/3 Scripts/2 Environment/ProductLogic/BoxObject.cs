using UnityEngine;

public class BoxObject
{
    public ProductIdentityEnums.Type boxType;
    public Vector3 dropOffTargetVector3;

    /// <summary>
    /// Creates a new BoxObject for the given product type with the drop-off target initialised to zero.
    /// The target position should be assigned immediately after construction via dropOffTargetVector3.
    /// </summary>
    /// <param name="type">The type of product the agent has picked up.</param>
    public BoxObject(ProductIdentityEnums.Type type)
    {
        boxType = type;
        dropOffTargetVector3 = Vector3.zero;
    }
}
