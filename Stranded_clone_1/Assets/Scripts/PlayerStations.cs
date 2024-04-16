using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Collections;

public class PlayerStations : NetworkBehaviour
{
    //Stations
    public NetworkVariable<FixedString64Bytes> station = new NetworkVariable<FixedString64Bytes>(writePerm: NetworkVariableWritePermission.Owner);
    public string currentStation;
    private Sync sync;

    //Buttons
    [SerializeField] private GameObject buttonPrefab;
    public GameObject buttons;
    public GameObject buttonCircles;

    //GameObjects
    private GameObject ship;
    private Spaceship shipScript;
    private GameObject shield;
    private GameObject grabber;
    private Grabber grabScript;
    private GameObject grabLine;
    private GameObject shop;

    //Steering
    public InputAction steering;

    //Thrusters
    private bool thrustersOn;
    private Vector3 oldShipPos;

    //Grabber
    [HideInInspector] public bool grabberFired;
    private float grabberWait;
    private bool grabberHasGrabbed;

    //Radar
    public bool radarUnlocked;
    private TMPro.TextMeshProUGUI radarText;
    private GameObject radarArrow;
    private Vector3 radarDir;
    private float cameraZoom;
    private float minDist;

    //Instructions
    public GameObject steerInstruction;
    private bool hideSteerInstruction;
    public GameObject thrusterInstruction;
    private bool hideThrusterInstruction;
    public GameObject shieldInstruction;
    private bool hideShieldInstruction;
    public GameObject grabberInstruction;
    private bool hideGrabberInstruction;

    //Outlines
    private GameObject steeringOutline;
    private GameObject thrusterOutline;
    private GameObject shieldsOutline;
    private GameObject grabberOutline;
    private GameObject radarOutline;

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
        buttons = Instantiate(buttonPrefab, new Vector3(0, 0, 0), transform.rotation, GameObject.Find("Canvas").transform);
        buttons.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
        buttons.name = "Buttons (" + GameObject.FindGameObjectsWithTag("Buttons").Length + ")";
        buttons.GetComponent<Buttons>().target = gameObject;
        buttons.transform.SetSiblingIndex(0);
        buttonCircles = buttons.transform.GetChild(0).gameObject;

        GameObject.Find("Shop Manager").GetComponent<ShopManager>().player = this;

        ship = GameObject.Find("Spaceship");
        shipScript = ship.GetComponent<Spaceship>();
        shield = GameObject.Find("Shield");
        grabber = GameObject.Find("Grabber");
        grabScript = grabber.GetComponent<Grabber>();
        grabLine = GameObject.Find("Grabber Rope");
        radarText = GameObject.Find("Radar Text").GetComponent<TMPro.TextMeshProUGUI>();
        radarArrow = GameObject.Find("Radar Arrow");
        shop = GameObject.Find("Shop Manager").GetComponent<ShopManager>().shop;
        
        sync = GameObject.Find("Sync Object").GetComponent<Sync>();
        if (IsOwner)
            sync.player = this;

        steerInstruction = GameObject.Find("Steering Instructions");
        thrusterInstruction = GameObject.Find("Thruster Instructions");
        shieldInstruction = GameObject.Find("Shield Instructions");
        grabberInstruction = GameObject.Find("Grabber Instructions");

        steeringOutline = GameObject.Find("Steering Outline");
        thrusterOutline = GameObject.Find("Thruster Outline");
        shieldsOutline = GameObject.Find("Shields Outline");
        grabberOutline = GameObject.Find("Grabber Outline");
        radarOutline = GameObject.Find("Radar Outline");

        HideInstructions();
        buttonCircles.SetActive(true);

