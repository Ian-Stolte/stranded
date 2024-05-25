using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class Sync : NetworkBehaviour
{
    private GameObject ship;
    private GameObject shield;
    private GameObject grabber;
    private GameObject thrusterFire;
    private GameObject radarArrow;
    private CameraFollow camera;
    public PlayerStations player;

    //pause
    public GameObject pauseMenu;
    public GameObject pauseIndicator;
    public NetworkVariable<bool> paused;

    void Start()
    {
        ship = GameObject.Find("Spaceship");
        shield = GameObject.Find("Shield");
        grabber = GameObject.Find("Grabber");
        camera = GameObject.Find("Main Camera").GetComponent<CameraFollow>();
        thrusterFire = ship.transform.GetChild(1).gameObject;
    }

    //PAUSE MENU
    [Rpc(SendTo.Server)]
    public void PauseServerRpc(bool showMenu)
    {
        paused.Value = !paused.Value;
        pauseMenu.SetActive((paused.Value && showMenu));
        pauseIndicator.SetActive(!pauseMenu.activeSelf);
        Time.timeScale = (paused.Value) ? 0 : 1;
        PauseClientRpc(paused.Value, showMenu);
    }

    [Rpc(SendTo.NotServer)]
    public void PauseClientRpc(bool pause, bool showMenu)
    {
        pauseMenu.SetActive((pause && showMenu));
        pauseIndicator.SetActive(!pauseMenu.activeSelf);
        Time.timeScale = (pause) ? 0 : 1;
    }

    //RESOURCE/FUEL SYNC
    [Rpc(SendTo.NotServer)]
    public void ChangeFuelClientRpc(float value, float max)
    {
        GameObject.Find("Fuel Bar").GetComponent<Image>().fillAmount = value/max;
    }

    [Rpc(SendTo.NotServer)]
    public void ChangeHealthClientRpc(float value, float max)
    {
        GameObject.Find("Ship Damage Bar").GetComponent<Image>().fillAmount = value/max;
    }

    //THRUSTER SYNC
    [Rpc(SendTo.Server)]
    public void WriteShipMoveServerRpc(Vector3 newVel, Vector3 newPos, bool thrustersOn, Vector3 addToShield)
    {
        ship.transform.position = newPos;
        ship.GetComponent<Rigidbody2D>().velocity = newVel;
        shield.transform.position += addToShield;
        shield.transform.position = ship.transform.position + Vector3.Normalize(shield.transform.position - ship.transform.position) * 5;
        Vector3 direction = shield.transform.position - ship.transform.position;
        Vector3 rot = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90);
        shield.transform.rotation = (Quaternion.Euler(rot));
        thrusterFire.SetActive(thrustersOn);
        ReadShipMoveClientRpc(newVel, newPos, thrustersOn, shield.transform.position, shield.transform.rotation);
    }

    [Rpc(SendTo.NotServer)]
    public void ReadShipMoveClientRpc(Vector3 newVel, Vector3 newPos, bool thrustersOn, Vector3 shieldPos, Quaternion shieldRot)
    {
        if (player != null)
        {
            if (player.currentStation != "thrusters")
            {
                ship.transform.position = newPos;
                ship.GetComponent<Rigidbody2D>().velocity = newVel;
            }
        }
        shield.transform.position = shieldPos;
        shield.transform.rotation = shieldRot;
        thrusterFire.SetActive(thrustersOn);
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
                //Debug.Log(ship);
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
}