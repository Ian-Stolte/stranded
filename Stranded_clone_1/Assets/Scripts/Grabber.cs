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
        else if (!grabberFiring.Value && Vector2.Distance(transform.position, ship.transform.position) > 0.1f /*&& LayerMask.LayerToName(grabbedObj.layer) != "Asteroid"*/)
        {
            Vector2 dist = ship.transform.position - transform.position; //move arm back to ship
            dist = dist.normalized;
            transform.position += new Vector3(dist.x*retractSpeed, dist.y*retractSpeed, 0);
            if (grabbedObj != ship && grabbedObj != null)
            {
                if (LayerMask.LayerToName(grabbedObj.layer) == "Resource")
                {
                    grabbedObj.transform.position += new Vector3(dist.x * retractSpeed, dist.y * retractSpeed, 0);
                }
            }
            //Vector3 retractDir = ship.transform.position - transform.position;
            //Vector3 rot = new Vector3(0, 0, Mathf.Atan2(retractDir.y, retractDir.x) * Mathf.Rad2Deg + 90);
            //transform.rotation = (Quaternion.Euler(rot));
        }
        Vector3 retractDir = ship.transform.position - transform.position;
        Vector3 rot = new Vector3(0, 0, Mathf.Atan2(retractDir.y, retractDir.x) * Mathf.Rad2Deg + 90);
        transform.rotation = (Quaternion.Euler(rot));
    }
}