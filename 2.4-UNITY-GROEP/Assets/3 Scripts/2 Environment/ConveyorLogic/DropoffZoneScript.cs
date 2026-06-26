using UnityEngine;

public class DropoffZoneScript : MonoBehaviour
{
    [Tooltip("The product type this zone accepts as a correct delivery.")]
    public ProductIdentityEnums.Type identity;

    [Tooltip("Anchor point where the delivered product is placed on arrival.")]
    [SerializeField] private Transform boxAnchor;

    [Tooltip("Reference to the CellManager GameObject for reporting delivery results.")]
    [SerializeField] private GameObject CellManager;

    private CellManager cellManagerScript;
    private GameObject heldBox;
    
    private bool isOccupied = false;

    void Start()
    {
        cellManagerScript = CellManager.GetComponent<CellManager>();
    }

    /// <summary>
    /// Returns true if the given box type matches the identity of this drop-off zone.
    /// </summary>
    /// <param name="boxType">The type of the product being delivered.</param>
    /// <returns>True if the delivery is correct, false otherwise.</returns>
    public bool CheckDropoff(ProductIdentityEnums.Type boxType)
    {
        if (boxType == identity)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Destroys the currently displayed box and marks the zone as unoccupied.
    /// Called by CellManager when the agent delivers to the other zone.
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
    /// Handles an agent entering the trigger. Transfers the held product to this zone,
    /// checks whether the delivery is correct and reports the result to CellManager.
    /// </summary>
    /// <param name="collider">The collider that entered the trigger zone.</param>
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
