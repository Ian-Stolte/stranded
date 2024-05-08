using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class ButtonFunctions : NetworkBehaviour
{
    public void SetSingleplayer()
    {
        GameObject.Find("Scene Loader").GetComponent<SceneLoader>().singleplayer = true;
    }

    public void LoadScene(string name)
    {
        if (name == "Multiplayer")
        {
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
            {
                g.GetComponent<PlayerStations>().enabled = true;
            }
            GameObject.Find("Scene Loader").GetComponent<SceneLoader>().difficulty.Value = (int)GameObject.Find("Slider").GetComponent<Slider>().value;
        }
        else
        {
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
            {
                g.GetComponent<PlayerStations>().finishedSetup = false;
            }
        }
        if (name == "Start Screen")
        {
            GameObject.Find("Scene Loader").GetComponent<SceneLoader>().alreadyConnected = true;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(name, LoadSceneMode.Single);
    }
}