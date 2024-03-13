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

    // Speed variables
    [Header("Speed Variables")]
    public float turnSpeed;
    public float thrustSpeed;
    public float decelSpeed;
    public float maxSpeed;
    public float speedDecrease;
    public float stunDuration; // Duration of the stun in seconds
    public bool isStunned; // Indicates if the player is stunned
    float maxSpeedRecord;

    // Resource variables
    [Header("Resource Variables")]
    public NetworkVariable<int> shipHealth;
    public int shipHealthMax;
    public int resourcesCollected;
    public NetworkVariable<float> fuelAmount;
    public float fuelMax;
    [Tooltip("How many seconds between each fuel depletion")] [SerializeField] private float depletionInterval;
    [Tooltip("How much fuel depletes each interval")] [SerializeField] private float depletionAmount;

    // Text variables
    [Header("Text Variables")]
    [SerializeField] private GameObject coordText;
    [SerializeField] private GameObject speedText;
    [SerializeField] private GameObject resourceText;

    //References
    private Sync sync;

    void Start()
    {
        shipHealth.Value = shipHealthMax;
        resourcesCollected = 0;
        fuelAmount.Value = fuelMax;
        StartCoroutine(DepleteOverTime());
        sync = GameObject.Find("Sync Object").GetComponent<Sync>();
    }

    void Update()
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

        //show coordinates
        coordText.GetComponent<TMPro.TextMeshProUGUI>().text = "x: " + Mathf.Round(transform.position.x) + "  y: " + Mathf.Round(transform.position.y);
        speedText.GetComponent<TMPro.TextMeshProUGUI>().text = "" + Mathf.Round(speed) + " km/s";
        resourceText.GetComponent<TMPro.TextMeshProUGUI>().text = "Resources Collected: " + resourcesCollected;
    }

    //collision
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Asteroid(Clone)")
        {
            // Updates the health bar
            GameObject damageBar = GameObject.Find("Ship Damage Bar");
            shipHealth.Value--;
            damageBar.GetComponent<ResourceBar>().ChangeResourceToAmount(shipHealth.Value, shipHealthMax);

            if (shipHealth.Value <= 0){
                Debug.Log("Game Over! Your ship broke down...");
                GameObject.Find("Ship Damage Bar").GetComponent<ResourceBar>().GameOver();
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

    // Resource enters trigger collider
    void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.gameObject.name == "Resource(Clone)")
        {
            resourcesCollected++;
            //sync.ChangeFuelServerRpc(collider.gameObject.GetComponent<ResourceBehavior>().value.Value, fuelMax);
            if (IsServer)
                fuelAmount.Value += collider.gameObject.GetComponent<ResourceBehavior>().value.Value;
                fuelAmount.Value = Mathf.Min(fuelAmount.Value, fuelMax);
            GameObject.Find("Fuel Bar").GetComponent<ResourceBar>().ChangeResourceToAmount(fuel.Value, max);
        }
    }

    //deplete fuel
    IEnumerator DepleteOverTime()
    {
        while (fuelAmount.Value > 0)
        {   
            yield return new WaitForSeconds(depletionInterval); // Wait
            //sync.ChangeFuelServerRpc(-depletionAmount, fuelMax);
            if (IsServer)
                fuelAmount.Value -= depletionAmount;
                fuelAmount.Value = Mathf.Max(fuelAmount.Value, 0);
            GameObject.Find("Fuel Bar").GetComponent<ResourceBar>().ChangeResourceToAmount(fuelAmount.Value, fuelMax);
        }

        // Game over
        if (fuelAmount.Value <= 0)
        {
            GameObject.Find("Fuel Bar").GetComponent<ResourceBar>().GameOver(); //probably only needed if we do a game over UI on the bar
            GameOver();
        }
    }

    void GameOver()
    {
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
        {
            Destroy(g);
        }
        NetworkManager.Singleton.SceneManager.LoadScene("Game Over", LoadSceneMode.Single);
    }

    // Stun
    public void Stun()
    {
        if (!isStunned)
        {
            isStunned = true;
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
            maxSpeedRecord = maxSpeed; // Remembers the current speed
            maxSpeed = 0;
            // GetComponent<Rigidbody2D>().velocity = Vector2.zero;

            Invoke("EndStun", stunDuration); // Ends stun after duration
        }
    }

    private void EndStun()
    {
        isStunned = false;
        maxSpeed = maxSpeedRecord;
    }
}
