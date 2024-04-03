using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Based on shop tutorial by Flarvain on YouTube
// Link: https://www.youtube.com/watch?v=kUwnfkYcaFU
public class ShopManager : MonoBehaviour
{
    private GameObject shop;
    private GameObject ship;
    private Spaceship shipScript;
    public TMP_Text scrapsText;


    void Start()
    {
        shop = GameObject.Find("Shop");
        shop.SetActive(false);

        shipScript = GameObject.Find("Spaceship").GetComponent<Spaceship>();  
        AddScraps();     
    }

    public void AddScraps()
    {
        scrapsText.text =  "Scraps: " + shipScript.scrapsCollected.ToString();
    }

}
