using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Based on shop tutorial by Flarvain on YouTube
// Link: https://www.youtube.com/watch?v=kUwnfkYcaFU
public class ShopManager : MonoBehaviour
{
    public GameObject shop;
    private GameObject ship;
    private Spaceship shipScript;
    [HideInInspector] public PlayerStations player;

    public TMP_Text scrapsText;
    public BoostEffect[] boostEffectsSO;
    [SerializeField] GameObject[] shopPanelsGO;
    public ShopTemplate[] shopPanels;
    public Button[] myPurchaseBtns;

    public GameObject openShopBtn;
    public GameObject closeShopBtn;

    private AudioManager audio;
    private bool startMusic;

    void Start()
    {
        audio = GameObject.Find("Audio Manager").GetComponent<AudioManager>();

        openShopBtn.SetActive(true);
        closeShopBtn.SetActive(false);

        ship = GameObject.Find("Spaceship");
        shipScript = ship.GetComponent<Spaceship>();  

        CloseShop();
        AddScraps();  
        Debug.Log("Number of elements in shopPanelsGO: " + shopPanelsGO.Length);   
        LoadPanels();
        // CheckPurchaseable();
    }

    void Update()
    {
        if (Physics2D.OverlapCircle(GameObject.Find("Spaceship").transform.position, 8, LayerMask.GetMask("Shop")) && !shop.activeSelf)
        {
            openShopBtn.SetActive(true);
            if (Input.GetKeyDown(KeyCode.E))
                OpenShop();
        }
        else if ((Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape)) && shop.activeSelf)
        {
            CloseShop();
        }
        else if (!Physics2D.OverlapCircle(GameObject.Find("Spaceship").transform.position, 12, LayerMask.GetMask("Shop")))
        {
            CloseShop();
        }
        else
        {
            openShopBtn.SetActive(false);
        }

        //audio fade
        float distance = Vector2.Distance(transform.position, GameObject.Find("Spaceship").transform.position);
        Sound shopMusic = Array.Find(audio.music, sound => sound.name == "Shop");
        Sound strandedMusic = Array.Find(audio.music, sound => sound.name == "Stranded");
        if (distance < 50)
        {
            if (startMusic)
            {
                shopMusic.source.volume = 0.4f * (50 - distance)/50;
                if (!shop.activeSelf)
                    strandedMusic.source.volume = 0.2f + 0.2f*(distance/50);
            }
        }
        else {
            startMusic = true;
            shopMusic.source.volume = 0;
            strandedMusic.source.volume = 0.4f;
        }
    }

    public void OpenShop()
    {
        shop.SetActive(true);
        openShopBtn.SetActive(false);
        closeShopBtn.SetActive(true);
        AddScraps();
        // player.currentStation = "none";
        // player.HideInstructions();
        if (startMusic)
        {
            Sound s = Array.Find(audio.music, sound => sound.name == "Stranded");
            s.source.volume = 0;
        }
        GameObject.Find("Sync Object").GetComponent<Sync>().PauseServerRpc(false);
    }

    public void CloseShop()
    {
        if (shop.activeSelf)
            GameObject.Find("Sync Object").GetComponent<Sync>().PauseServerRpc(false);
        shop.SetActive(false);
        openShopBtn.SetActive(Physics2D.OverlapCircle(GameObject.Find("Spaceship").transform.position, 8, LayerMask.GetMask("Shop")));
        closeShopBtn.SetActive(false);
    }

    public void AddScraps()
    {
        scrapsText.text =  "Scraps: " + shipScript.scraps.Value;
        // CheckPurchaseable();
    }

    public void LoadPanels()
    {
        for (int i=0; i < boostEffectsSO.Length; i++)
        {
            shopPanelsGO[i].SetActive(true);
        }

        for (int i = 0; i < boostEffectsSO.Length; i++)
        {
            shopPanels[i].itemName.text = boostEffectsSO[i].itemName;
            shopPanels[i].itemDescription.text = boostEffectsSO[i].itemDescription;
            shopPanels[i].itemPrice.text =  boostEffectsSO[i].baseCost.ToString() + " Scraps";
        }
    }

    public void CheckPurchaseable()
    {
        Debug.Log("Array length: " + boostEffectsSO.Length);
        for (int i = 0; i < boostEffectsSO.Length; i++)
        {
            Debug.Log("Cost: " + boostEffectsSO[i].baseCost);
            if (shipScript.scraps.Value >= boostEffectsSO[i].baseCost) // If player has enough money
            {
                Debug.Log("Yay");
                myPurchaseBtns[i].interactable = true;
            } 
            else 
            {
                myPurchaseBtns[i].interactable = false;
            }
        }
    }

    public void PurchaseItem(int btnNo)
    {
        if (shipScript.scraps.Value >= boostEffectsSO[btnNo].baseCost)
        {
            shipScript.scraps.Value = shipScript.scraps.Value - boostEffectsSO[btnNo].baseCost;
            AddScraps();
        }
    }
}