using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class StatTracker : NetworkBehaviour
{
    public NetworkVariable<float> totalTime;
    public NetworkVariable<int> resourcesCollected;
    public NetworkVariable<int> scrapsCollected;

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
        if (scene.name == "Game Over")
        {
            string formattedTime;
            if (totalTime.Value < 60)
            {
                formattedTime = Mathf.Round(totalTime.Value) + "s";
            }
            else
            {
                formattedTime = Mathf.Round(totalTime.Value / 60) + ":" + Mathf.Round(totalTime.Value % 60);
            }
            GameObject.Find("Total Time").GetComponent<TMPro.TextMeshProUGUI>().text = "You survived for " + formattedTime;
            GameObject.Find("Resource Text").GetComponent<TMPro.TextMeshProUGUI>().text = "Resources: " + resourcesCollected.Value;
            GameObject.Find("Scrap Text").GetComponent<TMPro.TextMeshProUGUI>().text = "Scraps: " + scrapsCollected.Value;
        }
        else if (scene.name == "Multiplayer")
        {
            totalTime.Value = 0;
            resourcesCollected.Value = 0;
            scrapsCollected.Value = 0;
        }
    }

    void Update()
    {
        if (IsServer)
        {
            totalTime.Value += Time.deltaTime;
        }
    }
}
