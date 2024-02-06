using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Sync : NetworkBehaviour
{
//TODO: Fix inability for client to write to vars
    //public NetworkVariable<Vector3> shipPos = new NetworkVariable<Vector3>();
    //public NetworkVariable<Quaternion> shipRot = new NetworkVariable<Quaternion>();
    
    GameObject ship;

    void Start()
    {
        ship = GameObject.Find("Spaceship");
    }

    void Update()
    {
        WriteShipPosServerRpc(ship.transform.position);
    }

    [ServerRpc]
    public void WriteShipPosServerRpc(Vector3 newPos)
    {
        ReadShipPosClientRpc(newPos);
    }

    [ClientRpc]
    public void ReadShipPosClientRpc(Vector3 newPos)
    {
        ship.transform.position = newPos;
    }
}
