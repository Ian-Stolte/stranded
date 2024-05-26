using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class ButtonFunctions : NetworkBehaviour
{
    [SerializeField] private GameObject loadingScreen;

    public void SetSingleplayer()
    {
        GameObject.Find("Scene Loader").GetComponent<SceneLoader>().singleplayer = true;
    }

    [Rpc(SendTo.Server)]
    public void ClickButtonServerRpc(int n)
    {
        GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Button Press " + n);
        ClickButtonClientRpc(n);
    }

    [Rpc(SendTo.NotServer)]
    private void ClickButtonClientRpc(int n)
    {
        GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Button Press " + n);
    }

    [Rpc(SendTo.Server)]
    public void LoadSceneServerRpc(string name)
    {
        if (name == "Multiplayer")
        {
            loadingScreen.SetActive(true);
            LoadingScreenClientRpc();
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

    [Rpc(SendTo.NotServer)]
    private void LoadingScreenClientRpc()
    {
        loadingScreen.SetActive(true);
    }
}