using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Sync : NetworkBehaviour
{
    public NetworkVariable<Vector3> shipPos = new NetworkVariable<Vector3>();
    public NetworkVariable<Vector3> shipVel = new NetworkVariable<Vector3>();
    //public NetworkVariable<Quaternion> shipRot = new NetworkVariable<Quaternion>();
    
    GameObject ship;
    PlayerStations player;

    void Start()
    {
//TODO: find better way to identify client player
        /*foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (g.GetComponent<PlayerStations>().IsOwner) {
                player = g.GetComponent<PlayerStations>();
                Debug.Log("Found player: " + g.name);
            }
        }*/
        ship = GameObject.Find("Spaceship");
    }

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
        //if (player.currentStation != "thrusters") {
            ship.transform.position = newPos;
            ship.GetComponent<Rigidbody2D>().velocity = newVel;
        //}
    }
}
