using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class ResourceBehavior : NetworkBehaviour
{
    public NetworkVariable<float> speed;
    public NetworkVariable<Vector3> direction;
    [SerializeField] float despawnDistance;


    GameObject ship;

    void Start()
    {
        ship = GameObject.Find("Spaceship");
    }

    void Update()
    {
        //Debug.Log("Amt collected:" + resourcesCollected);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Spaceship")
        {
            //resourcesCollected = resourcesCollected + 1;
            //Debug.Log("Amt collected:" + resourcesCollected);
            GetComponent<NetworkObject>().Despawn(true);
        }
    }

    void FixedUpdate()
    {
        transform.position += speed.Value * direction.Value * Time.deltaTime;
        if (Vector3.Distance(transform.position, ship.transform.position) > despawnDistance && IsServer)
        {
            //GetComponent<NetworkObject>().Despawn(true);
        }
    }
}
