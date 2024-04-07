using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Based on shop tutorial by Flarvain on YouTube
// Link: https://www.youtube.com/watch?v=kUwnfkYcaFU
public class ShopManager : MonoBehaviour
{
    [SerializeField] GameObject shop;
    private GameObject ship;
    private Spaceship shipScript;
    public TMP_Text scrapsText;
    public BoostEffect[] boostEffectsSO;
    public GameObject[] shopPanelsGO;
    public ShopTemplate[] shopPanels;

    public GameObject openShopBtn;
    public GameObject closeShopBtn;

    void Start()
    {
        shop = GameObject.Find("Shop");
        CloseShop();

        openShopBtn.SetActive(true);
        closeShopBtn.SetActive(false);

        for (int i=0; i < boostEffectsSO.Length; i++)
        {
            shopPanelsGO[i].SetActive(true);
        }

        shipScript = GameObject.Find("Spaceship").GetComponent<Spaceship>();  
        AddScraps();     
        LoadPanels();
    }

    public void OpenShop()
    {
        shop.SetActive(true);
        openShopBtn.SetActive(false);
        closeShopBtn.SetActive(true);
    }

    public void CloseShop()
    {
        shop.SetActive(false);
        openShopBtn.SetActive(true);
        closeShopBtn.SetActive(false);
    }

    public void AddScraps()
    {
        scrapsText.text =  "Scraps: " + shipScript.scrapsCollected.ToString();
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
