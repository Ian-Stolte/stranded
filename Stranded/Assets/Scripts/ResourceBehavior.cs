using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class ResourceBehavior : NetworkBehaviour
{
    [Tooltip ("How much fuel this resource restores")] public NetworkVariable<float> value;
    public NetworkVariable<Vector3> position;
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
        if (IsServer)
        {
            transform.position += speed.Value * direction.Value * Time.deltaTime;
            position.Value = transform.position;
            ResourcePosClientRpc();
            if (Vector3.Distance(transform.position, ship.transform.position) > despawnDistance)
            {
                GetComponent<NetworkObject>().Despawn(true);
            }
        }
    }

    [Rpc(SendTo.NotServer)]
    public void ResourcePosClientRpc()
    {
        transform.position = position.Value;
    }
}
