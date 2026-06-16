using UnityEngine;

public class DropoffZoneScript : MonoBehaviour
{
    public ProductIdentityEnums.Type identity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool CheckDropoff(ProductIdentityEnums.Type boxType)
    {
        if (boxType == identity)
        {
            return true;
        }
        return false;
    }
}
