using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class ButtonFunctions : NetworkBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject introBackground;
    private SceneLoader sceneLoader;

    void Start()
    {
        sceneLoader = GameObject.Find("Scene Loader").GetComponent<SceneLoader>();
    }

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
        GameObject.Find("Fader").GetComponent<Animator>().Play("FadeOut");
        StartCoroutine(LoadSceneCor(name));
    }

    IEnumerator LoadSceneCor(string name)
    {
        Time.timeScale = 1;
        if (name == "Multiplayer")
        {
            if (!sceneLoader.introComplete)
            {
                introBackground.SetActive(true);
                bool skip = false;
                float i = 0;
                while (!skip && i < 7)
                {
                    if (Input.GetMouseButtonDown(0))
                        skip = true;
                    if (i == 0)
                    {
                        StartCoroutine(FadeText(GameObject.Find("Text 1"), 4));
                    }
                    else if (Mathf.Abs(i-2) < 0.1f)
                    {
                        StartCoroutine(FadeText(GameObject.Find("Text 2"), 3));
                    }
                    i += 0.01f;
                    yield return new WaitForSeconds(0.01f);
                    
                }
                introBackground.SetActive(false);
                sceneLoader.introComplete = true;
            }
            loadingScreen.SetActive(true);
            LoadingScreenClientRpc();
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
            {
                g.GetComponent<PlayerStations>().enabled = true;
            }
            sceneLoader.difficulty.Value = (int)GameObject.Find("Slider").GetComponent<Slider>().value;
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
            {
                g.GetComponent<PlayerStations>().finishedSetup = false;
            }
        }
        if (name == "Start Screen")
        {
            sceneLoader.alreadyConnected = true;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(name, LoadSceneMode.Single);
    }

    private IEnumerator FadeText(GameObject text, float duration)
    {
        for (float i = 0; i < 0.5f; i += 0.01f)
        {
            text.GetComponent<CanvasGroup>().alpha = i/0.3f;
            yield return new WaitForSeconds(0.01f);
        }
        yield return new WaitForSeconds(duration - 1);
        for (float i = 0; i < 0.5f; i += 0.01f)
        {
            text.GetComponent<CanvasGroup>().alpha = 1 - (i/0.3f);
            yield return new WaitForSeconds(0.01f);
        }
    }

    [Rpc(SendTo.NotServer)]
    private void LoadingScreenClientRpc()
    {
        loadingScreen.SetActive(true);
    }
}