using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class ButtonFunctions : NetworkBehaviour
{
    public NetworkVariable<bool> doingText;
    public NetworkVariable<float> introTimer;
    public NetworkVariable<float> winTimer;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject introBackground;
    [SerializeField] private GameObject winBackground;
    private SceneLoader sceneLoader;

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
        sceneLoader = GameObject.Find("Scene Loader").GetComponent<SceneLoader>();
        if (SceneManager.GetActiveScene().name == "Win Screen")
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += StartWinText;
        }
    }

    void StartWinText(
        string sceneName,
        UnityEngine.SceneManagement.LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut
    )
    {
        if (IsServer)
        {
            doingText.Value = true;
            winTimer.Value = 0;
        }
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= StartWinText;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && doingText.Value && (winTimer.Value > 0.5f && introTimer.Value > 0.5f))
            SkipServerRpc();

        if (doingText.Value)
        {
            if (winTimer.Value < 15 || introTimer.Value < 25)
            {
                if (IsServer)
                {
                    winTimer.Value = Mathf.Min(9999, winTimer.Value + Time.deltaTime);
                    introTimer.Value = Mathf.Min(9999, introTimer.Value + Time.deltaTime);
                }
                //Win Text
                if (Mathf.Abs(winTimer.Value-0.5f) < 0.05f)
                    StartCoroutine(FadeText(GameObject.Find("Text 1"), 6));
                else if (Mathf.Abs(winTimer.Value-2.5f) < 0.05f)
                    StartCoroutine(FadeText(GameObject.Find("Text 2"), 4));
                else if (Mathf.Abs(winTimer.Value-7.5f) < 0.05f)
                    StartCoroutine(FadeText(GameObject.Find("Text 3"), 7.5f));
                else if (Mathf.Abs(winTimer.Value-11f) < 0.05f)
                    StartCoroutine(FadeText(GameObject.Find("Text 4"), 4));

                //Intro Text
                if (Mathf.Abs(introTimer.Value-0.5f) < 0.05f)
                    StartCoroutine(FadeText(GameObject.Find("Text 1"), 6));
                else if (Mathf.Abs(introTimer.Value-7f) < 0.05f)
                    StartCoroutine(FadeText(GameObject.Find("Text 2"), 6));
                else if (Mathf.Abs(introTimer.Value-9f) < 0.05f)
                    StartCoroutine(FadeText(GameObject.Find("Text 3"), 4));
                else if (Mathf.Abs(introTimer.Value-13.5f) < 0.05f)
                    StartCoroutine(FadeText(GameObject.Find("Text 4"), 6));
                else if (Mathf.Abs(introTimer.Value-15.5f) < 0.05f)
                    StartCoroutine(FadeText(GameObject.Find("Text 5"), 4));
                else if (Mathf.Abs(introTimer.Value-19.5f) < 0.05f)
                    StartCoroutine(FadeText(GameObject.Find("Text 6"), 6));
            }
            else
            {
                EndStuffServerRpc();
            }
        }
    }

    [Rpc(SendTo.Server)]
    void EndStuffServerRpc()
    {
        doingText.Value = false;
        sceneLoader.introComplete = true;
        GameObject.Find("Fader").GetComponent<Animator>().Play("FadeIn");
        if (winBackground != null)
            winBackground.SetActive(false);
        if (introBackground != null)
            introBackground.SetActive(false);
        for (int i = 1; i < 7; i++)
        {
            if (GameObject.Find("Text " + i) != null)
                GameObject.Find("Text " + i).SetActive(false);
        }
        EndStuffClientRpc();
    }
    
    [Rpc(SendTo.NotServer)]
    void EndStuffClientRpc()
    {
        sceneLoader.introComplete = true;
        GameObject.Find("Fader").GetComponent<Animator>().Play("FadeIn");
        if (winBackground != null)
            winBackground.SetActive(false);
        if (introBackground != null)
            introBackground.SetActive(false);
        for (int i = 1; i < 7; i++)
        {
            if (GameObject.Find("Text " + i) != null)
                GameObject.Find("Text " + i).SetActive(false);
        }
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
        if (name != "Multiplayer")
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
                if (IsServer)
                {
                    introTimer.Value = 0;
                    doingText.Value = true;
                }
            }
            yield return new WaitForSeconds(0.1f);
            yield return new WaitUntil(() => doingText.Value == false);
            loadingScreen.SetActive(true);
            LoadingScreenClientRpc();
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
        winTimer.Value = 9999;
        introTimer.Value = 9999;
        GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Button Press 1");
        ClickButtonClientRpc(1);
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