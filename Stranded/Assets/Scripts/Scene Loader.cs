using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneByName()
    {
        Debug.Log("Hello world");
        SceneManager.LoadScene(0);
    }
}
