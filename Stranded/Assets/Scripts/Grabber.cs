using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Grabber : NetworkBehaviour
{
    [HideInInspector] public NetworkVariable<bool> grabberFiring;
    [HideInInspector] public NetworkVariable<Vector3> direction;
    
    public float speed;
    public float retractSpeed;
    public float waitTime;
    public float maxDistance;

    private GameObject ship;

    void Start()
    {
        ship = GameObject.Find("Spaceship");
    }

    void FixedUpdate()
    {
//TODO: accel and decel on the motion
        //Grabber
        if (grabberFiring.Value)
        {
            transform.position += direction.Value*speed; //move arm in direction fired
        }
        else if (!grabberFiring.Value && Vector2.Distance(transform.position, ship.transform.position) > 0.1f)
        {
            Vector2 dist = ship.transform.position - transform.position; //move arm back to ship
            dist = dist.normalized;
            transform.position += new Vector3(dist.x*retractSpeed, dist.y*retractSpeed, 0);
        }
    }
}