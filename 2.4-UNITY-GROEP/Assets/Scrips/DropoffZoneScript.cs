using UnityEngine;

public class DropoffZoneScript : MonoBehaviour
{
    public ProductIdentityEnums.Type identity;
    [SerializeField] private Transform boxAnchor;
    private GameObject heldBox;
    private bool isOccupied = false;
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

    void OnTriggerEnter(Collider collider)
    {
        GameObject obj = collider.gameObject;
        if(isOccupied && obj.CompareTag("MLAgent") && obj.GetComponent<MLAgentScript>().currentState == MLAgentScript.State.CarryingBox)
        {
            Destroy(heldBox);
            isOccupied = false;
        }
        if (obj.CompareTag("MLAgent") && obj.GetComponent<MLAgentScript>().currentState == MLAgentScript.State.CarryingBox)
        {
            MLAgentScript agent = obj.GetComponent<MLAgentScript>();
            heldBox = agent.PassBox(boxAnchor);
            if (CheckDropoff(heldBox.GetComponent<ProductIdentity>().identity))
            {
                Debug.Log("Correct box dropped off");
                agent.AddReward(1.0f);
            }
            else
            {
                Debug.Log("Incorrect box dropped off");
                agent.AddReward(-1.0f);
            }
            isOccupied = true;
        }
    }
}
