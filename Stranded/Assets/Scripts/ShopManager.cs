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
    public string colorStart;
    private string[] steeringInfo; 
    private string[] thrustInfo;
    private string[] shieldInfo;
    private string[] grabberInfo;
    private string[] radarInfo;

    void Start()
    {
        shopPanelsGO[3].SetActive(false);
        steeringInfo = new string[] {"Speed  1 → " + colorStart + "1.5</color>", "Speed  1.5 → " + colorStart + "2</color>", "Speed  2 → " + colorStart + "2.5</color>", "Fully upgraded!" };
        thrustInfo = new string[] {"Acceleration  2 → " + colorStart + "3</color>\nMax Speed  7 → " + colorStart + "9</color>", "Unlocks periodic boosts of speed", "Acceleration  3 → " + colorStart + "4.5</color>\nMax Speed  9 → " + colorStart + "12</color>", "Fully upgraded!"};
        shieldInfo = new string[] {"Speed  1.3 → " + colorStart + "2</color>", "Width  4 → " + colorStart + "6</color>", "Speed  2 → " + colorStart + "3</color>", "Fully upgraded!"};
        grabberInfo = new string[] {"30% chance of double resources", "Range  12 → " + colorStart + "15</color>\nSpeed  0.5 → " + colorStart + "0.55</color>", "Double  30% → " + colorStart + "60%</color>\nRange  15 → " + colorStart + "20</color>\nSpeed  0.55 → " + colorStart + "0.7</color>", "Fully upgraded!"};
        radarInfo = new string[] {"Range  75 → " + colorStart + "125</color>", "Points toward all shipwrecks within range", "Range  125 → " + colorStart + "200</color>", "Fully upgraded!"};
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
        fuelBarRectTransform.anchoredPosition = new Vector2(-730, -350); // Set initial position of resource bars
        healthBarRectTransform.anchoredPosition = new Vector2(-730, -450);

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
        //if (Physics2D.OverlapCircle(GameObject.Find("Spaceship").transform.position, 8, LayerMask.GetMask("Shop")) && !shop.activeSelf)
        //{
            openShopBtn.SetActive(true);
            if (Input.GetKeyDown(KeyCode.E) && !sync.paused.Value && !shop.activeSelf)
            {
                OpenShopServerRpc();
            }
        //}
        else if ((Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape)) && shop.activeSelf)
        {
            CloseShopServerRpc();
        }
        /*else if (!Physics2D.OverlapCircle(GameObject.Find("Spaceship").transform.position, 12, LayerMask.GetMask("Shop")) && shop.activeSelf)
        {
            CloseShopServerRpc();
        }
        else
        {
            openShopBtn.SetActive(false);
        }*/

        //audio fade
        /*float distance = Vector2.Distance(transform.position, GameObject.Find("Spaceship").transform.position);
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
        }*/
    }
    
    //OPEN SHOP
    [Rpc(SendTo.Server)]
    public void OpenShopServerRpc()
    {
        shop.SetActive(true);
        openShopBtn.SetActive(false);
        GameObject.Find("Event System").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
        closeShopBtn.SetActive(true);

        GameObject.Find("Boosts Tab").GetComponent<Image>().color = new Color32(44,44,44,255);
        GameObject.Find("Upgrades Tab").GetComponent<Image>().color = new Color32(72,72,72,255);
        GameObject.Find("Cosmetics Tab").GetComponent<Image>().color = new Color32(72,72,72,255);

        boostsPage.SetActive(true);
        upgradesPage.SetActive(false);
        cosmeticsPage.SetActive(false);

        fuelBar.SetActive(true);
        healthBar.SetActive(true);
        
        fuelBarRectTransform.anchoredPosition = new Vector2(750, -350);
        healthBarRectTransform.anchoredPosition = new Vector2(750, -450);

        AddScraps();
        AddRadioParts();
        
        shopMusic.source.volume = 0.4f;
        strandedMusic.source.volume = 0;
//TODO: Fix fade in not working b/c paused (manually add like 0.01 every frame in Update?)
        //StartCoroutine(audio.StartFade("Shop", 1, 0.4f));
        //StartCoroutine(audio.StartFade("Stranded", 1, 0));
        
        sync.PauseServerRpc(false);
        OpenShopClientRpc();
    }

    [Rpc(SendTo.NotServer)]
    public void OpenShopClientRpc()
    {
        shop.SetActive(true);
        openShopBtn.SetActive(false);
        GameObject.Find("Event System").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
        closeShopBtn.SetActive(true);

        GameObject.Find("Boosts Tab").GetComponent<Image>().color = new Color32(44,44,44,255);
        GameObject.Find("Upgrades Tab").GetComponent<Image>().color = new Color32(72,72,72,255);
        GameObject.Find("Cosmetics Tab").GetComponent<Image>().color = new Color32(72,72,72,255);

        boostsPage.SetActive(true);
        upgradesPage.SetActive(false);
        cosmeticsPage.SetActive(false);

        fuelBar.SetActive(true);
        healthBar.SetActive(true);

        fuelBarRectTransform.anchoredPosition = new Vector2(750, -350);
        healthBarRectTransform.anchoredPosition = new Vector2(750, -450);

        AddScraps();
        AddRadioParts();
        shopMusic.source.volume = 0.4f;
        strandedMusic.source.volume = 0;
        //StartCoroutine(audio.StartFade("Shop", 1, 0.4f));
        //StartCoroutine(audio.StartFade("Stranded", 1, 0));
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
        fuelBarRectTransform.anchoredPosition = new Vector2(-730, -350); // Back to initial position of resource bars
        healthBarRectTransform.anchoredPosition = new Vector2(-730, -450);

        shopMusic.source.volume = 0;
        strandedMusic.source.volume = 0.4f;
        //StartCoroutine(audio.StartFade("Shop", 1, 0f));
        //StartCoroutine(audio.StartFade("Stranded", 1, 0.4f));

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
        fuelBarRectTransform.anchoredPosition = new Vector2(-730, -350); // Back to initial position of resource bars
        healthBarRectTransform.anchoredPosition = new Vector2(-730, -450);

        shopMusic.source.volume = 0;
        strandedMusic.source.volume = 0.4f;
        //StartCoroutine(audio.StartFade("Shop", 1, 0f));
        //StartCoroutine(audio.StartFade("Stranded", 1, 0.4f));
    }

    public void AddScraps()
    {
        scrapsText.text =  "Scraps: " + shipScript.scraps.Value;
        CheckPurchaseable(shipScript.scraps.Value);
    }

    public void AddRadioParts()
    {
        if (shipScript.radioParts.Value > 0){
            radioPartsObject.SetActive(true);
            radioPartsText.text = "Radio Parts: " + shipScript.radioParts.Value;
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

        if (shipScript.radioParts.Value >= boostEffectsSO[3].baseCost){
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

        if (btnNo != 3){
            if (shipScript.scraps.Value >= boostEffectsSO[btnNo].baseCost)
            {
                shipScript.scraps.Value = shipScript.scraps.Value - boostEffectsSO[btnNo].baseCost;
                AddScraps();
                GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Purchase Boost");
                PurchaseBoostClientRpc(btnNo);
            }  
        } else {
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
        cosmeticsPage.SetActive(tabNo == 3);
        if (tabNo == 1)
        {
            GameObject.Find("Boosts Tab").GetComponent<Image>().color = new Color32(44,44,44,255);
            GameObject.Find("Upgrades Tab").GetComponent<Image>().color = new Color32(72,72,72,255);
            GameObject.Find("Cosmetics Tab").GetComponent<Image>().color = new Color32(72,72,72,255);
            fuelBar.SetActive(true);
            healthBar.SetActive(true);
        }
        else if (tabNo == 2)
        {
            GameObject.Find("Upgrades Tab").GetComponent<Image>().color = new Color32(44,44,44,255);
            GameObject.Find("Boosts Tab").GetComponent<Image>().color = new Color32(72,72,72,255);
            GameObject.Find("Cosmetics Tab").GetComponent<Image>().color = new Color32(72,72,72,255);
            fuelBar.SetActive(false);
            healthBar.SetActive(false);
        }
        else if (tabNo == 3)
        {
            GameObject.Find("Cosmetics Tab").GetComponent<Image>().color = new Color32(44,44,44,255);
            GameObject.Find("Upgrades Tab").GetComponent<Image>().color = new Color32(72,72,72,255);
            GameObject.Find("Boosts Tab").GetComponent<Image>().color = new Color32(72,72,72,255);
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