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
    public int shipHealth;
    public int resourcesCollected;

    [SerializeField] GameObject coordText;
    [SerializeField] GameObject speedText;
    [SerializeField] GameObject resourceText;
    [SerializeField] private ShipDamage resourceBarTracker;

    void Start()
    {
        resourcesCollected = 0;
        shipHealth = 10;
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
            GameObject damageBar = GameObject.Find("Ship Damage Bar");
            shipHealth--;
            Debug.Log(shipHealth);
            damageBar.GetComponent<ShipDamage>().ChangeResourceToAmount(shipHealth);
            // resourceBarTracker.ChangeResourceToAmount(shipDamage);
        }
        if (collision.gameObject.name == "Resource(Clone)")
        {
            resourcesCollected++;
            Debug.Log(resourcesCollected);
            resourceBarTracker.ChangeResourceToAmount(resourcesCollected);
        }
    }
}
