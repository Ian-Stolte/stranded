using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Sync : NetworkBehaviour
{
    public NetworkVariable<Vector3> shipPos = new NetworkVariable<Vector3>();
    public NetworkVariable<Vector3> shipVel = new NetworkVariable<Vector3>();
    public NetworkVariable<Quaternion> shipRot = new NetworkVariable<Quaternion>();
    
    private GameObject ship;
    private GameObject thrusterFire;
    public PlayerStations player;

    void Start()
    {
        ship = GameObject.Find("Spaceship");
        thrusterFire = ship.transform.GetChild(1).gameObject;
    }

    //RESOURCE SYNC
    /*[Rpc(SendTo.Server)]
    public void ChangeFuelServerRpc(float toAdd, float max)
    {
        NetworkVariable<float> fuel = ship.GetComponent<Spaceship>().fuelAmount;
        fuel.Value += toAdd;
        fuel.Value = Mathf.Min(fuel.Value, max);
        fuel.Value = Mathf.Max(fuel.Value, 0);
        ResourceBar barScript = GameObject.Find("Fuel Bar").GetComponent<ResourceBar>();
        barScript.ChangeResourceToAmount(fuel.Value, max);
        ChangeFuelServerRpc(fuel.Value, max);
    }

    [Rpc(SendTo.NotServer)]
    public void ChangeFuelClientRpc(float currentValue, float max)
    {
        ResourceBar barScript = GameObject.Find("Fuel Bar").GetComponent<ResourceBar>();
        barScript.ChangeResourceToAmount(currentValue, max);
    }*/


    //THRUSTER SYNC
    [Rpc(SendTo.Server)]
    public void WriteShipMoveServerRpc(Vector3 newVel, Vector3 newPos, bool fire)
    {
//Any benefit to not updating these if called from the server (b/c that would cause it to update twice)?
        ship.transform.position = newPos;
        ship.GetComponent<Rigidbody2D>().velocity = newVel;
        thrusterFire.SetActive(fire);
        ReadShipMoveClientRpc(newVel, newPos, fire);
    }

    [Rpc(SendTo.NotServer)]
    public void ReadShipMoveClientRpc(Vector3 newVel, Vector3 newPos, bool fire)
    {
        if (player != null)
        {
            if (player.currentStation != "thrusters")
            {
                ship.transform.position = newPos;
                ship.GetComponent<Rigidbody2D>().velocity = newVel;
            }
        }
        thrusterFire.SetActive(fire);
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
