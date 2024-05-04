using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class SceneLoader : NetworkBehaviour
{
    public bool singleplayer;
    public NetworkVariable<int> difficulty;
    private string[] difficultyText = new string[] { "Easy", "Normal", "Hard", "Expert" };
    public Color[] difficultyColors = new Color[4];

    public void SetSingleplayer()
    {
        singleplayer = true;
    }

    public void LoadScene(string name)
    {
        if (name == "Multiplayer")
        {
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
            {
                g.GetComponent<PlayerStations>().enabled = true;
            }
            difficulty.Value = (int)GameObject.Find("Slider").GetComponent<Slider>().value;
        }
        NetworkManager.Singleton.SceneManager.LoadScene(name, LoadSceneMode.Single);
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
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += DoSetup;
        }
    }

    void DoSetup(
        string sceneName,
        UnityEngine.SceneManagement.LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut
    )
    {
        //Set up owned players first
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
        GameObject pauseMenu = GameObject.Find("Sync Object").GetComponent<Sync>().pauseMenu;
        pauseMenu.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = difficultyText[difficulty.Value];
        pauseMenu.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().color = difficultyColors[difficulty.Value];
        pauseMenu.transform.GetChild(2).gameObject.SetActive(singleplayer);
        GameObject.Find("Spaceship").GetComponent<Spaceship>().SetupDifficulty(difficulty.Value);
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= DoSetup;
    }
}
