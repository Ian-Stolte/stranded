using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spaceship : MonoBehaviour
{
    public float turnSpeed;
    public float thrustSpeed;
    public float decelSpeed;
    public float maxSpeed;
    public float speedDecrease;
    public int shipHealth;
    public int shipHealthMax;
    public int resourcesCollected;
    public float fuelAmount;
    public float fuelMax;
    [Tooltip("How many seconds between each fuel depletion")] [SerializeField] private float depletionInterval;
    [Tooltip("How much fuel depletes each interval")] [SerializeField] private float depletionAmount;

    [SerializeField] private GameObject coordText;
    [SerializeField] private GameObject speedText;
    [SerializeField] private GameObject resourceText;

    void Start()
    {
        shipHealth = shipHealthMax;
        resourcesCollected = 0;
        fuelAmount = fuelMax;
        StartCoroutine(DepleteOverTime());
    }

    void Update()
    {
        GetComponent<Rigidbody2D>().velocity *= (1 - decelSpeed);
        Vector2 vel = GetComponent<Rigidbody2D>().velocity;

        //reduce to max speed
        float speed = Mathf.Sqrt(Mathf.Pow(vel.x, 2) + Mathf.Pow(vel.y, 2));
        if (speed > maxSpeed)
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
        Debug.Log("Entered collision with " + collision.gameObject.name);
        if (collision.gameObject.name == "Asteroid(Clone)")
        {
            // Updates the health bar
            GameObject damageBar = GameObject.Find("Ship Damage Bar");
            shipHealth--;
            damageBar.GetComponent<ResourceBar>().ChangeResourceToAmount(shipHealth, shipHealthMax);

            // Slows down the ship's maximum speed
            maxSpeed -= speedDecrease;
        }
    }

    //resource enters trigger collider
    void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.gameObject.name == "Resource(Clone)")
        {
            ResourceBar barScript = GameObject.Find("Fuel Bar").GetComponent<ResourceBar>();
            resourcesCollected++;
            fuelAmount += collider.gameObject.GetComponent<ResourceBehavior>().value.Value;
            fuelAmount = Mathf.Min(fuelAmount, fuelMax);
            Debug.Log("Resource pickup: " + fuelAmount);
            barScript.ChangeResourceToAmount(fuelAmount, fuelMax);
        }
    }

    //deplete fuel
    IEnumerator DepleteOverTime()
    {
        while (fuelAmount > 0)
        {   
            yield return new WaitForSeconds(depletionInterval); // Wait
            fuelAmount -= depletionAmount;
            fuelAmount = Mathf.Max(fuelAmount, 0);
            GameObject.Find("Fuel Bar").GetComponent<ResourceBar>().ChangeResourceToAmount(fuelAmount, fuelMax);
            Debug.Log("Depleting... " + fuelAmount);
        }

        // Game over
        if (fuelAmount <= 0)
        {
            Debug.Log("Game Over!");
            GameObject.Find("Fuel Bar").GetComponent<ResourceBar>().GameOver(); //probably only needed if we do a game over UI on the bar
        }
    }
}
