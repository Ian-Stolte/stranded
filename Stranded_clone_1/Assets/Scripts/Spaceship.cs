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
    [SerializeField] GameObject boostIndicator;

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
    public NetworkVariable<float> fuelAmount;
    public float fuelMax;
    [Tooltip("How many seconds between each fuel depletion")] [SerializeField] private float depletionInterval;
    [Tooltip("How much fuel depletes each interval")] [SerializeField] private float depletionAmount;
    public GameObject resourceTextPrefab;
    private float gameTime;

    // Text variables
    [Header("Text Variables")]
    [SerializeField] private GameObject coordText;
    [SerializeField] private GameObject speedText;
    [SerializeField] private GameObject resourceText;
    [SerializeField] private GameObject scrapText;

    //References
    private Sync sync;
    private ShopManager shop;
    private StatTracker stats;
    [HideInInspector] public PlayerStations player;

    //Radar
    [HideInInspector] public bool radarUnlocked;
    [HideInInspector] public int radarRange;
    [HideInInspector] public bool multipleArrows;

    //Grabber
    [HideInInspector] public bool grabberUnlocked;
    [HideInInspector] public int grabberRange;
    [HideInInspector] public bool multipleRewards;
    [HideInInspector] public bool grabberSpeed;


    //Upgrades
    private float[] turnSpeeds = new float[] {1, 1.5f, 2, 2.5f};
    private float[] thrustSpeeds = new float[] {3, 4, 4, 5};
    private float[] shieldSpeeds = new float[] {1.5f, 2, 2, 3};
    private float[] maxSpeeds = new float[] {8, 10, 10, 12};
    private int[] radarRanges = new int[] {100, 150, 150, 200};
    private int[] grabberRanges = new int[] {13, 16, 16, 20};
    private float[] grabberSpeeds = new float[] {0.5f, 0.55f, 0.55f, 0.7f};
    private float[] grabberRetractSpeeds = new float[] {0.2f, 0.25f, 0.25f, 0.35f};

    //Difficulty levels
    private float[] dmgLevels = new float[] {1, 1.5f, 1.5f, 2};
    private float[] depletionIntervals = new float[] {8, 6.5f, 5, 4};
    private (float min, float max)[] asteroidSpeeds = new (float min, float max)[] {(0.5f, 2), (0.5f, 2.5f), (0.5f, 3), (1.5f, 3.5f)};
    private (float min, float max)[] asteroidDelays = new (float min, float max)[] {(2, 5), (1, 4), (1, 3.5f), (0.5f, 3)};
    private (float min, float max)[] shipwreckDelays = new (float min, float max)[] {(10, 20), (15, 30), (15, 30), (20, 40)};

    void Start()
    {
        shipHealth.Value = shipHealthMax; //shows a warning that we're writing to the var before it exists--should do this on connect instead
        fuelAmount.Value = fuelMax;
        StartCoroutine(DepleteOverTime());
        
        sync = GameObject.Find("Sync Object").GetComponent<Sync>();
        shop = GameObject.Find("Shop Manager").GetComponent<ShopManager>();
        stats = GameObject.Find("Stat Tracker").GetComponent<StatTracker>();
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
    }

    void Update()
    {
        gameTime += Time.deltaTime;

        //Only works for 1 boost charge
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
        resourceText.GetComponent<TMPro.TextMeshProUGUI>().text = "Resources: " + stats.resourcesCollected.Value;
        scrapText.GetComponent<TMPro.TextMeshProUGUI>().text = "Scraps: " + scraps.Value;
    }


    //Hit Asteroid
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Asteroid(Clone)" && !asteroidImmunity.Value)
        {
            GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Asteroid Collision");
            GameObject.Find("Screen Flash").GetComponent<Animator>().Play("ScreenFlash");
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
                GameOver();
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

    IEnumerator HitImmunity()
    {
        asteroidImmunity.Value = true;
        yield return new WaitForSeconds(2);
        asteroidImmunity.Value = false;
    }


    //Collect Resource
    void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.gameObject.name == "Resource(Clone)")
        {
            if (GameObject.Find("Grabber").GetComponent<Grabber>().grabbedObj == collider.gameObject)
            {
                player.hideGrabberInstruction = true;
                player.HideInstructions();
            }
            if (IsServer)
            {
                GameObject resourceText = Instantiate(resourceTextPrefab, transform.position, Quaternion.identity, GameObject.Find("Canvas").transform);
                Rect canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>().rect;
                resourceText.GetComponent<RectTransform>().anchoredPosition = new Vector2(canvasRect.width/2, canvasRect.height/2);
               
                float multiplier = 1; //keeps it normal if multiplied by 1
                if (multipleRewards) //set random chance
                { 
                    float randValue = Random.value;
                    if (randValue > .7f) //30% chance
                    {
                        multiplier = 2; 
                    }
                }

                resourceText.GetComponent<TMPro.TextMeshProUGUI>().text = "+" + (collider.gameObject.GetComponent<ResourceBehavior>().value.Value * multiplier);
                resourceText.transform.SetSiblingIndex(0);
                ResourceTextClientRpc(collider.gameObject.GetComponent<ResourceBehavior>().value.Value * multiplier);
                stats.resourcesCollected.Value++;
                fuelAmount.Value += (collider.gameObject.GetComponent<ResourceBehavior>().value.Value * multiplier);
                fuelAmount.Value = Mathf.Min(fuelAmount.Value, fuelMax);
                GameObject.Find("Fuel Bar").GetComponent<Image>().fillAmount = fuelAmount.Value / fuelMax;
                sync.ChangeFuelClientRpc(fuelAmount.Value, fuelMax);
            }
        }
        if (collider.gameObject.name == "Shipwreck(Clone)")
        {  
            if (GameObject.Find("Grabber").GetComponent<Grabber>().grabbedObj == collider.gameObject)
            {
                player.hideGrabberInstruction = true;
                player.HideInstructions();
            }
            if (player.usedRadar)
            {
                player.hideRadarInstruction = true;
                if (player.currentStation == "radar")
                    player.HideInstructions();
            }
            if (IsServer)
            {
                scraps.Value++;
                stats.scrapsCollected.Value++;
                float multiplier = 1;
                if (multipleRewards) //set random chance
                {
                    float randValue = Random.value;
                    if (randValue > .7f)
                    {
                        multiplier = 2;
                    }
                }

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
            GameOver();
        }
    }

    void GameOver()
    {
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
        {
            g.GetComponent<PlayerStations>().enabled = false;
        }
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
        if (type == "Thruster Upgrade")
        {
            thrustSpeed = thrustSpeeds[level-1];
            maxSpeed = maxSpeeds[level-1];
            if (level >= 3)
            {
                boostUnlocked = true;
                boostIndicator.SetActive(true);
            }
        }
        else if (type == "Radar Upgrade")
        {
            radarUnlocked = true;
            player.buttonCircles.transform.GetChild(4).gameObject.SetActive(true);
            radarRange = radarRanges[level-1];
            if (level >= 3)
            {
                multipleArrows = true;
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
            player.buttonCircles.transform.GetChild(3).gameObject.SetActive(true);
            grabberRange = grabberRanges[level - 1];
            GameObject.Find("Grabber").GetComponent<Grabber>().speed = grabberSpeeds[level-1];
            GameObject.Find("Grabber").GetComponent<Grabber>().retractSpeed = grabberRetractSpeeds[level-1];
            if (level >= 3)
            {
                multipleRewards = true;
            }
        }
        else if (type == "Steering Upgrade")
        {
            turnSpeed = turnSpeeds[level-1];
        }
    }
}