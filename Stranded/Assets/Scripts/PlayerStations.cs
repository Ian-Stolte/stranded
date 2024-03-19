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
    
    private GameObject thrusterFire;
    private bool thrustersOn;
    private GameObject ship;
    private Spaceship shipScript;
    
    private GameObject shield;

    [SerializeField] private GameObject buttonPrefab;
    public GameObject buttons;
    public GameObject buttonCircles;

    public InputAction steering;

    private Sync sync;
    private GameObject steerInstruction;
    private GameObject thrusterInstruction;
    private GameObject shieldInstruction;


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
        buttonCircles = buttons.transform.GetChild(0).gameObject;
        buttonCircles.GetComponent<Buttons>().target = gameObject;

        ship = GameObject.Find("Spaceship");
        shipScript = ship.GetComponent<Spaceship>();
        //thrusterFire = ship.transform.GetChild(1).gameObject;

        //shield set up
        shield = GameObject.Find("Shield");
        
        sync = GameObject.Find("Sync Object").GetComponent<Sync>();
        if (IsOwner)
            sync.player = this;

        steerInstruction = GameObject.Find("Steering Instructions");
        thrusterInstruction = GameObject.Find("Thruster Instructions");
        shieldInstruction = GameObject.Find("Shield Instructions");

        steerInstruction.SetActive(false);
        thrusterInstruction.SetActive(false);
        shieldInstruction.SetActive(false);
        buttonCircles.SetActive(true);
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
            if (currentStation != "none" && Input.GetKeyDown(KeyCode.Q))
            {
                currentStation = "none";
                if (steerInstruction != null)
                    steerInstruction.SetActive(false);
                if (thrusterInstruction != null)
                    thrusterInstruction.SetActive(false);
                if (shieldInstruction != null)
                    shieldInstruction.SetActive(false);
            }
            //Buttons
            if (currentStation == "none" && IsOwner) {
                buttonCircles.SetActive(true);
            }
            else {
                buttonCircles.SetActive(false);
            }

            //Steering
            if (currentStation == "steering")
            {
                if (steerInstruction != null)
                {
                    if (steering.ReadValue<float>() != 0)
                        Destroy(steerInstruction);
                    else
                        steerInstruction.SetActive(true);
                }
                if (!shipScript.isStunned)
                {
                    ship.transform.Rotate(new Vector3(0, 0, 1), steering.ReadValue<float>() * shipScript.turnSpeed);
                }
            }
            //Write ship rotation
            if (currentStation == "steering" || (IsServer && buttonCircles.transform.GetChild(0).GetComponent<Button>().interactable))
            {
                sync.WriteShipRotServerRpc(ship.transform.rotation);
            }

            //Thrusters
            if (currentStation == "thrusters")
            {
                if (thrusterInstruction != null)
                    thrusterInstruction.SetActive(true);
            }
            if (currentStation == "thrusters" && Input.GetKey(KeyCode.Space) && !shipScript.isStunned)
            {
                if (thrusterInstruction != null)
                    Destroy(thrusterInstruction);
                thrustersOn = true;
                Vector3 rot = (ship.transform.eulerAngles + new Vector3(0, 0, 90)) * Mathf.Deg2Rad;
                ship.GetComponent<Rigidbody2D>().AddForce(new Vector2(Mathf.Cos(rot.z)*shipScript.thrustSpeed, Mathf.Sin(rot.z)*shipScript.thrustSpeed), ForceMode2D.Force);
            }
            else {
                thrustersOn = false;
            }
            //Write ship position & velocity
            if (currentStation == "thrusters" || (IsServer && buttonCircles.transform.GetChild(1).GetComponent<Button>().interactable)) {
                sync.WriteShipMoveServerRpc(ship.GetComponent<Rigidbody2D>().velocity, ship.transform.position, thrustersOn);
            }

            //Shields
            if (currentStation == "shields")
            {
                if (shieldInstruction != null)
                {
                    if (steering.ReadValue<float>() != 0)
                        Destroy(shieldInstruction);
                    else
                        shieldInstruction.SetActive(true);                
                }
                shield.transform.RotateAround(ship.transform.localPosition, new Vector3(0, 0, steering.ReadValue<float>()), shipScript.shieldSpeed);
            }
            //Write shield rotation
            if (currentStation == "shields" || (IsServer && buttonCircles.transform.GetChild(2).GetComponent<Button>().interactable))
            {
                sync.WriteShieldServerRpc(shield.transform.rotation, shield.transform.position);
            }
        }
        else {
            //Read station (from owner)
            GetComponent<PlayerStations>().currentStation = "" + station.Value;
        }
    }
}
