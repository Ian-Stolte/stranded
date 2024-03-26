using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Grabber : NetworkBehaviour
{
    public NetworkVariable<bool> grabberFiring;
    public NetworkVariable<Vector3> direction;
    public float speed;
    public float retractSpeed;
    private GameObject ship;

    void Start()
    {
        ship = GameObject.Find("Spaceship");
    }

    void FixedUpdate()
    {
        //Grabber
        if (Input.GetKey(KeyCode.Space) && grabberFiring.Value)
        {
            Debug.Log("Grabber firing!");
            transform.position += direction.Value*speed; //move arm in direction fired
        }
        else if (!grabberFiring.Value && Vector2.Distance(transform.position, ship.transform.position) > 3)
        {
            Debug.Log("Grabber retracting!");
            Vector2 dist = ship.transform.position - transform.position; //move arm back to ship
            dist = dist.normalized;
            transform.position += new Vector3(dist.x*retractSpeed, dist.y*retractSpeed, 0);
        }
    }
}
