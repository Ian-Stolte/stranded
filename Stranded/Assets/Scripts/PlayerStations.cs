using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerStations : MonoBehaviour
{
    /*public enum station
    {
        NONE,
        THRUSTERS,
        STEERING
    }
    public station currentStation;*/
    public string currentStation;
    
    [SerializeField] GameObject thrusterFire;
    GameObject ship;
    Spaceship shipScript;

    //[SerializeField] GameObject buttonPrefab;
    GameObject buttons;

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
        currentStation = "none";
        buttons = GameObject.Find("Spaceship Buttons");
        //buttons = Instantiate(buttonPrefab, new Vector3(0, 0, 0), transform.rotation, GameObject.Find("Canvas").transform);
        //buttons.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
        //buttons.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => SetStation(station.THRUSTERS));
        //buttons.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate {SetStation(station.STEERING); });
        ship = GameObject.Find("Spaceship");
        shipScript = ship.GetComponent<Spaceship>();
    }

    void Update()
    {
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

    public void SetStation(string s)
    {
        currentStation = s;
    }
}
