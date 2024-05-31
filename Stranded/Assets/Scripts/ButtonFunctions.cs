using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class ButtonFunctions : NetworkBehaviour
{
    public NetworkVariable<bool> skip;
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
        if (name != "Multiplayer")
            GameObject.Find("Fader").GetComponent<Animator>().Play("FadeOut");
        StartCoroutine(LoadSceneCor(name));
        LoadSceneClientRpc(name);
    }

    [Rpc(SendTo.NotServer)]
    private void LoadSceneClientRpc(string name)
    {
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
                float i = 0;
                while (!skip.Value && i < 21)
                {
                    if (Input.GetMouseButtonDown(0))
                        SkipServerRpc();
                    if (Mathf.Abs(i-0.5f) < 0.1f)
                        StartCoroutine(FadeText(GameObject.Find("Text 1"), 5));
                    else if (Mathf.Abs(i-6f) < 0.1f)
                        StartCoroutine(FadeText(GameObject.Find("Text 2"), 5));
                    else if (Mathf.Abs(i-8f) < 0.1f)
                        StartCoroutine(FadeText(GameObject.Find("Text 3"), 3));
                    else if (Mathf.Abs(i-11.5f) < 0.1f)
                        StartCoroutine(FadeText(GameObject.Find("Text 4"), 5));
                    else if (Mathf.Abs(i-13.5f) < 0.1f)
                        StartCoroutine(FadeText(GameObject.Find("Text 5"), 3));
                    else if (Mathf.Abs(i-16.5f) < 0.1f)
                        StartCoroutine(FadeText(GameObject.Find("Text 6"), 5));
                    i += 0.01f;
                    yield return new WaitForSeconds(0.01f);
                }
                if (skip.Value)
                    GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Button Press 1");
                introBackground.SetActive(false);
                sceneLoader.introComplete = true;
            }
            GameObject.Find("Fader").GetComponent<Animator>().Play("FadeIn");
            loadingScreen.SetActive(true);
            yield return new WaitForSeconds(1);
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
            {
                g.GetComponent<PlayerStations>().enabled = true;
            }
            if (IsServer)
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

        if (IsServer)
            NetworkManager.Singleton.SceneManager.LoadScene(name, LoadSceneMode.Single);
    }

    [Rpc(SendTo.Server)]
    private void SkipServerRpc()
    {
        skip.Value = true;
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