using UnityEngine;

public class DropoffZoneScript : MonoBehaviour
{
    public ProductIdentityEnums.Type identity;
    
    [SerializeField] private Transform boxAnchor;
    [SerializeField] private GameObject CellManager;
    CellManager cellManagerScript;
    private GameObject heldBox;
    
    private bool isOccupied = false;

    void Start()
    {
        cellManagerScript = CellManager.GetComponent<CellManager>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="boxType"></param>
    /// <returns></returns>
    public bool CheckDropoff(ProductIdentityEnums.Type boxType)
    {
        if (boxType == identity)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void ClearBox()
    {
        if (isOccupied && heldBox != null)
        {
            Destroy(heldBox);
            heldBox = null;
            isOccupied = false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="collider"></param>
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
