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
    [SerializeField] private bool singlePlayer;
    [SerializeField] private float asteroidDmg;

    // Speed variables
    [Header("Speed Variables")]
    public float turnSpeed;
    public float shieldSpeed;
    public float thrustSpeed;
    public float decelSpeed;
    public float maxSpeed;
    public float speedDecrease;
    private float maxSpeedRecord;

    //Collision vars
    public NetworkVariable<bool> asteroidImmunity;
    public float stunDuration; // Duration of the stun in seconds
    [HideInInspector] public bool isStunned; // Indicates if the player is stunned
    public bool controlOfThrusters;

    // Resource variables
    [Header("Resource Variables")]
    public NetworkVariable<float> shipHealth;
    public int shipHealthMax;
    public NetworkVariable<int> scraps;
    public NetworkVariable<float> fuelAmount;
    public float fuelMax;
    [Tooltip("How many seconds between each fuel depletion")] [SerializeField] private float depletionInterval;
    [Tooltip("How much fuel depletes each interval")] [SerializeField] private float depletionAmount;
    public GameObject resourceTextPrefab;

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

    void Start()
    {
        shipHealth.Value = shipHealthMax; //shows a warning that we're writing to the var before it exists--should do this on connect instead
        fuelAmount.Value = fuelMax;
        StartCoroutine(DepleteOverTime());
        
        sync = GameObject.Find("Sync Object").GetComponent<Sync>();
        shop = GameObject.Find("Shop Manager").GetComponent<ShopManager>();
        stats = GameObject.Find("Stat Tracker").GetComponent<StatTracker>();
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

    //collision
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Asteroid(Clone)" && !asteroidImmunity.Value)
        {
            GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Asteroid Collision");
            // Updates the health bar
            if (IsServer)
            {
                shipHealth.Value -= asteroidDmg;
                GameObject.Find("Ship Damage Bar").GetComponent<ResourceBar>().ChangeResourceToAmount(shipHealth.Value, shipHealthMax);
                sync.ChangeHealthClientRpc(shipHealth.Value, shipHealthMax);
                StartCoroutine(HitImmunity());
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
        yield return new WaitForSeconds(5);
        asteroidImmunity.Value = false;
    }

    // Resource enters trigger collider
    void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.gameObject.name == "Resource(Clone)")
        {
            if (IsServer)
            {
                GameObject resourceText = Instantiate(resourceTextPrefab, transform.position, Quaternion.identity, GameObject.Find("Canvas").transform);
                Rect canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>().rect;
                resourceText.GetComponent<RectTransform>().anchoredPosition = new Vector2(canvasRect.width/2, canvasRect.height/2);
                resourceText.GetComponent<TMPro.TextMeshProUGUI>().text = "+" + collider.gameObject.GetComponent<ResourceBehavior>().value.Value;
                stats.resourcesCollected.Value++;
                fuelAmount.Value += collider.gameObject.GetComponent<ResourceBehavior>().value.Value;
                fuelAmount.Value = Mathf.Min(fuelAmount.Value, fuelMax);
                GameObject.Find("Fuel Bar").GetComponent<ResourceBar>().ChangeResourceToAmount(fuelAmount.Value, fuelMax);
                sync.ChangeFuelClientRpc(fuelAmount.Value, fuelMax);
            }
        }
        if (collider.gameObject.name == "Shipwreck(Clone)")
        {  
            if (IsServer)
            {
                scraps.Value++;
                stats.scrapsCollected.Value++;
            }
            shop.AddScraps();
        }
    }

    //deplete fuel
    IEnumerator DepleteOverTime()
    {
        while (fuelAmount.Value > 0)
        {   
            yield return new WaitForSeconds(depletionInterval); // wait
            if (IsServer && (GameObject.FindGameObjectsWithTag("Player").Length > 1 || singlePlayer))
            {
                fuelAmount.Value -= depletionAmount;
                fuelAmount.Value = Mathf.Max(fuelAmount.Value, 0);
                GameObject.Find("Fuel Bar").GetComponent<ResourceBar>().ChangeResourceToAmount(fuelAmount.Value, fuelMax);
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
}