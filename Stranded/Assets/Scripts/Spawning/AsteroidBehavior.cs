using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AsteroidBehavior : NetworkBehaviour
{
    public NetworkVariable<float> speed;
    public NetworkVariable<Vector3> direction;
    [SerializeField] private float despawnDistance;
    private GameObject ship;

    void Start()
    {
        ship = GameObject.Find("Spaceship");
    }

    void FixedUpdate()
    {
        if (ship.GetComponent<Spaceship>().controlOfThrusters)
        {
            transform.position += speed.Value * direction.Value * Time.deltaTime;
            if (Vector3.Distance(transform.position, ship.transform.position) > despawnDistance && IsServer)
            {
                GetComponent<NetworkObject>().Despawn(true);
            }
            if (GameObject.FindGameObjectsWithTag("Player").Length > 1)
            {    
                if (IsServer)
                    WriteAsteroidPosClientRpc(transform.position, transform.rotation, GetComponent<Rigidbody2D>().velocity);
                else
                    WriteAsteroidPosServerRpc(transform.position, transform.rotation, GetComponent<Rigidbody2D>().velocity);
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void WriteAsteroidPosServerRpc(Vector3 pos, Quaternion rot, Vector2 vel)
    {
        transform.position = pos;
        transform.rotation = rot;
        GetComponent<Rigidbody2D>().velocity = vel;
    }

    [Rpc(SendTo.NotServer)]
    public void WriteAsteroidPosClientRpc(Vector3 pos, Quaternion rot, Vector2 vel)
    {
        transform.position = pos;
        transform.rotation = rot;
        GetComponent<Rigidbody2D>().velocity = vel;
    }
}
