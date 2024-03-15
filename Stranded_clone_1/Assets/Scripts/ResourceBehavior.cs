using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class ResourceBehavior : NetworkBehaviour
{
    [Tooltip ("How much fuel this resource restores")] public NetworkVariable<float> value;
    public NetworkVariable<float> speed;
    public NetworkVariable<Vector3> direction;
    [SerializeField] private float despawnDistance;

    GameObject ship;

    void Start()
    {
        ship = GameObject.Find("Spaceship");
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        //despawn on collision
        if (collider.gameObject.name == "Spaceship")
        { 
            GetComponent<NetworkObject>().Despawn(true);
        }
    }

    void FixedUpdate()
    {
        transform.position += speed.Value * direction.Value * Time.deltaTime;
        if (Vector3.Distance(transform.position, ship.transform.position) > despawnDistance && IsServer)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
    }
}
