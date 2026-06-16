using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class AgentSpawner : MonoBehaviour
{
    [Header("Prefab & Layout")]
    public GameObject areaPrefab;
    
    public int rows = 4;
    public int cols = 4;
    
    public float spacing = 20f;
    
    private List<Agent> agents = new List<Agent>();

    void Start()
    {
        if (areaPrefab == null)
        {
            Debug.LogError("TrainingAreaSpawner: areaPrefab is not assigned.");
            return;
        }
        
        SpawnGrid();
    }

    void SpawnGrid()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 pos = new Vector3(r * spacing, 0f, c * spacing);
                Quaternion rot = Quaternion.identity;
                GameObject areaInstance = Instantiate(areaPrefab, pos, rot, transform);

                // Give each instance a unique name for easier debugging
                areaInstance.name = $"{areaPrefab.name}_r{r}_c{c}";

                // Spawn an agent directly under the area prefab
                GameObject agentPrefab = Resources.Load<GameObject>("AgentPrefab"); // Replace with your agent prefab path
                if (agentPrefab != null)
                {
                    GameObject agentInstance = Instantiate(agentPrefab, Vector3.zero, Quaternion.identity, areaInstance.transform);

                    // Give each instance a unique name for easier debugging
                    agentInstance.name = $"{agentPrefab.name}_area_{areaInstance.name}";

                    Agent agent = agentInstance.GetComponent<Agent>();
                    if (agent != null)
                    {
                        agents.Add(agent);
                    }
                }
            }
        }
    }
}