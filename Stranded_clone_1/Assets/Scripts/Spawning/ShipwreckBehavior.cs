using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShipwreckBehavior : NetworkBehaviour
{
    [Tooltip ("How many scraps this shipwreck gives")] public NetworkVariable<float> value;
    [SerializeField] private float despawnDistance;
    public GameObject radarArrow;

    GameObject ship;

    void Start()
    {
        ship = GameObject.Find("Spaceship");
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        //despawn on collision
        if (collider.gameObject.name == "Spaceship" && IsServer)
        { 
            radarArrow.GetComponent<NetworkObject>().Despawn(true);
            GetComponent<NetworkObject>().Despawn(true);
        }
    }

    void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, ship.transform.position) > despawnDistance && IsServer)
        {
            radarArrow.GetComponent<NetworkObject>().Despawn(true);
            GetComponent<NetworkObject>().Despawn(true);
        }
    }

    [Rpc(SendTo.NotServer)]
    public void SetArrowClientRpc(NetworkObjectReference arrow)
    {
        radarArrow = arrow;
    }
}
