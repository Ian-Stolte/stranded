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
    public GameObject radioPartsObject;
    [HideInInspector] public PlayerStations player;
    private Sync sync;

    public TMP_Text scrapsText;
    public TMP_Text radioPartsText;
    public BoostEffect[] boostEffectsSO;
    [SerializeField] GameObject[] shopPanelsGO;
    public ShopTemplate[] shopPanels;
    public Button[] myPurchaseBtns;

    public GameObject openShopBtn;
    public GameObject closeShopBtn;

    private AudioManager audio;
    private bool startMusic;
    private Sound shopMusic;
    private Sound strandedMusic;

    [SerializeField] private GameObject exclamationPoint1;
    [SerializeField] private GameObject exclamationPoint2;

    private RectTransform fuelBarRectTransform;
    private RectTransform healthBarRectTransform;

    [Header("Shop Tabs")]
    public GameObject boostsPage;
    public GameObject upgradesPage;

    [Header("Upgrade Info")]
    [SerializeField] GameObject[] upgradePanels;
    [SerializeField] GameObject boostIndicator;

    //Level Info
    public string colorStart;
    private string[] steeringInfo; 
    private string[] thrustInfo;
    private string[] shieldInfo;
    private string[] grabberInfo;
    private string[] radarInfo;

    //Upgrade level for radio part collection
    public float upgradeMultiplier;

    void Start()
    {
        shopPanelsGO[3].SetActive(false);
        steeringInfo = new string[] {"Speed  0.8 → " + colorStart + "1.1</color>", "Speed  1.1 → " + colorStart + "1.6</color>", "Speed  1.6 → " + colorStart + "2.5</color>", "Fully upgraded!" };
        thrustInfo = new string[] {"Acceleration  2 → " + colorStart + "3</color>\nMax Speed  7 → " + colorStart + "9</color>", "Unlocks periodic boosts of speed", "Acceleration  3 → " + colorStart + "4.5</color>\nMax Speed  9 → " + colorStart + "12</color>", "Fully upgraded!"};
        shieldInfo = new string[] {"Speed  1.3 → " + colorStart + "2</color>", "Width  4 → " + colorStart + "6</color>", "Speed  2 → " + colorStart + "3</color>", "Fully upgraded!"};
        grabberInfo = new string[] {"30% chance of double resources", "Range  12 → " + colorStart + "15</color>\nSpeed  0.5 → " + colorStart + "0.55</color>", "Double  30% → " + colorStart + "60%</color>\nRange  15 → " + colorStart + "20</color>\nSpeed  0.55 → " + colorStart + "0.7</color>", "Fully upgraded!"};
        radarInfo = new string[] {"Range  50 → " + colorStart + "100</color>", "Points toward all shipwrecks within range", "Range  100 → " + colorStart + "200</color>", "Fully upgraded!"};
        upgradeMultiplier = 1;
        //Initialize text values
        foreach (GameObject g in upgradePanels)
        {
            string[] infoList = new string[0];
            if (g.name == "Steering Upgrade")
            {
                g.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "Lv. 1 → " + colorStart + "2</color>";
                g.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = steeringInfo[0];
            }
            else if (g.name == "Thruster Upgrade")
            {
                g.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "Lv. 1 → " + colorStart + "2</color>";
                g.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = thrustInfo[0];
            }
            else if (g.name == "Shield Upgrade")
            {
                g.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "Lv. 1 → " + colorStart + "2</color>";
                g.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = shieldInfo[0];
            }
            else if(g.name == "Grabber Upgrade")
            {
                g.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = colorStart + "Unlock</color>";
                g.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = "Unlocks a grabber arm to collect resources";
            }
            else if (g.name == "Radar Upgrade")
            {
                g.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = colorStart + "Unlock</color>";
                g.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = "Unlock a radar to find the nearest shipwreck";
            }
        }

        fuelBarRectTransform = fuelBar.GetComponent<RectTransform>();
        healthBarRectTransform = healthBar.GetComponent<RectTransform>();
        fuelBarRectTransform.anchoredPosition = new Vector2(-720, -350); // Set initial position of resource bars
        healthBarRectTransform.anchoredPosition = new Vector2(-720, -455);

        audio = GameObject.Find("Audio Manager").GetComponent<AudioManager>();
        shopMusic = Array.Find(audio.music, sound => sound.name == "Shop");
        strandedMusic = Array.Find(audio.music, sound => sound.name == "Stranded");

        openShopBtn.SetActive(true);
        closeShopBtn.SetActive(false);

        ship = GameObject.Find("Spaceship");
        shipScript = ship.GetComponent<Spaceship>();
        sync = GameObject.Find("Sync Object").GetComponent<Sync>();

        AddScraps();  
        LoadPanels();
        radioPartsObject.SetActive(false);
        
    }

    void Update()
    {
        openShopBtn.SetActive(true);
        if (Input.GetKeyDown(KeyCode.E) && !sync.paused.Value && !shop.activeSelf)
            OpenShopServerRpc();
        else if ((Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape)) && shop.activeSelf)
            CloseShopServerRpc();
    }
    
    //OPEN SHOP
    [Rpc(SendTo.Server)]
    public void OpenShopServerRpc()
    {
        shop.SetActive(true);
        openShopBtn.SetActive(false);
        exclamationPoint1.SetActive(false);
        GameObject.Find("Event System").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
        closeShopBtn.SetActive(true);

        GameObject.Find("Boosts Tab").GetComponent<Image>().color = new Color32(72,72,72,255);
        GameObject.Find("Upgrades Tab").GetComponent<Image>().color = new Color32(44,44,44,255);

        boostsPage.SetActive(false);
        upgradesPage.SetActive(true);

        fuelBar.SetActive(false);
        healthBar.SetActive(false);
        
        fuelBarRectTransform.anchoredPosition = new Vector2(750, -350);
        healthBarRectTransform.anchoredPosition = new Vector2(750, -455);

        AddScraps();
        AddRadioParts();
        
        GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Button Press 2");
        
        sync.PauseServerRpc(false);
        OpenShopClientRpc();
    }

    [Rpc(SendTo.NotServer)]
    public void OpenShopClientRpc()
    {
        shop.SetActive(true);
        openShopBtn.SetActive(false);
        exclamationPoint1.SetActive(false);
        GameObject.Find("Event System").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
        closeShopBtn.SetActive(true);

        GameObject.Find("Boosts Tab").GetComponent<Image>().color = new Color32(72,72,72,255);
        GameObject.Find("Upgrades Tab").GetComponent<Image>().color = new Color32(44,44,44,255);

        boostsPage.SetActive(false);
        upgradesPage.SetActive(true);

        fuelBar.SetActive(false);
        healthBar.SetActive(false);

        fuelBarRectTransform.anchoredPosition = new Vector2(750, -350);
        healthBarRectTransform.anchoredPosition = new Vector2(750, -455);

        AddScraps();
        AddRadioParts();

        GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Button Press 2");
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
        fuelBarRectTransform.anchoredPosition = new Vector2(-720, -350); // Back to initial position of resource bars
        healthBarRectTransform.anchoredPosition = new Vector2(-720, -455);

        GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Button Press 1");

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
        fuelBarRectTransform.anchoredPosition = new Vector2(-720, -350); // Back to initial position of resource bars
        healthBarRectTransform.anchoredPosition = new Vector2(-720, -455);

        GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Button Press 1");
    }

    public void AddScraps()
    {
        scrapsText.text =  "Scraps: " + shipScript.scraps.Value;
        CheckPurchaseable(shipScript.scraps.Value);
    }

    public void AddRadioParts()
    {
        if (shipScript.radioParts.Value > 0) {
            radioPartsObject.SetActive(true);
            radioPartsText.text = "Radio Parts: " + shipScript.radioParts.Value;
            if (shipScript.radioParts.Value == 5)
            {
                exclamationPoint1.SetActive(true);
                exclamationPoint2.SetActive(true);
            }
        } else {
            radioPartsObject.SetActive(false);
        }
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

    public void CheckPurchaseable(int scraps)
    {
        foreach (GameObject g in upgradePanels) // Checks if upgrades are purchaseable
        {
            //int cost = 2 * g.GetComponent<StationTemplate>().baseCost * g.GetComponent<StationTemplate>().stationLevel;
            StationTemplate upgrade = g.GetComponent<StationTemplate>();
            int cost = 3;
            if (upgrade.stationLevel == 1)
                cost = 2;
            if (upgrade.stationLevel == 3)
                cost = 5;
            if (scraps >= cost && g.GetComponent<StationTemplate>().stationLevel != 4) // If player has enough money
            {
                g.GetComponent<Button>().interactable = true;
                g.GetComponent<StationTemplate>().currentCost.GetComponent<CanvasGroup>().alpha = 1;
            } 
            else 
            {
                g.GetComponent<Button>().interactable = false;
                g.GetComponent<StationTemplate>().currentCost.GetComponent<CanvasGroup>().alpha = 0.3f;
            }
        }
        for (int i = 0; i < boostEffectsSO.Length; i++)
        {
            if (scraps >= boostEffectsSO[i].baseCost)
            {
                shopPanelsGO[i].GetComponent<Button>().interactable = true;
                shopPanelsGO[i].transform.GetChild(2).GetComponent<CanvasGroup>().alpha = 1;
            }
            else
            {
                shopPanelsGO[i].GetComponent<Button>().interactable = false;
                shopPanelsGO[i].transform.GetChild(2).GetComponent<CanvasGroup>().alpha = 0.3f;
            }
        }

        if (shipScript.radioParts.Value >= 5) {
            shopPanelsGO[3].SetActive(true);
            shopPanelsGO[3].GetComponent<Button>().interactable = true;
            shopPanelsGO[3].transform.GetChild(2).GetComponent<CanvasGroup>().alpha = 1;
        } else {
            shopPanelsGO[3].SetActive(false);
        }

    }

    [Rpc(SendTo.Server)]
    public void PurchaseBoostServerRpc(int btnNo)
    {
        if (btnNo != 3)
        {
            if (shipScript.scraps.Value >= boostEffectsSO[btnNo].baseCost)
            {
                shipScript.scraps.Value = shipScript.scraps.Value - boostEffectsSO[btnNo].baseCost;
                AddScraps();
                GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Purchase Boost");
                PurchaseBoostClientRpc(btnNo);
            }  
        }
        else 
        {
            Debug.Log("Radio Parts: " + shipScript.radioParts.Value.ToString());
            if (shipScript.radioParts.Value >= boostEffectsSO[btnNo].baseCost)
            {
                shipScript.radioParts.Value = shipScript.radioParts.Value - boostEffectsSO[btnNo].baseCost;
                GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Purchase Boost");
            }
        }
         
    }

    [Rpc(SendTo.NotServer)]
    public void PurchaseBoostClientRpc(int btnNo)
    {
        AddScraps();
        scrapsText.text = "Scraps: " + (shipScript.scraps.Value - boostEffectsSO[btnNo].baseCost);
        CheckPurchaseable(shipScript.scraps.Value - boostEffectsSO[btnNo].baseCost);
        GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Purchase Boost");
    }

    //Change Tab
    void ChangeTab(int tabNo)
    {
        CheckPurchaseable(shipScript.scraps.Value);
        boostsPage.SetActive(tabNo == 1);
        upgradesPage.SetActive(tabNo == 2);
        if (tabNo == 1)
        {
            GameObject.Find("Boosts Tab").GetComponent<Image>().color = new Color32(44,44,44,255);
            GameObject.Find("Upgrades Tab").GetComponent<Image>().color = new Color32(72,72,72,255);
            // GameObject.Find("Cosmetics Tab").GetComponent<Image>().color = new Color32(72,72,72,255);
            fuelBar.SetActive(true);
            healthBar.SetActive(true);
            exclamationPoint2.SetActive(false);
        }
        else if (tabNo == 2)
        {
            GameObject.Find("Upgrades Tab").GetComponent<Image>().color = new Color32(44,44,44,255);
            GameObject.Find("Boosts Tab").GetComponent<Image>().color = new Color32(72,72,72,255);
            // GameObject.Find("Cosmetics Tab").GetComponent<Image>().color = new Color32(72,72,72,255);
            fuelBar.SetActive(false);
            healthBar.SetActive(false);
        }
    }

    [Rpc(SendTo.Server)]
    public void ChangeTabServerRpc(int tabNo)
    {
        ChangeTab(tabNo);
        ChangeTabClientRpc(tabNo);
    }

    [Rpc(SendTo.NotServer)]
    public void ChangeTabClientRpc(int tabNo)
    {
        ChangeTab(tabNo);
    }


    [Rpc(SendTo.Server)]
    public void StationUpgradeServerRpc(string stationUpgrade)
    {
        StationTemplate upgrade = GameObject.Find(stationUpgrade).GetComponent<StationTemplate>();
        int cost = 3;
        if (upgrade.stationLevel == 1)
            cost = 2;
        if (upgrade.stationLevel == 3)
            cost = 5;

        if (shipScript.scraps.Value >= cost)
        {
            shipScript.scraps.Value = shipScript.scraps.Value - cost;
            AddScraps();
            upgradeMultiplier += 0.5f; //increase upgrade count for radio part multiplier and apply factor of 0.5 to make it appropriate scaling factor for radio part chance
            upgrade.stationLevel += 1;
            int newCost = 3;
            if (upgrade.stationLevel == 1)
                newCost = 2;
            if (upgrade.stationLevel == 3)
                newCost = 5;
            GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Upgrade");
            if (upgrade.stationLevel == 4)
            {
                upgrade.GetComponent<Button>().interactable = false;
                upgrade.stationLevelText.text = "Lv. " + upgrade.stationLevel;
                upgrade.currentCost.gameObject.SetActive(false);
            }
            else {
                upgrade.stationLevelText.text = "Lv. " + upgrade.stationLevel + " → " + colorStart + (upgrade.stationLevel+1) + "</color>";
                
                upgrade.currentCost.text = newCost + " Scraps";
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
            else if(stationUpgrade == "Grabber Upgrade")
            {
                infoList = grabberInfo;
            }
            else if (stationUpgrade == "Steering Upgrade")
            {
                infoList = steeringInfo;
            }
            upgrade.nextLevelInfo.text = infoList[upgrade.stationLevel-1];
            shipScript.UpgradeStation(stationUpgrade, upgrade.stationLevel);
            StationUpgradeClientRpc(stationUpgrade, cost, newCost);
        }
    }

    [Rpc(SendTo.NotServer)]
    public void StationUpgradeClientRpc(string stationUpgrade, int cost, int newCost)
    {
        StationTemplate upgrade = GameObject.Find(stationUpgrade).GetComponent<StationTemplate>();
        scrapsText.text = "Scraps: " + (shipScript.scraps.Value - cost);
        CheckPurchaseable(shipScript.scraps.Value - cost);

        upgrade.stationLevel += 1;

        GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Upgrade");
        if (upgrade.stationLevel == 4)
        {
            upgrade.GetComponent<Button>().interactable = false;
            upgrade.stationLevelText.text = "Lv. " + upgrade.stationLevel;
            upgrade.currentCost.gameObject.SetActive(false);
        }
        else
        {
            upgrade.stationLevelText.text = "Lv. " + upgrade.stationLevel + " → " + colorStart + (upgrade.stationLevel + 1) + "</color>";
            upgrade.currentCost.text = newCost + " Scraps";
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
        else if (stationUpgrade == "Grabber Upgrade")
        {
            infoList = grabberInfo;
        }
        else if (stationUpgrade == "Steering Upgrade")
        {
            infoList = steeringInfo;
        }
        upgrade.nextLevelInfo.text = infoList[upgrade.stationLevel - 1];
        shipScript.UpgradeStation(stationUpgrade, upgrade.stationLevel);
    }
}