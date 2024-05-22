using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Grabber : NetworkBehaviour
{
    [HideInInspector] public NetworkVariable<bool> grabberFiring;
    [HideInInspector] public NetworkVariable<Vector3> direction;
    [HideInInspector] public GameObject grabbedObj;
    
    public float speed;
    private float currentSpeed;
    public float decelAmount;
    public float retractSpeed;
    private float currentRetractSpeed;
    public float retractDecel;
    public float waitTime;
    public float maxDistance;

    private GameObject ship;

    void Start()
    {
        ship = GameObject.Find("Spaceship");
        currentSpeed = speed;
        currentRetractSpeed = retractSpeed;
    }

    void FixedUpdate()
    {
        if (grabberFiring.Value)
        {
            currentRetractSpeed = retractSpeed;
            transform.position += direction.Value*currentSpeed; //move arm in direction fired
            currentSpeed *= decelAmount;
        }
        else if (!grabberFiring.Value && Vector2.Distance(transform.position, ship.transform.position) > 0.1f /*&& LayerMask.LayerToName(grabbedObj.layer) != "Asteroid"*/)
        {
            currentSpeed = speed;
            Vector2 dist = ship.transform.position - transform.position; //move arm back to ship
            dist = dist.normalized;
            transform.position += new Vector3(dist.x*currentRetractSpeed, dist.y*currentRetractSpeed, 0);
            if (currentRetractSpeed < retractSpeed*2)
                currentRetractSpeed *= retractDecel;
            if (grabbedObj != ship && grabbedObj != null)
            {
                grabbedObj.transform.position = transform.position + Vector3.Normalize(transform.position - ship.transform.position)*0.7f;
            }
        }
        else
        {
            currentRetractSpeed = retractSpeed;
        }
        Vector3 retractDir = ship.transform.position - transform.position;
        Vector3 rot = new Vector3(0, 0, Mathf.Atan2(retractDir.y, retractDir.x) * Mathf.Rad2Deg + 90);
        transform.rotation = (Quaternion.Euler(rot));
    }
}