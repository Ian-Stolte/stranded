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
    private GameObject shield;
    private GameObject grabber;
    private GameObject thrusterFire;
    private CameraFollow camera;
    public PlayerStations player;

    void Start()
    {
        ship = GameObject.Find("Spaceship");
        shield = GameObject.Find("Shield");
        grabber = GameObject.Find("Grabber");
        camera = GameObject.Find("Main Camera").GetComponent<CameraFollow>();
        thrusterFire = ship.transform.GetChild(1).gameObject;
    }

    //RESOURCE/FUEL SYNC
    [Rpc(SendTo.NotServer)]
    public void ChangeFuelClientRpc(float value, float max)
    {
        GameObject.Find("Fuel Bar").GetComponent<ResourceBar>().ChangeResourceToAmount(value, max);
    }

    [Rpc(SendTo.NotServer)]
    public void ChangeHealthClientRpc(float value, float max)
    {
        GameObject.Find("Ship Damage Bar").GetComponent<ResourceBar>().ChangeResourceToAmount(value, max);
    }

    //THRUSTER SYNC
    [Rpc(SendTo.Server)]
    public void WriteShipMoveServerRpc(Vector3 newVel, Vector3 newPos, bool fire)
    {
        //Any benefit to not updating these if called from the server (b/c that would cause it to update twice)?
        GameObject.Find("Shield").transform.position += newPos - ship.transform.position;
        ship.transform.position = newPos;
        ship.GetComponent<Rigidbody2D>().velocity = newVel;
        thrusterFire.SetActive(fire);
        camera.UpdateCamera();
        ReadShipMoveClientRpc(newVel, newPos, fire);
    }

    [Rpc(SendTo.NotServer)]
    public void ReadShipMoveClientRpc(Vector3 newVel, Vector3 newPos, bool fire)
    {
        if (player != null)
        {
            if (player.currentStation != "thrusters")
            {
                GameObject.Find("Shield").transform.position += newPos - ship.transform.position;
                ship.transform.position = newPos;
                ship.GetComponent<Rigidbody2D>().velocity = newVel;
            }
        }
        thrusterFire.SetActive(fire);
        camera.UpdateCamera();
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

    //SHIELD SYNC
    [Rpc(SendTo.Server)]
    public void WriteShieldServerRpc(Quaternion angle, Vector3 position)
    {
        shield.transform.rotation = angle;
        shield.transform.position = position;
        ReadShieldClientRpc(angle, position);
    }

    [Rpc(SendTo.NotServer)]
    public void ReadShieldClientRpc(Quaternion angle, Vector3 position)
    {
        if (player != null)
        {
            if (player.currentStation != "shields")
            {
                shield.transform.rotation = angle;
                shield.transform.position = position;
            }
        }
    }

    //GRABBER SYNC
    [Rpc(SendTo.Server)]
    public void WriteGrabberFiringRpc(bool firing)
    {
        grabber.GetComponent<Grabber>().grabberFiring.Value = firing;
    }

    [Rpc(SendTo.Server)]
    public void WriteGrabberPosServerRpc(Vector3 direction, Vector3 rotation)
    {
        grabber.GetComponent<Grabber>().direction.Value = direction;
        grabber.transform.rotation = (Quaternion.Euler(rotation));
        //ReadGrabberPosClientRpc(rotation);
    }

    [Rpc(SendTo.Server)]
    public void WriteGrabberCloseServerRpc(NetworkObjectReference obj, bool closed)
    {
        grabber.transform.GetChild(0).gameObject.SetActive(closed);
        grabber.transform.GetChild(1).gameObject.SetActive(!closed);
        grabber.GetComponent<Grabber>().grabbedObj = obj;
        WriteGrabberCloseClientRpc(obj, closed);
    }

    [Rpc(SendTo.NotServer)]
    public void WriteGrabberCloseClientRpc(NetworkObjectReference obj, bool closed)
    {
        grabber.transform.GetChild(0).gameObject.SetActive(closed);
        grabber.transform.GetChild(1).gameObject.SetActive(!closed);
        grabber.GetComponent<Grabber>().grabbedObj = obj;
    }

    /*[Rpc(SendTo.NotServer)]
    public void ReadGrabberPosClientRpc(Vector3 direction, Vector3 rotation)
    {
        if (player != null)
        {
            if (player.currentStation != "steering")
            {
                ship.transform.rotation = newAngle;
            }
        }
    }*/
}