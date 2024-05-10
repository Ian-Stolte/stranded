using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Partially based on shop tutorial by Flarvain on YouTube
// Link: https://www.youtube.com/watch?v=kUwnfkYcaFU
public class ShopManager : NetworkBehaviour
{
    public GameObject shop;
    private GameObject ship;
    private Spaceship shipScript;
    public GameObject fuelBar;
    public GameObject healthBar;
    [HideInInspector] public PlayerStations player;
    private Sync sync;

    public TMP_Text scrapsText;
    public BoostEffect[] boostEffectsSO;
    [SerializeField] GameObject[] shopPanelsGO;
    public ShopTemplate[] shopPanels;
    public Button[] myPurchaseBtns;

    public GameObject openShopBtn;
    public GameObject closeShopBtn;

    private AudioManager audio;
    private bool startMusic;

    private RectTransform fuelBarRectTransform;
    private RectTransform healthBarRectTransform;

    [Header("Shop Tabs")]
    public GameObject boostsPage;
    public GameObject upgradesPage;
    public GameObject cosmeticsPage;

    [Header("Upgrade Info")]
    [SerializeField] GameObject[] upgradePanels;
    [SerializeField] GameObject boostIndicator;

    //Level Info
    private string[] thrustInfo = new string[] {"Speed 3 → 4\nMax 8 → 10", "Unlocks periodic boosts of speed", "Speed 4 → 5\nMax 10 → 12", "Fully upgraded!"};
    private string[] radarInfo = new string[] {"Range 100 → 150", "Points toward all shipwrecks within range", "Range 150 → 200", "Fully upgraded!"};
    private string[] shieldInfo = new string[] { "Speed 1.3 → 2", "Width 4 → 6", "Speed 2 → 3", "Fully upgraded!" };
    
    void Start()
    {
        fuelBarRectTransform = fuelBar.GetComponent<RectTransform>();
        healthBarRectTransform = healthBar.GetComponent<RectTransform>();
        fuelBarRectTransform.anchoredPosition = new Vector2(-730f, -326f); // Set initial position of resource bars
        healthBarRectTransform.anchoredPosition = new Vector2(-730f, -423f);

        audio = GameObject.Find("Audio Manager").GetComponent<AudioManager>();

        openShopBtn.SetActive(true);
        closeShopBtn.SetActive(false);

        ship = GameObject.Find("Spaceship");
        shipScript = ship.GetComponent<Spaceship>();
        sync = GameObject.Find("Sync Object").GetComponent<Sync>();

        AddScraps();  
        LoadPanels();
    }

