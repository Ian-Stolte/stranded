using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerStations : NetworkBehaviour
{
    /*public enum station
    {
        NONE,
        THRUSTERS,
        STEERING
    }
    public station currentStation;*/
    public string currentStation;
    
    GameObject thrusterFire;
    GameObject ship;
    Spaceship shipScript;

    [SerializeField] GameObject buttonPrefab;
    public GameObject buttons;

    public InputAction steering;

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
//TODO: fix naming players
        if (IsOwner)
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
    }

    void Update()
    {
        //Hide incorrect buttons
        if (IsOwner)
        {
            GameObject[] allButtons = GameObject.FindGameObjectsWithTag("Buttons");
            foreach (GameObject g in allButtons)
            {
                if (g != buttons)
                {
//TODO: fix not disabling Buttons (2) correctly
                    Debug.Log("Disabling!");
                    g.SetActive(false);
                }
            }
        }

        //Exit station
        if (currentStation != "none" && Input.GetKeyDown(KeyCode.Escape))
        {
            currentStation = "none";
        }
        //Buttons
        if (currentStation == "none") {
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
        //Steering
        if (currentStation == "steering")
        {
            ship.transform.Rotate(new Vector3(0, 0, 1), steering.ReadValue<float>()*shipScript.turnSpeed);
        }
    }
}
