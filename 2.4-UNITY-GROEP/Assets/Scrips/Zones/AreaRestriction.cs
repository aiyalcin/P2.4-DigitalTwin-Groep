using UnityEngine;

public class AreaRestriction : MonoBehaviour
{
    public Vector3 minBounds = new Vector3(-10, 0, -10);
    public Vector3 maxBounds = new Vector3(10, 5, 10);

    void LateUpdate()
    {
        // Get the current position of the agent
        Vector3 currentPosition = transform.position;

        // Restrict the agent's X coordinate within the bounds
        if (currentPosition.x < minBounds.x)
            currentPosition.x = minBounds.x;
        else if (currentPosition.x > maxBounds.x)
            currentPosition.x = maxBounds.x;

        // Restrict the agent's Y coordinate within the bounds
        if (currentPosition.y < minBounds.y)
            currentPosition.y = minBounds.y;
        else if (currentPosition.y > maxBounds.y)
            currentPosition.y = maxBounds.y;

        // Restrict the agent's Z coordinate within the bounds
        if (currentPosition.z < minBounds.z)
            currentPosition.z = minBounds.z;
        else if (currentPosition.z > maxBounds.z)
            currentPosition.z = maxBounds.z;

        // Apply the restricted position back to the agent
        transform.position = currentPosition;
    }
}