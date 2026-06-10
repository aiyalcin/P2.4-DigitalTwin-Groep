using UnityEngine;

public class ProductIdentity : MonoBehaviour
{
    public ProductIdentityEnums.Identity identity;
    public Vector3 location;

    private void Update()
    {
        location = transform.position; // not sure if this is neccesary
    }
}