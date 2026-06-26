using UnityEngine;

public class DropoffZoneScript : MonoBehaviour
{
    public ProductIdentityEnums.Type identity;
    
    [SerializeField] private Transform boxAnchor;
    [SerializeField] private GameObject CellManager;
    CellManager cellManagerScript;
    private GameObject heldBox;
    
    private bool isOccupied = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cellManagerScript = CellManager.GetComponent<CellManager>();
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

    public void ClearBox()
    {
        if (isOccupied && heldBox != null)
        {
            Destroy(heldBox);
            heldBox = null;
            isOccupied = false;
        }
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
            cellManagerScript.ClearOtherZones(this);
    
            MLAgentScript agent = obj.GetComponent<MLAgentScript>();
            heldBox = agent.PassBox(boxAnchor);
            if (CheckDropoff(heldBox.GetComponent<ProductIdentity>().identity))
            {
                cellManagerScript.DropoffHit(true);
            }
            else
            {
                cellManagerScript.DropoffHit(false);
            }
            isOccupied = true;
        }
    }
}
