using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class SceneLoader : NetworkBehaviour
{
    public void LoadScene(string name)
    {
        int difficulty = 0;
        if (name == "Multiplayer")
        {
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
            {
                g.GetComponent<PlayerStations>().enabled = true;
            }
            difficulty = (int)GameObject.Find("Slider").GetComponent<Slider>().value;
        }
        NetworkManager.Singleton.SceneManager.LoadScene(name, LoadSceneMode.Single);
        Debug.Log("Difficulty is: " + difficulty);
    }

     void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Multiplayer")
        {
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (g.GetComponent<PlayerStations>().IsOwner)
                    g.GetComponent<PlayerStations>().Setup();
            }
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (!g.GetComponent<PlayerStations>().IsOwner)
                    g.GetComponent<PlayerStations>().Setup();
            }
        }
    }
}
