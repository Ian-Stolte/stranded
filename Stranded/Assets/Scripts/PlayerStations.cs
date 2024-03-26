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
    private Vector3 oldShipPos;
    
    private GameObject shield;
    private GameObject grabber;

    [SerializeField] private GameObject buttonPrefab;
    public GameObject buttons;
    public GameObject buttonCircles;

    public InputAction steering;

    private Sync sync;
    private GameObject steerInstruction;
    private bool hideSteerInstruction;
    private GameObject thrusterInstruction;
    private bool hideThrusterInstruction;
    private GameObject shieldInstruction;
    private bool hideShieldInstruction;

    private bool grabberFiring;
    private float grabberWait;


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
        shield = GameObject.Find("Shield");
        grabber = GameObject.Find("Grabber");
        
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
                steerInstruction.SetActive(false);
                thrusterInstruction.SetActive(false);
                shieldInstruction.SetActive(false);
            }
            //Buttons
            if (currentStation == "none" && IsOwner) {
                buttonCircles.SetActive(true);
            }
            else {
                buttonCircles.SetActive(false);
            }

            //Grabber
            if (currentStation == "grabber")
            {
                //wait timer (delay between activations)
                if (Vector3.Distance(grabber.transform.position, ship.transform.position) <= 3 && !grabber.GetComponent<Grabber>().grabberFiring.Value)
                {
                    if (grabberWait == -1)
                    {
                        grabberWait = grabber.GetComponent<Grabber>().waitTime;
                    }
                    else
                    {
                        grabberWait -= Time.deltaTime;
                        grabberWait = Mathf.Max(grabberWait, 0);
                    }
                }
                //retract if too far
                if (Vector3.Distance(grabber.transform.position, ship.transform.position) > grabber.GetComponent<Grabber>().maxDistance)
                {
                    grabber.GetComponent<Grabber>().grabberFiring.Value = false;
                }
                //fire if space pressed and delay time has elapsed
                else if (Input.GetKeyDown(KeyCode.Space) && grabberWait <= 0 && grabberWait != -1)
                {
//TODO: shoot toward mouse
                    grabber.GetComponent<Grabber>().grabberFiring.Value = true;
                    grabberWait = -1;
                    Vector3 rot = (ship.transform.eulerAngles + new Vector3(0, 0, 90)) * Mathf.Deg2Rad;
                    Vector3 dir = new Vector3(Mathf.Cos(rot.z), Mathf.Sin(rot.z), 0);
                    grabber.GetComponent<Grabber>().direction.Value = dir.normalized;
                }
                //retract if space released
                else if (Input.GetKeyUp(KeyCode.Space))
                {
                    grabber.GetComponent<Grabber>().grabberFiring.Value = false;
//TODO: if in the collision box of an asteroid, attach to it
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (IsOwner)
        {
            //Steering
            if (currentStation == "steering")
            {
                if (!hideSteerInstruction) {
                    steerInstruction.SetActive(true);
                }
                if (!shipScript.isStunned)
                {
                    ship.transform.Rotate(new Vector3(0, 0, 1), steering.ReadValue<float>()*shipScript.turnSpeed);
                    if (steering.ReadValue<float>() != 0) {
                        hideSteerInstruction = true;
                        steerInstruction.SetActive(false);
                    }
                }
            }
            //write ship rotation
            if (currentStation == "steering" || (IsServer && buttonCircles.transform.GetChild(0).GetComponent<Button>().interactable))
            {
                sync.WriteShipRotServerRpc(ship.transform.rotation);
            }

            //Thrusters
            if (currentStation == "thrusters" && !hideThrusterInstruction)
            {
                thrusterInstruction.SetActive(true);
            }
            if (currentStation == "thrusters" && Input.GetKey(KeyCode.Space) && !shipScript.isStunned)
            {
                hideThrusterInstruction = true;
                thrusterInstruction.SetActive(false);
                thrustersOn = true;
                Vector3 rot = (ship.transform.eulerAngles + new Vector3(0, 0, 90)) * Mathf.Deg2Rad;
                ship.GetComponent<Rigidbody2D>().AddForce(new Vector2(Mathf.Cos(rot.z)*shipScript.thrustSpeed, Mathf.Sin(rot.z)*shipScript.thrustSpeed), ForceMode2D.Force);
            }
            else {
                thrustersOn = false;
            }
            //write ship position & velocity
            if (currentStation == "thrusters" || (IsServer && buttonCircles.transform.GetChild(1).GetComponent<Button>().interactable)) {
                GameObject.Find("Shield").transform.position += ship.transform.position - oldShipPos;
                sync.WriteShipMoveServerRpc(ship.GetComponent<Rigidbody2D>().velocity, ship.transform.position, thrustersOn);
                oldShipPos = ship.transform.position;
            }

            //Shields
            if (currentStation == "shields")
            {
                if (!hideShieldInstruction) {
                    shieldInstruction.SetActive(true);
                }
                if (!shipScript.isStunned)
                {
                    shield.transform.RotateAround(ship.transform.localPosition, new Vector3(0, 0, steering.ReadValue<float>()), shipScript.shieldSpeed);
                    if (steering.ReadValue<float>() != 0) {
                        hideShieldInstruction = true;
                        shieldInstruction.SetActive(false);
                    }
                }
            }
            //write shield rotation
            if (currentStation == "shields" || (IsServer && buttonCircles.transform.GetChild(2).GetComponent<Button>().interactable))
            {
                sync.WriteShieldServerRpc(shield.transform.rotation, shield.transform.position);
            }

//TODO: if grabber station, rotate around ship with mouse
        }
        else {
            //Read station (from owner)
            GetComponent<PlayerStations>().currentStation = "" + station.Value;
        }
    }
}
