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
        if (collider.gameObject.name == "Spaceship" && IsServer)
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
        if (GameObject.FindGameObjectsWithTag("Player").Length > 1)
        {
            if (IsServer)
                WritePosClientRpc(transform.position, transform.rotation, GetComponent<Rigidbody2D>().velocity);
            else
                WritePosServerRpc(transform.position, transform.rotation, GetComponent<Rigidbody2D>().velocity);
        }
    }

    [Rpc(SendTo.Server)]
    public void WritePosServerRpc(Vector3 pos, Quaternion rot, Vector2 vel)
    {
        transform.position = pos;
        transform.rotation = rot;
        GetComponent<Rigidbody2D>().velocity = vel;
    }

    [Rpc(SendTo.NotServer)]
    public void WritePosClientRpc(Vector3 pos, Quaternion rot, Vector2 vel)
    {
        transform.position = pos;
        transform.rotation = rot;
        GetComponent<Rigidbody2D>().velocity = vel;
    }
}