        if (IsOwner)
            StartCoroutine(Radar());
    }

    void Update()
    {
        if (IsOwner)
        {
            //Show station outlines
            steeringOutline.SetActive((currentStation == "steering"));
            thrusterOutline.SetActive((currentStation == "thrusters"));
            shieldsOutline.SetActive((currentStation == "shields"));
            grabberOutline.SetActive((currentStation == "grabber"));
            radarOutline.SetActive((currentStation == "radar"));

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
            if (currentStation != "none" && (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape)))
            {
                if (currentStation == "grabber")
                {
                    sync.WriteGrabberFiringRpc(false);
                    grabberFired = false;
                }
                currentStation = "none";
                HideInstructions();
            }
            //Buttons
            if (currentStation == "none" && IsOwner && !shop.activeSelf)
            {
                buttonCircles.SetActive(true);
            }
            else
            {
                buttonCircles.SetActive(false);
            }

            //Grabber
            grabLine.GetComponent<LineRenderer>().SetPosition(0, grabber.transform.localPosition);
            grabLine.GetComponent<LineRenderer>().SetPosition(1, ship.transform.localPosition);
            if (currentStation == "grabber" && !shipScript.isStunned)
            {
                if (!hideGrabberInstruction)
                {
                    grabberInstruction.SetActive(true);
                }

                //wait timer (delay between activations)
                if (Vector3.Distance(grabber.transform.position, ship.transform.position) <= 2 && !grabberFired)
                {
                    if (grabberWait == -1)
                    {
                        grabberWait = grabScript.waitTime;
                        grabberHasGrabbed = false;
                    }
                    else
                    {
                        grabberWait -= Time.deltaTime;
                        grabberWait = Mathf.Max(grabberWait, 0);
                    }
                }
                //retract if space pressed
                if (Input.GetKeyDown(KeyCode.Space) && !grabberHasGrabbed && Vector2.Distance(grabber.transform.position, ship.transform.position) > 5 /*&& grabScript.grabberFiring.Value*/)
                {
                    grabberHasGrabbed = true;
                    sync.WriteGrabberFiringRpc(false);
                    grabberFired = false;
                    Bounds b = grabber.GetComponent<BoxCollider2D>().bounds;
                    Collider2D grabCollider = Physics2D.OverlapBox(b.center, b.extents * 2, 0, LayerMask.GetMask("Resource", "Shipwreck"));
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
                //retract if too far
                else if (Vector3.Distance(grabber.transform.position, ship.transform.position) > grabScript.maxDistance)
                {
                    sync.WriteGrabberFiringRpc(false);
                    grabberFired = false;
                }
                //fire if space pressed and delay time has elapsed
                else if (Input.GetKeyDown(KeyCode.Space) && grabberWait <= 0 && grabberWait != -1)
                {
                    hideGrabberInstruction = true;
                    grabberInstruction.SetActive(false);
                    Vector3 mousePos = Input.mousePosition;
                    Rect canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>().rect;
                    Vector3 canvasScale = GameObject.Find("Canvas").GetComponent<RectTransform>().localScale;
                    float mouseXChange = mousePos.x/(canvasRect.width * canvasScale.x) - 0.5f;
                    float mouseYChange = mousePos.y/(canvasRect.height * canvasScale.y) - 0.5f;
                    //take into account camera follow if ship is moving
                    Vector3 cameraOffset = (GameObject.Find("Main Camera").transform.position - grabber.transform.position)/GameObject.Find("Main Camera").GetComponent<Camera>().orthographicSize;
                    Vector3 dir = new Vector3((mouseXChange+cameraOffset.x)*canvasRect.width*canvasScale.x, (mouseYChange+cameraOffset.y)*canvasRect.height*canvasScale.y, 0);

                    grabberWait = -1;
                    Vector3 rot = new Vector3(0, 0, Mathf.Atan2(mouseYChange, mouseXChange) * Mathf.Rad2Deg - 90);
                    sync.WriteGrabberFiringRpc(true);
                    grabberFired = true;
                    sync.WriteGrabberPosServerRpc(dir.normalized, rot);
                    sync.WriteGrabberCloseServerRpc(ship, false); //ship = nothing grabbed
                }
                //release grab if space released
                else if (Input.GetKeyUp(KeyCode.Space))
                {
                    sync.WriteGrabberCloseServerRpc(ship, false); //ship = nothing grabbed
                }
            }
            else if (shipScript.isStunned && grabScript.grabberFiring.Value)
            {
                sync.WriteGrabberFiringRpc(false);
                grabberFired = false;
            }

            //Radar
            float cameraZoom = GameObject.Find("Main Camera").GetComponent<Camera>().orthographicSize;
            if (minDist >= 1.67 * cameraZoom && minDist <= 100 && currentStation == "radar")
            {
                radarArrow.SetActive(true);
                radarArrow.transform.position = ship.transform.position + radarDir * (cameraZoom - 5);
                radarArrow.transform.localScale = new Vector3(0.65f + (cameraZoom - 15) / 100, 1 + (cameraZoom - 15) / 100, 1);
            }
            else
            {
                radarArrow.SetActive(false);
            }

            radarText.enabled = (currentStation == "radar");
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
                shipScript.controlOfThrusters = true;
                sync.WriteShipMoveServerRpc(ship.GetComponent<Rigidbody2D>().velocity, ship.transform.position, thrustersOn, ship.transform.position - oldShipPos);
            }
            else
            {
                shipScript.controlOfThrusters = false;
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

    public void HideInstructions()
    {
        steerInstruction.SetActive(false);
        thrusterInstruction.SetActive(false);
        shieldInstruction.SetActive(false);
        grabberInstruction.SetActive(false);
    }

    private IEnumerator Radar()
//TODO: allow multiple arrows? (instantiate prefabs & destroy instead of setting inactive?)
    {
        while (true)
        {
            yield return new WaitForSeconds(2);
            radarArrow.SetActive(false);
            yield return new WaitForSeconds(0.05f);
            yield return new WaitUntil(() => currentStation == "radar");
            if (radarUnlocked)
            {
                GameObject[] shipwrecks = GameObject.FindGameObjectsWithTag("Shipwreck");
                minDist = 101;
                if (shipwrecks.Length == 0)
                {
                    radarText.text = "N/A";
                    radarArrow.SetActive(false);
                }
                else
                {
                    GameObject closestWreck = shipwrecks[0];
                    foreach (GameObject g in shipwrecks)
                    {
                        float dist = Vector3.Distance(ship.transform.position, g.transform.position)-3;
                        if (dist < minDist)
                        {
                            minDist = dist;
                            closestWreck = g;
                        }
                    }
                    if (minDist == 101)
                        radarText.text = "N/A";
                    else
                        radarText.text = Mathf.Round(minDist) + " km";
                    if (minDist >= 1.67 * cameraZoom && minDist <= 100)
                    {
                        radarArrow.SetActive(true);
                        radarDir = Vector3.Normalize(closestWreck.transform.position - ship.transform.position);
                        Vector3 rot = new Vector3(0, 0, Mathf.Atan2(radarDir.y, radarDir.x) * Mathf.Rad2Deg - 90);
                        radarArrow.transform.rotation = (Quaternion.Euler(rot));
                    }
                    else
                    {
                        radarArrow.SetActive(false);
                    }
                }
            }
        }
    }
}