    void Update()
    {
        if (Physics2D.OverlapCircle(GameObject.Find("Spaceship").transform.position, 8, LayerMask.GetMask("Shop")) && !shop.activeSelf)
        {
            openShopBtn.SetActive(true);
            if (Input.GetKeyDown(KeyCode.E) && !sync.paused.Value)
            {
                OpenShopServerRpc();
            }
        }
        else if ((Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape)) && shop.activeSelf)
        {
            CloseShopServerRpc();
        }
        else if (!Physics2D.OverlapCircle(GameObject.Find("Spaceship").transform.position, 12, LayerMask.GetMask("Shop")) && shop.activeSelf)
        {
            CloseShopServerRpc();
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
    
    //OPEN SHOP
    [Rpc(SendTo.Server)]
    public void OpenShopServerRpc()
    {
        shop.SetActive(true);
        openShopBtn.SetActive(false);
        closeShopBtn.SetActive(true);

        GameObject.Find("Boosts Tab").GetComponent<Image>().color = new Color32(44,44,44,255);
        GameObject.Find("Upgrades Tab").GetComponent<Image>().color = new Color32(72,72,72,255);
        GameObject.Find("Cosmetics Tab").GetComponent<Image>().color = new Color32(72,72,72,255);

        boostsPage.SetActive(true);
        upgradesPage.SetActive(false);
        cosmeticsPage.SetActive(false);

        fuelBar.SetActive(true);
        healthBar.SetActive(true);
        
        fuelBarRectTransform.anchoredPosition = new Vector2(750f, 64f);
        healthBarRectTransform.anchoredPosition = new Vector2(750f, -19f);

        AddScraps();
        if (startMusic)
        {
            Sound s = Array.Find(audio.music, sound => sound.name == "Stranded");
            s.source.volume = 0;
        }
        sync.PauseServerRpc(false);
        OpenShopClientRpc();
    }

    [Rpc(SendTo.NotServer)]
    public void OpenShopClientRpc()
    {
        shop.SetActive(true);
        openShopBtn.SetActive(false);
        closeShopBtn.SetActive(true);

        GameObject.Find("Boosts Tab").GetComponent<Image>().color = new Color32(44,44,44,255);
        GameObject.Find("Upgrades Tab").GetComponent<Image>().color = new Color32(72,72,72,255);
        GameObject.Find("Cosmetics Tab").GetComponent<Image>().color = new Color32(72,72,72,255);

        boostsPage.SetActive(true);
        upgradesPage.SetActive(false);
        cosmeticsPage.SetActive(false);

        fuelBar.SetActive(true);
        healthBar.SetActive(true);

        fuelBarRectTransform.anchoredPosition = new Vector2(750f, 90f);
        healthBarRectTransform.anchoredPosition = new Vector2(750f, 7f);

        AddScraps();
        if (startMusic)
        {
            Sound s = Array.Find(audio.music, sound => sound.name == "Stranded");
            s.source.volume = 0;
        }
    }

    //CLOSE SHOP
    [Rpc(SendTo.Server)]
    public void CloseShopServerRpc()
    {
        sync.PauseServerRpc(false);
        shop.SetActive(false);
        
        openShopBtn.SetActive(Physics2D.OverlapCircle(GameObject.Find("Spaceship").transform.position, 8, LayerMask.GetMask("Shop")));
        closeShopBtn.SetActive(false);

        fuelBar.SetActive(true);
        healthBar.SetActive(true);
        fuelBarRectTransform.anchoredPosition = new Vector2(-730f, -326f); // Back to initial position of resource bars
        healthBarRectTransform.anchoredPosition = new Vector2(-730f, -423f);
        CloseShopClientRpc();
    }

    [Rpc(SendTo.NotServer)]
    public void CloseShopClientRpc()
    {
        shop.SetActive(false);
        openShopBtn.SetActive(Physics2D.OverlapCircle(GameObject.Find("Spaceship").transform.position, 8, LayerMask.GetMask("Shop")));
        closeShopBtn.SetActive(false);

        fuelBar.SetActive(true);
        healthBar.SetActive(true);
        fuelBarRectTransform.anchoredPosition = new Vector2(-730f, -326f); // Back to initial position of resource bars
        healthBarRectTransform.anchoredPosition = new Vector2(-730f, -423f);
    }

    public void AddScraps()
    {
        scrapsText.text =  "Scraps: " + shipScript.scraps.Value;
        CheckPurchaseable();
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
        for (int i = 0; i < boostEffectsSO.Length; i++)
        {
            if (shipScript.scraps.Value >= boostEffectsSO[i].baseCost) // If player has enough money
            {
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

            Debug.Log("You just bought " + boostEffectsSO[btnNo] + " for " + boostEffectsSO[btnNo].baseCost);
        }
    }
	
    public void ChangeTab(int tabNo)
    {
        if (tabNo == 1){
            // Debug.Log("Opening Boost Tab");
            GameObject.Find("Boosts Tab").GetComponent<Image>().color = new Color32(44,44,44,255);
            GameObject.Find("Upgrades Tab").GetComponent<Image>().color = new Color32(72,72,72,255);
            GameObject.Find("Cosmetics Tab").GetComponent<Image>().color = new Color32(72,72,72,255);

            boostsPage.SetActive(true);
            upgradesPage.SetActive(false);
            cosmeticsPage.SetActive(false);

            fuelBar.SetActive(true);
            healthBar.SetActive(true);
        } else if (tabNo == 2){
            // Debug.Log("Opening Upgrades Tab");
            GameObject.Find("Upgrades Tab").GetComponent<Image>().color = new Color32(44,44,44,255);
            GameObject.Find("Boosts Tab").GetComponent<Image>().color = new Color32(72,72,72,255);
            GameObject.Find("Cosmetics Tab").GetComponent<Image>().color = new Color32(72,72,72,255);

            boostsPage.SetActive(false);
            upgradesPage.SetActive(true);
            cosmeticsPage.SetActive(false);

            fuelBar.SetActive(false);
            healthBar.SetActive(false);
        } else if (tabNo == 3){
            // Debug.Log("Opening Cosmetics Tab");
            GameObject.Find("Cosmetics Tab").GetComponent<Image>().color = new Color32(44,44,44,255);
            GameObject.Find("Upgrades Tab").GetComponent<Image>().color = new Color32(72,72,72,255);
            GameObject.Find("Boosts Tab").GetComponent<Image>().color = new Color32(72,72,72,255);

            boostsPage.SetActive(false);
            upgradesPage.SetActive(false);
            cosmeticsPage.SetActive(true);
            
            fuelBar.SetActive(false);
            healthBar.SetActive(false);
        }
    }

    public void StationUpgrade(string stationUpgrade)
    {
        int cost = GameObject.Find(stationUpgrade).GetComponent<StationTemplate>().baseCost * GameObject.Find(stationUpgrade).GetComponent<StationTemplate>().stationLevel;
        
        if (shipScript.scraps.Value >= cost)
        {
            shipScript.scraps.Value = shipScript.scraps.Value - Mathf.Max(cost, 1); // Remove the money
            AddScraps();

            StationTemplate upgrade = GameObject.Find(stationUpgrade).GetComponent<StationTemplate>();
            upgrade.stationLevel += 1;
            if (upgrade.stationLevel == 4)
            {
                upgrade.GetComponent<Button>().interactable = false;
                upgrade.stationLevelText.text = "Level " + upgrade.stationLevel;
                upgrade.currentCost.gameObject.SetActive(false);
            }
            else {
                upgrade.stationLevelText.text = "Level " + upgrade.stationLevel + " → " + (upgrade.stationLevel+1);
                upgrade.currentCost.text = (upgrade.baseCost * upgrade.stationLevel) + " Scraps";  
            }
            var infoList = thrustInfo;
            if (stationUpgrade == "Radar Upgrade")
            {
                infoList = radarInfo;
            }
            else if (stationUpgrade == "Shield Upgrade")
            {
                infoList = shieldInfo;
            }
            upgrade.nextLevelInfo.text = infoList[upgrade.stationLevel-1];
            shipScript.UpgradeStation(stationUpgrade, upgrade.stationLevel);
        }
    }
}