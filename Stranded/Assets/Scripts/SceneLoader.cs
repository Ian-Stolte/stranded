using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class SceneLoader : NetworkBehaviour
{
    public void LoadScene(string name)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(name, LoadSceneMode.Single);
    }
}
