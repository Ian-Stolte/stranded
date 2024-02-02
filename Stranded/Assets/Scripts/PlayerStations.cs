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

    //[SerializeField] GameObject buttonPrefab;
    GameObject buttons;

    [SerializeField] float rotateSpeed;
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
            //add force
        }
        else {
            thrusterFire.SetActive(false);
        }
        //Steering
        if (currentStation == "steering")
        {
            ship.transform.Rotate(new Vector3(0, 0, 1), steering.ReadValue<float>()*rotateSpeed);
        }
    }

    public void SetStation(string s)
    {
        currentStation = s;
    }
}
