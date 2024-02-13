using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Sync : NetworkBehaviour
{
    public NetworkVariable<Vector3> shipPos = new NetworkVariable<Vector3>();
    public NetworkVariable<Vector3> shipVel = new NetworkVariable<Vector3>();
    public NetworkVariable<Quaternion> shipRot = new NetworkVariable<Quaternion>();
    
    GameObject ship;
    public PlayerStations player;

    void Start()
    {
        ship = GameObject.Find("Spaceship");
    }

    //THRUSTER SYNC
    [Rpc(SendTo.Server)]
    public void WriteShipMoveServerRpc(Vector3 newVel, Vector3 newPos)
    {
        ship.transform.position = newPos;
        ship.GetComponent<Rigidbody2D>().velocity = newVel;
        ReadShipMoveClientRpc(newVel, newPos);
    }

    [Rpc(SendTo.NotServer)]
    public void ReadShipMoveClientRpc(Vector3 newVel, Vector3 newPos)
    {
        if (player != null)
        {
            if (player.currentStation != "thrusters")
            {
                ship.transform.position = newPos;
                ship.GetComponent<Rigidbody2D>().velocity = newVel;
            }
        }
    }

    //STEERING SYNC
    [Rpc(SendTo.Server)]
    public void WriteShipRotServerRpc(Quaternion newAngle)
    {
        ship.transform.rotation = newAngle;
        ReadShipRotClientRpc(newAngle);
    }

    [Rpc(SendTo.NotServer)]
    public void ReadShipRotClientRpc(Quaternion newAngle)
    {
        if (player != null)
        {
            if (player.currentStation != "steering")
            {
                ship.transform.rotation = newAngle;
            }
        }
    }
}
