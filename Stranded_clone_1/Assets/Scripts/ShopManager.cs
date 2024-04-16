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
    public GameObject[] shopPanelsGO;
    public ShopTemplate[] shopPanels;

    public GameObject openShopBtn;
    public GameObject closeShopBtn;

    void Start()
    {
        CloseShop();

        openShopBtn.SetActive(true);
        closeShopBtn.SetActive(false);

        for (int i=0; i < boostEffectsSO.Length; i++)
        {
            shopPanelsGO[i].SetActive(true);
        }

        ship = GameObject.Find("Spaceship");
        shipScript = ship.GetComponent<Spaceship>();  
        AddScraps();     
        LoadPanels();
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
        Sound s = Array.Find(GameObject.Find("Audio Manager").GetComponent<AudioManager>().music, sound => sound.name == "Shop");
        if (distance < 50)
            s.source.volume = 0.4f * (50 - distance)/50;
        else
            s.source.volume = 0;
    }

    public void OpenShop()
    {
        shop.SetActive(true);
        openShopBtn.SetActive(false);
        closeShopBtn.SetActive(true);
        player.currentStation = "none";
        player.HideInstructions();
    }

    public void CloseShop()
    {
        shop.SetActive(false);
        openShopBtn.SetActive(Physics2D.OverlapCircle(GameObject.Find("Spaceship").transform.position, 8, LayerMask.GetMask("Shop")));
        closeShopBtn.SetActive(false);
    }

    public void AddScraps()
    {
        scrapsText.text =  "Scraps: " + shipScript.scraps.Value;
    }

    public void LoadPanels()
    {
        for (int i = 0; i < boostEffectsSO.Length; i++)
        {
            shopPanels[i].itemName.text = boostEffectsSO[i].itemName;
            shopPanels[i].itemDescription.text = boostEffectsSO[i].itemDescription;
            shopPanels[i].itemPrice.text =  boostEffectsSO[i].baseCost.ToString() + " Scraps";
        }
    }
}