using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Buttons : MonoBehaviour
{
    public GameObject target;
    private GameObject[] players;
    private Transform buttons;

    void Start()
    {
        buttons = transform.GetChild(0);
    }

    void Update()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        CheckInUse(buttons.GetChild(0));
        CheckInUse(buttons.GetChild(1));
        CheckInUse(buttons.GetChild(2));
        CheckInUse(buttons.GetChild(3));
        CheckInUse(buttons.GetChild(4));

        //swap between stations with number keys
        if (Input.GetKeyDown(KeyCode.Alpha1) && buttons.GetChild(0).GetComponent<Button>().interactable)
        {
            ChangeTo("steering");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && buttons.GetChild(1).GetComponent<Button>().interactable)
        {
            ChangeTo("thrusters");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && buttons.GetChild(2).GetComponent<Button>().interactable)
        {
            ChangeTo("shields");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && buttons.GetChild(3).GetComponent<Button>().interactable)
        {
            ChangeTo("grabber");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5) && buttons.GetChild(4).GetComponent<Button>().interactable)
        {
            ChangeTo("radar");
        }
    }

    void ChangeTo(string station)
    {
        PlayerStations p = target.GetComponent<PlayerStations>();
        p.HideInstructions();
        
        if (p.currentStation == "grabber")
        {
            GameObject.Find("Sync Object").GetComponent<Sync>().WriteGrabberFiringRpc(false);
            p.grabberFired = false;
        }
        p.currentStation = station;
    }
    
    public void CheckInUse(Transform child)
    {
        bool inUse = false;
        foreach (GameObject p in players)
        {
            if (child.name.ToLower() == p.GetComponent<PlayerStations>().station.Value)
            {
                child.GetComponent<Button>().interactable = false;
                inUse = true;
            }
        }
        if (!inUse)
        {
            child.GetComponent<Button>().interactable = true;
        }
    }

    public void SetStation(string s)
    {
        target.GetComponent<PlayerStations>().currentStation = s;
    }
}