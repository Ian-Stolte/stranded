using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AsteroidBehavior : NetworkBehaviour
{
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
        transform.position += speed.Value * direction.Value * Time.deltaTime;
        if (Vector3.Distance(transform.position, ship.transform.position) > despawnDistance && IsServer)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
    }
}
