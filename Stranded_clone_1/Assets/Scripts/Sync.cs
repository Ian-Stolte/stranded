using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Sync : NetworkBehaviour
{
//TODO: Change to velocity & fix inability of client to properly accelerate the ship
    public NetworkVariable<Vector3> shipPos = new NetworkVariable<Vector3>();
    //public NetworkVariable<Quaternion> shipRot = new NetworkVariable<Quaternion>();
    
    GameObject ship;

    void Start()
    {
        ship = GameObject.Find("Spaceship");
    }

    [Rpc(SendTo.Server)]
    public void WriteShipPosServerRpc(Vector3 newPos)
    {
        ship.transform.position = newPos;
        ReadShipPosClientRpc(newPos);
    }

    [Rpc(SendTo.NotServer)]
    public void ReadShipPosClientRpc(Vector3 newPos)
    {
        ship.transform.position = newPos;
    }
}
