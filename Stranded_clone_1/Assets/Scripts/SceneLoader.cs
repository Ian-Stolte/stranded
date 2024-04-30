using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class SceneLoader : NetworkBehaviour
{
    public void LoadScene(string name)
    {
        if (name == "Multiplayer")
        {
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
            {
                g.GetComponent<PlayerStations>().enabled = true;
            }
        }
        NetworkManager.Singleton.SceneManager.LoadScene(name, LoadSceneMode.Single);
    }
}
