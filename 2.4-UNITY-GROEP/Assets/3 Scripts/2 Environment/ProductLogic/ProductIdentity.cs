using UnityEngine;

public class ProductIdentity : MonoBehaviour
{
    [Tooltip("The product type of this object. Used by the agent and drop-off zones to identify and sort correctly.")]
    public ProductIdentityEnums.Type identity;
}