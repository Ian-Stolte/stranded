using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Collections;

public class PlayerStations : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> station = new NetworkVariable<FixedString64Bytes>(writePerm: NetworkVariableWritePermission.Owner);
    public string currentStation;
    
    GameObject thrusterFire;
    GameObject ship;
    Spaceship shipScript;

    [SerializeField] GameObject buttonPrefab;
    public GameObject buttons;

    public InputAction steering;

    Sync sync;

    void OnEnable()
    {
        steering.Enable();
    }
    
    void OnDisable()
    {
        steering.Disable();
    }

    void Start()
    {
//TODO: fix naming players (works on host, not on client --- all spawn at same time)
        name = "Player " + GameObject.FindGameObjectsWithTag("Player").Length;

        currentStation = "none";
        buttons = GameObject.Find("Spaceship Buttons");
        buttons = Instantiate(buttonPrefab, new Vector3(0, 0, 0), transform.rotation, GameObject.Find("Canvas").transform);
        buttons.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
        buttons.name = "Buttons (" + GameObject.FindGameObjectsWithTag("Buttons").Length + ")";
        buttons.GetComponent<Buttons>().target = gameObject;

        ship = GameObject.Find("Spaceship");
        shipScript = ship.GetComponent<Spaceship>();
        thrusterFire = ship.transform.GetChild(1).gameObject;

        sync = GameObject.Find("Sync Object").GetComponent<Sync>();
    }

    void Update()
    {
        //Hide incorrect buttons
        if (IsOwner)
        {
            //Write station (as owner)
            station.Value = currentStation;

            GameObject[] allButtons = GameObject.FindGameObjectsWithTag("Buttons");
            foreach (GameObject g in allButtons)
            {
                if (g != buttons)
                {
                    g.SetActive(false);
                }
            }

            //Exit station
            if (currentStation != "none" && Input.GetKeyDown(KeyCode.Escape))
            {
                currentStation = "none";
            }
            //Buttons
            if (currentStation == "none" && IsOwner) {
                buttons.SetActive(true);
            }
            else {
                buttons.SetActive(false);
            }
                
            //Thrusters
            if (currentStation == "thrusters" && Input.GetKey(KeyCode.Space))
            {
                thrusterFire.SetActive(true);
                Vector3 rot = (ship.transform.eulerAngles + new Vector3(0, 0, 90)) * Mathf.Deg2Rad;
                ship.GetComponent<Rigidbody2D>().AddForce(new Vector2(Mathf.Cos(rot.z)*shipScript.thrustSpeed, Mathf.Sin(rot.z)*shipScript.thrustSpeed), ForceMode2D.Force);
            }
            else {
                thrusterFire.SetActive(false);
            }
            //Write ship position & velocity
            if (currentStation == "thrusters" || (IsServer && buttons.transform.GetChild(1).GetComponent<Button>().interactable)) {
                sync.WriteShipMoveServerRpc(ship.GetComponent<Rigidbody2D>().velocity, ship.transform.position);
            }

            //Steering
            if (currentStation == "steering")
            {
                ship.transform.Rotate(new Vector3(0, 0, 1), steering.ReadValue<float>()*shipScript.turnSpeed);
            }
        }
        else {
            //Read station (from owner)
            GetComponent<PlayerStations>().currentStation = "" + station.Value;
        }
    }
}
