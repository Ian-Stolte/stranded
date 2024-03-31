using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    private GameObject shop;
    void Start()
    {
        shop = GameObject.Find("Shop");
        shop.SetActive(false);        
    }

}
