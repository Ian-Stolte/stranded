using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AsteroidBehavior : NetworkBehaviour
{
    public NetworkVariable<Vector3> position;
    public NetworkVariable<float> speed;
    public NetworkVariable<Vector3> direction;
    [SerializeField] float despawnDistance;

    GameObject ship;

    void Start()
    {
        ship = GameObject.Find("Spaceship");
    }

    void FixedUpdate()
    {
        if (IsServer)
        {
            transform.position += speed.Value * direction.Value * Time.deltaTime;
            position.Value = transform.position;
            AsteroidPosClientRpc();
            if (Vector3.Distance(transform.position, ship.transform.position) > despawnDistance)
            {
                GetComponent<NetworkObject>().Despawn(true);
            }
        }
    }

    [Rpc(SendTo.NotServer)]
    public void AsteroidPosClientRpc()
    {
        transform.position = position.Value;
    }
}
