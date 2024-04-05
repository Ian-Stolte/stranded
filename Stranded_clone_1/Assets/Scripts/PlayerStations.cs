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
    
    private bool thrustersOn;
    private GameObject ship;
    private Spaceship shipScript;
    private Vector3 oldShipPos;
    
    private GameObject shield;
    private GameObject grabber;
    private Grabber grabScript;
    private GameObject grabLine;

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

    private bool grabberFired;
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
        shield = GameObject.Find("Shield");
        grabber = GameObject.Find("Grabber");
        grabScript = grabber.GetComponent<Grabber>();
        grabLine = GameObject.Find("Grabber Rope");
        
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
                if (currentStation == "grabber")
                {
                    sync.WriteGrabberFiringRpc(false);
                    grabberFired = false;
                }
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
            grabLine.GetComponent<LineRenderer>().SetPosition(0, grabber.transform.localPosition);
            grabLine.GetComponent<LineRenderer>().SetPosition(1, ship.transform.localPosition);
            if (currentStation == "grabber")
            {
                //wait timer (delay between activations)
                if (Vector3.Distance(grabber.transform.position, ship.transform.position) <= 2 && !grabberFired)
                {
                    if (grabberWait == -1)
                    {
                        grabberWait = grabScript.waitTime;
                    }
                    else
                    {
                        grabberWait -= Time.deltaTime;
                        grabberWait = Mathf.Max(grabberWait, 0);
                    }
                }
                //retract if too far
                if (Vector3.Distance(grabber.transform.position, ship.transform.position) > grabScript.maxDistance)
                {
                    sync.WriteGrabberFiringRpc(false);
                    grabberFired = false;
                }
                //fire if space pressed and delay time has elapsed
                else if (Input.GetKeyDown(KeyCode.Space) && grabberWait <= 0 && grabberWait != -1)
                {
                    Vector3 mousePos = Input.mousePosition;
                    Rect canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>().rect;
                    Vector3 canvasScale = GameObject.Find("Canvas").GetComponent<RectTransform>().localScale;
                    float mouseXChange = mousePos.x - (canvasRect.width * canvasScale.x) * 0.5f;
                    float mouseYChange = mousePos.y - (canvasRect.height * canvasScale.y) * 0.5f;
//TODO: take into account camera follow if ship is moving
    //Debug.Log(GameObject.Find("Main Camera").transform.position - grabber.transform.position);
                    Vector3 dir = new Vector3(mouseXChange, mouseYChange, 0);

                    grabberWait = -1;
                    Vector3 rot = new Vector3(0, 0, Mathf.Atan2(mouseYChange, mouseXChange) * Mathf.Rad2Deg - 90);
                    sync.WriteGrabberFiringRpc(true);
                    grabberFired = true;
                    sync.WriteGrabberPosServerRpc(dir.normalized, rot);
                    sync.WriteGrabberCloseServerRpc(ship, false); //ship = nothing grabbed
                }
                //retract if space pressed
                else if (Input.GetKeyDown(KeyCode.Space) && grabberWait == -1 && Vector2.Distance(grabber.transform.position, ship.transform.position) > 5 && grabScript.grabberFiring.Value)
                {
                    sync.WriteGrabberFiringRpc(false);
                    grabberFired = false;
                    Bounds b = grabber.GetComponent<BoxCollider2D>().bounds;
                    Collider2D grabCollider = Physics2D.OverlapBox(b.center, b.extents * 2, 0, LayerMask.GetMask(/*"Asteroid",*/ "Resource"));
                    GameObject obj = ship;
                    if (grabCollider != null)
                    {
                        obj = grabCollider.gameObject;
                    }
                    sync.WriteGrabberCloseServerRpc(obj, true);
                    /*if (grabScript.grabbedObj != null)
                    {
                        if (LayerMask.LayerToName(grabScript.grabbedObj.layer) == "Asteroid")
                        {
                            grabScript.grabbedObj.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
                            Debug.Log("Asteroid Grabbed!");
                        }
                    }*/
                }
                //release grab if space released
                else if (Input.GetKeyUp(KeyCode.Space))
                {
                    sync.WriteGrabberCloseServerRpc(ship, false); //ship = nothing grabbed
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
            if (currentStation == "thrusters" || (IsServer && buttonCircles.transform.GetChild(1).GetComponent<Button>().interactable))
            {
                sync.WriteShipMoveServerRpc(ship.GetComponent<Rigidbody2D>().velocity, ship.transform.position, thrustersOn, ship.transform.position - oldShipPos);
            }
            oldShipPos = ship.transform.position;

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
        }
        else {
            //Read station (from owner)
            GetComponent<PlayerStations>().currentStation = "" + station.Value;
        }
    }
}
