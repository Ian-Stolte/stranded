using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class StatTracker : MonoBehaviour
{
    public float totalTime;
    public float resourcesCollected;
    public int scrapsCollected;
    public string causeOfDeath;

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
        if (scene.name == "Game Over" || scene.name == "Win Screen")
        {
            Time.timeScale = 1;
            string formattedTime;
            if (totalTime < 60)
            {
                formattedTime = Mathf.Round(totalTime) + "s";
            }
            else
            {
                string seconds = "" + Mathf.Round(totalTime % 60);
                if (seconds.Length == 1)
                    seconds = "0" + seconds;
                formattedTime = Mathf.Round(totalTime / 60) + ":" + seconds;
            }
            GameObject.Find("Total Time").GetComponent<TMPro.TextMeshProUGUI>().text = "You survived for " + formattedTime;
            GameObject.Find("Resource Text").GetComponent<TMPro.TextMeshProUGUI>().text = "Resources: " + resourcesCollected;
            GameObject.Find("Scrap Text").GetComponent<TMPro.TextMeshProUGUI>().text = "Scraps: " + scrapsCollected;
            if (scene.name == "Game Over")
                GameObject.Find("Cause of Death").GetComponent<TMPro.TextMeshProUGUI>().text = causeOfDeath;
        }
        else if (scene.name == "Multiplayer")
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += InitializeVals;
        }
    }

    void InitializeVals(
        string sceneName,
        UnityEngine.SceneManagement.LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut)
    {
        totalTime = 0;
        resourcesCollected = 0;
        scrapsCollected = 0;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= InitializeVals;
    }

    void Update()
    {
        totalTime += Time.deltaTime;
    }
}
