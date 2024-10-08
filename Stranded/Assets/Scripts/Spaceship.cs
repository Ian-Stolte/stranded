using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class Spaceship : NetworkBehaviour
{
    //To test
    [Header("Test Behaviors")]
    [SerializeField] private bool stun;
    [SerializeField] private bool slowSpeed;
    [SerializeField] private bool destroyAsteroid;
    [SerializeField] private float asteroidDmg;
    [SerializeField] private float knockbackAmount;

    // Speed variables
    [Header("Speed Variables")]
    public float turnSpeed;
    public float shieldSpeed;
    public float thrustSpeed;
    public float decelSpeed;
    public float maxSpeed;
    public float speedDecrease;
    private float maxSpeedRecord;

    //Boost vars
    [Header("Boost Variables")]
    public bool boostUnlocked;
    public float boostSpeed;
    [HideInInspector] public int boostCount;
    [HideInInspector] public bool boosting;
    public float boostDuration;
    public float boostCooldown;
    [HideInInspector] public float boostTimer;
    [SerializeField] private GameObject boostIndicator;
    [SerializeField] public GameObject boostText;

    //Collision vars
    [Header("Collision Variables")]
    public NetworkVariable<bool> asteroidImmunity;
    public float stunDuration; // Duration of the stun in seconds
    [HideInInspector] public bool isStunned; // Indicates if the player is stunned
    public bool controlOfThrusters;

    //Resource variables
    [Header("Resource Variables")]
    public NetworkVariable<float> shipHealth;
    public int shipHealthMax;
    public NetworkVariable<int> scraps;
    public NetworkVariable<float> radioParts;
    public NetworkVariable<float> fuelAmount;
    public float fuelMax;
    [Tooltip("How many seconds between each fuel depletion")][SerializeField] private float depletionInterval;
    [Tooltip("How much fuel depletes each interval")][SerializeField] private float depletionAmount;
    public GameObject resourceTextPrefab;
    private float gameTime;
    private bool firstWreck = true;

    // Text variables
    [Header("Text Variables")]
    [SerializeField] private GameObject coordText;
    [SerializeField] private GameObject speedText;
    [SerializeField] private GameObject scrapText;
    [SerializeField] private GameObject radioPartsText;

    //Danger
    [Header("Danger Variables")]
    private AudioManager audio;
    private float dangerPct;
    [SerializeField] private float danger1Start;
    [SerializeField] private float danger1Max;
    [SerializeField] private float danger2Start;
    [SerializeField] private float danger2Max;
    [SerializeField] private float vignetteStart;
    [SerializeField] private float vignetteMax;

    //References
    private Sync sync;
    private ShopManager shop;
    private StatTracker stats;
    [HideInInspector] public PlayerStations player;

    //Radar
    [HideInInspector] public bool radarUnlocked;
    [HideInInspector] public int radarRange;
    [HideInInspector] public bool multipleArrows;
    public float radioChance;
    
    //Grabber
    [HideInInspector] public bool grabberUnlocked;
    [HideInInspector] public int grabberRange;
    [HideInInspector] public bool grabberSpeed;
    private float multiplierThreshold;

    //Upgrades
    private float[] turnSpeeds = new float[] { 0.8f, 1.1f, 1.6f, 2.5f };
    private float[] thrustSpeeds = new float[] { 2, 3, 3, 4.5f };
    private float[] shieldSpeeds = new float[] { 1.5f, 2, 2, 3 };
    private float[] maxSpeeds = new float[] { 7, 9, 9, 12 };
    private int[] radarRanges = new int[] { 50, 100, 100, 200 };
    private int[] grabberRanges = new int[] { 12, 12, 15, 20 };
    private float[] grabberMultiplierThresholds = new float[] { 1, 0.7f, 0.7f, 0.4f };
    private float[] grabberSpeeds = new float[] { 0.5f, 0.5f, 0.55f, 0.7f };
    private float[] grabberRetractSpeeds = new float[] { 0.2f, 0.2f, 0.25f, 0.35f };

    //Difficulty levels
    private float[] dmgLevels = new float[] { 0.5f, 1, 1.5f, 2 };
    private float[] depletionIntervals = new float[] { 8, 6.5f, 5, 4 };
    private (float min, float max)[] asteroidSpeeds = new (float min, float max)[] { (0.5f, 2), (0.5f, 2.5f), (1, 3), (1.5f, 3.5f) };
    private (float min, float max)[] asteroidDelays = new (float min, float max)[] { (2, 5), (1, 4), (1, 3.5f), (0.5f, 3) };
    private (float min, float max)[] shipwreckDelays = new (float min, float max)[] { (15, 25), (20, 30), (20, 30), (25, 40) };
    private float[] radioChances = new float[] {0.1f, 0.07f, 0.05f, 0.03f};
    private int[] maxShipwrecks = new int[] {8, 8, 6, 6};
    private int[] stormStart = new int[] {180, 180, 150, 120};
    private (int min, int max)[] stormDelays = new (int min, int max)[] {(30, 90), (30, 90), (25, 80), (20, 70)};

    void Start()
    {
        shipHealth.Value = shipHealthMax;
        fuelAmount.Value = fuelMax;
        StartCoroutine(DepleteOverTime());
        
        sync = GameObject.Find("Sync Object").GetComponent<Sync>();
        shop = GameObject.Find("Shop Manager").GetComponent<ShopManager>();
        stats = GameObject.Find("Stat Tracker").GetComponent<StatTracker>();

        audio = GameObject.Find("Audio Manager").GetComponent<AudioManager>();
        //audio.Play("Voice - Fuel Leaking");
    }

    public void SetupDifficulty(int difficulty)
    {
        asteroidDmg = dmgLevels[difficulty];
        depletionInterval = depletionIntervals[difficulty];
        AsteroidSpawner spawner = GameObject.Find("Asteroid Spawner").GetComponent<AsteroidSpawner>();
        spawner.minSpeed = asteroidSpeeds[difficulty].min;
        spawner.maxSpeed = asteroidSpeeds[difficulty].max;
        spawner.minDelay = asteroidDelays[difficulty].min;
        spawner.maxDelay = asteroidDelays[difficulty].max;
        ShipwreckSpawner wreckSpawner = GameObject.Find("Shipwreck Spawner").GetComponent<ShipwreckSpawner>();
        wreckSpawner.minDelay = shipwreckDelays[difficulty].min;
        wreckSpawner.maxDelay = shipwreckDelays[difficulty].max;
        wreckSpawner.maxAtOnce = maxShipwrecks[difficulty];
        radioChance = radioChances[difficulty];
        Storm storm = GameObject.Find("Storm").GetComponent<Storm>();
        storm.timer = stormStart[difficulty];
        storm.minDelay = stormDelays[difficulty].min;
        storm.maxDelay = stormDelays[difficulty].max;
    }

    void Update()
    {
        gameTime += Time.deltaTime;

        boostTimer = Mathf.Max(0, boostTimer - Time.deltaTime);
        if (boostTimer == 0 && boostUnlocked)
        {
            boostCount = 1;
            boostIndicator.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "Boost ready!";
        }
        else
        {
            boostIndicator.transform.GetChild(0).GetComponent<Image>().fillAmount = (boostCooldown-boostTimer)/boostCooldown;
            boostIndicator.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "Charging...";
        }

        if (dangerPct != Mathf.Min(fuelAmount.Value/fuelMax, shipHealth.Value/shipHealthMax)) //if danger changes...
        {
            dangerPct = Mathf.Min(fuelAmount.Value/fuelMax, shipHealth.Value/shipHealthMax);
            StartCoroutine(audio.StartFade("Danger 1", 2, danger1Max - (danger1Max/danger1Start)*dangerPct));
            StartCoroutine(audio.StartFade("Danger 2", 2, danger2Max - (danger2Max/danger2Start)*dangerPct));
        }
        GameObject.Find("Vignette").GetComponent<CanvasGroup>().alpha = vignetteMax - (vignetteMax/vignetteStart)*dangerPct;

        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Shipwreck"))
        {
            if (firstWreck && Mathf.Abs(g.transform.position.x - transform.position.x) < 27*GameObject.Find("Main Camera").GetComponent<Camera>().orthographicSize/15 && Mathf.Abs(g.transform.position.y - transform.position.y) < 15*GameObject.Find("Main Camera").GetComponent<Camera>().orthographicSize/15)
            {
                //audio.Play("Voice - Shipwreck " + UnityEngine.Random.Range(1, 3));
                firstWreck = false;
            }
        }
    }

    void FixedUpdate()
    {
        GetComponent<Rigidbody2D>().velocity *= (1 - decelSpeed);
        Vector2 vel = GetComponent<Rigidbody2D>().velocity;

        //reduce to max speed
        float speed = Mathf.Sqrt(Mathf.Pow(vel.x, 2) + Mathf.Pow(vel.y, 2));
        if (maxSpeed <= 0)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
        }
        else if (speed > maxSpeed)
        {
            float decel = Mathf.Sqrt(Mathf.Pow(maxSpeed, 2) / Mathf.Pow(speed, 2));
            GetComponent<Rigidbody2D>().velocity *= decel;
        }

        //show coordinates and resource/scrap count
        coordText.GetComponent<TMPro.TextMeshProUGUI>().text = "x: " + Mathf.Round(transform.position.x) + "  y: " + Mathf.Round(transform.position.y);
        speedText.GetComponent<TMPro.TextMeshProUGUI>().text = "" + Mathf.Round(speed) + " km/s";
        scrapText.GetComponent<TMPro.TextMeshProUGUI>().text = "Scraps: " + scraps.Value;
        if (radioParts.Value > 0)
            radioPartsText.GetComponent<TMPro.TextMeshProUGUI>().text = "Radio Parts: " + radioParts.Value;
        else
            radioPartsText.GetComponent<TMPro.TextMeshProUGUI>().text = "";
    }


    //Hit Asteroid
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Asteroid") && !asteroidImmunity.Value)
        {
            AteroidHitServerRpc();
            // Updates the health bar
            if (IsServer)
            {
                shipHealth.Value -= asteroidDmg;
                GameObject.Find("Ship Damage Bar").GetComponent<Image>().fillAmount = shipHealth.Value/shipHealthMax;
                sync.ChangeHealthClientRpc(shipHealth.Value, shipHealthMax);
                StartCoroutine(HitImmunity());
                Vector3 dir = collision.gameObject.transform.position - transform.position;
                collision.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector3.Normalize(dir) * knockbackAmount, ForceMode2D.Force);
            }

            if (shipHealth.Value <= 0) {
                GameOverServerRpc("Your ship was too damaged to continue");
            }

            // Slows down the ship's maximum speed
            if (slowSpeed)
                maxSpeed -= speedDecrease;

            // Stuns
            if (stun)
                Stun();

            if (destroyAsteroid && IsServer)
                collision.gameObject.GetComponent<NetworkObject>().Despawn(true);
        }
    }

    [Rpc(SendTo.Server)]
    private void AteroidHitServerRpc()
    {
        audio.Play("Asteroid Collision");
        GameObject.Find("Screen Flash Red").GetComponent<Animator>().Play("ScreenFlash");
        AsteroidHitClientRpc();
    }

    [Rpc(SendTo.NotServer)]
    private void AsteroidHitClientRpc()
    {
        audio.Play("Asteroid Collision");
        GameObject.Find("Screen Flash Red").GetComponent<Animator>().Play("ScreenFlash");
    }

    IEnumerator HitImmunity()
    {
        asteroidImmunity.Value = true;
        yield return new WaitForSeconds(2);
        asteroidImmunity.Value = false;
    }


    //Collect Resource/Shipwreck
    void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.gameObject.name == "Resource(Clone)")
        {
            ResourceCollectServerRpc();
            //if (UnityEngine.Random.value > 0.5f && fuelAmount.Value <= 4)
                //audio.Play("Voice - Resource " + UnityEngine.Random.Range(1, 4));
            int multiplier = 1;
            if (GameObject.Find("Grabber").GetComponent<Grabber>().grabbedObj == collider.gameObject)
            {
                player.hideGrabberInstruction = true;
                player.HideInstructions();
                float randValue = UnityEngine.Random.value;
                if (randValue > multiplierThreshold)
                {
                    multiplier = 2;
                }
            }
            if (IsServer)
            {
                GameObject resourceText = Instantiate(resourceTextPrefab, transform.position, Quaternion.identity, GameObject.Find("Canvas").transform);
                Rect canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>().rect;
                resourceText.GetComponent<RectTransform>().anchoredPosition = new Vector2(canvasRect.width/2, canvasRect.height/2);

                resourceText.GetComponent<TMPro.TextMeshProUGUI>().text = "+" + (collider.gameObject.GetComponent<ResourceBehavior>().value.Value * multiplier);
                resourceText.transform.SetSiblingIndex(0);
                ResourceTextClientRpc(collider.gameObject.GetComponent<ResourceBehavior>().value.Value * multiplier);
                fuelAmount.Value += (collider.gameObject.GetComponent<ResourceBehavior>().value.Value * multiplier);
                fuelAmount.Value = Mathf.Min(fuelAmount.Value, fuelMax);
                GameObject.Find("Fuel Bar").GetComponent<Image>().fillAmount = fuelAmount.Value / fuelMax;
                sync.ChangeFuelClientRpc(fuelAmount.Value, fuelMax);
            }
        }
        if (collider.gameObject.name == "Shipwreck(Clone)")
        {
            ScrapCollectServerRpc();
            int multiplier = 1;
            if (GameObject.Find("Grabber").GetComponent<Grabber>().grabbedObj == collider.gameObject)
            {
                player.hideGrabberInstruction = true;
                player.HideInstructions();
                float randValue = UnityEngine.Random.value;
                if (randValue > multiplierThreshold)
                {
                    multiplier = 2;
                }
            }
            if (player.usedRadar)
            {
                player.hideRadarInstruction = true;
                if (player.currentStation == "radar")
                    player.HideInstructions();
            }
            if (IsServer)
            {
                //radio parts chance
                if (UnityEngine.Random.value < (radioChance * GameObject.Find("Shop Manager").GetComponent<ShopManager>().upgradeMultiplier)) //A radio part is found
                {
                    radioParts.Value += 1;
                    shop.AddRadioParts();
                }
                scraps.Value += collider.GetComponent<ShipwreckBehavior>().value.Value * multiplier;
                GameObject wreckText = Instantiate(resourceTextPrefab, transform.position, Quaternion.identity, GameObject.Find("Canvas").transform);
                Rect canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>().rect;
                wreckText.GetComponent<RectTransform>().anchoredPosition = new Vector2(canvasRect.width/2, canvasRect.height/2);
                wreckText.GetComponent<TMPro.TextMeshProUGUI>().text = "+" + (collider.gameObject.GetComponent<ShipwreckBehavior>().value.Value * multiplier);
                wreckText.transform.SetSiblingIndex(0);
                wreckText.GetComponent<TMPro.TextMeshProUGUI>().color = new Color32(231, 195, 34, 255);
                ResourceTextClientRpc(collider.gameObject.GetComponent<ShipwreckBehavior>().value.Value * multiplier, true);
            }
            shop.AddScraps();
        }
    }

    [Rpc(SendTo.Server)]
    void ResourceCollectServerRpc()
    {
        audio.Play("Resource Collect");
        stats.resourcesCollected++;
        ResourceCollectClientRpc();
    }

    [Rpc(SendTo.NotServer)]
    void ResourceCollectClientRpc()
    {
        audio.Play("Resource Collect");
        stats.resourcesCollected++;
    }

    [Rpc(SendTo.Server)]
    void ScrapCollectServerRpc()
    {
        audio.Play("Scrap Collect");
        stats.scrapsCollected++;
        ScrapCollectClientRpc();
    }

    [Rpc(SendTo.NotServer)]
    void ScrapCollectClientRpc()
    {
        audio.Play("Scrap Collect");
        stats.scrapsCollected++;
    }

    [Rpc(SendTo.NotServer)]
    void ResourceTextClientRpc(float value, bool changeColor = false)
    {
        GameObject resourceText = Instantiate(resourceTextPrefab, transform.position, Quaternion.identity, GameObject.Find("Canvas").transform);
        Rect canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>().rect;
        resourceText.GetComponent<RectTransform>().anchoredPosition = new Vector2(canvasRect.width/2, canvasRect.height/2);
        resourceText.GetComponent<TMPro.TextMeshProUGUI>().text = "+" + value;
        resourceText.transform.SetSiblingIndex(0);
        if (changeColor)
            resourceText.GetComponent<TMPro.TextMeshProUGUI>().color = new Color32(231, 195, 34, 255);
    }

    //deplete fuel
    IEnumerator DepleteOverTime()
    {
        while (fuelAmount.Value > 0)
        {   
            yield return new WaitForSeconds(depletionInterval - Mathf.Min(1, gameTime/600)); // wait
            if (IsServer)
            {
                fuelAmount.Value -= depletionAmount;
                fuelAmount.Value = Mathf.Max(fuelAmount.Value, 0);
                GameObject.Find("Fuel Bar").GetComponent<Image>().fillAmount = fuelAmount.Value/fuelMax;
                sync.ChangeFuelClientRpc(fuelAmount.Value, fuelMax);
            }
        }

        // Game over
        if (fuelAmount.Value <= 0)
        {
            GameOverServerRpc("You ran out of fuel");
        }
    }

    [Rpc(SendTo.Server)]
    public void GameOverServerRpc(string cause)
    {
        GameObject.Find("Fader").GetComponent<Animator>().Play("FadeOut");
        GameOverClientRpc(cause);
        StartCoroutine(GameOverCor(cause));
    }

    [Rpc(SendTo.NotServer)]
    void GameOverClientRpc(string cause)
    {
        GameObject.Find("Fader").GetComponent<Animator>().Play("FadeOut");
        StartCoroutine(GameOverCor(cause));
    }

    IEnumerator GameOverCor(string cause)
    {
        Time.timeScale = 1;
        yield return new WaitForSeconds(0.5f);
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
        {
            g.GetComponent<PlayerStations>().currentStation = "none";
            g.GetComponent<PlayerStations>().enabled = false;  
        }
        stats.causeOfDeath = cause;
        if (IsServer)
            NetworkManager.Singleton.SceneManager.LoadScene("Game Over", LoadSceneMode.Single);
    }

    // Stun
    public void Stun()
    {
        if (!isStunned)
        {
            isStunned = true;
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
            maxSpeedRecord = maxSpeed; // remembers the current speed
            maxSpeed = 0;

            Invoke("EndStun", stunDuration); // ends stun after duration
        }
    }

    private void EndStun()
    {
        isStunned = false;
        maxSpeed = maxSpeedRecord;
    }

    public void UpgradeStation(string type, int level)
    {
        if (type == "Steering Upgrade")
        {
            turnSpeed = turnSpeeds[level-1];
        }
        else if (type == "Thruster Upgrade")
        {
            thrustSpeed = thrustSpeeds[level-1];
            maxSpeed = maxSpeeds[level-1];
            if (level == 3)
            {
                boostUnlocked = true;
                boostIndicator.SetActive(true);
                boostText.SetActive(true);
            }
        }
        else if (type == "Shield Upgrade")
        {
            if (level != 3)
            {
                shieldSpeed = shieldSpeeds[level - 1];
            }
            else
            {
                Vector3 shieldWidth = GameObject.Find("Shield").transform.localScale;
                GameObject.Find("Shield").transform.localScale = new Vector3(6, shieldWidth.y, shieldWidth.z);
            }
        }
        else if (type == "Grabber Upgrade")
        {
            grabberUnlocked = true;
            GameObject.Find("Storm").GetComponent<Storm>().stationsUnlocked.Add(4);
            player.buttonCircles.transform.GetChild(3).gameObject.SetActive(true);
            grabberRange = grabberRanges[level - 1];
            multiplierThreshold = grabberMultiplierThresholds[level - 1];
            GameObject.Find("Grabber").GetComponent<Grabber>().speed = grabberSpeeds[level-1];
            GameObject.Find("Grabber").GetComponent<Grabber>().retractSpeed = grabberRetractSpeeds[level-1];
        }
        else if (type == "Radar Upgrade")
        {
            radarUnlocked = true;
            GameObject.Find("Storm").GetComponent<Storm>().stationsUnlocked.Add(5);
            player.buttonCircles.transform.GetChild(4).gameObject.SetActive(true);
            radarRange = radarRanges[level-1];
            if (level >= 3)
            {
                multipleArrows = true;
            }
        }
    }
}