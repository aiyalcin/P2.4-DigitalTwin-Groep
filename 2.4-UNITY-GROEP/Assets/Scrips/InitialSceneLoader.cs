using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialSceneLoader : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene("Factory 1");
    }
}