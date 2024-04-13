using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class StatTracker : NetworkBehaviour
{
    private float totalTime;
    public int resourcesCollected;
    public int scrapsCollected;

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
            if (totalTime < 60)
            {
                formattedTime = Mathf.Round(totalTime) + "s";
            }
            else
            {
                formattedTime = Mathf.Round(totalTime / 60) + ":" + Mathf.Round(totalTime % 60);
            }
            GameObject.Find("Total Time").GetComponent<TMPro.TextMeshProUGUI>().text = "You survived for " + formattedTime;
            GameObject.Find("Resource Text").GetComponent<TMPro.TextMeshProUGUI>().text = "Resources: " + resourcesCollected;
            GameObject.Find("Scrap Text").GetComponent<TMPro.TextMeshProUGUI>().text = "Scraps: " + scrapsCollected;
        }
    }

    void Update()
    {
        if (IsServer)
        {
            totalTime += Time.deltaTime;
        }
    }
}
