using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Buttons : MonoBehaviour
{
    public GameObject target;
    private GameObject[] players;

    void Update()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        CheckInUse(transform.GetChild(0));
        CheckInUse(transform.GetChild(1));
        CheckInUse(transform.GetChild(2));
        CheckInUse(transform.GetChild(3));
//TODO: allow swap between stations without hitting Q?
        if (Input.GetKeyDown(KeyCode.Alpha1) && transform.GetChild(0).GetComponent<Button>().interactable)
        {
            target.GetComponent<PlayerStations>().currentStation = "steering";
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && transform.GetChild(1).GetComponent<Button>().interactable)
        {
            target.GetComponent<PlayerStations>().currentStation = "thrusters";
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && transform.GetChild(2).GetComponent<Button>().interactable)
        {
            target.GetComponent<PlayerStations>().currentStation = "shields";
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && transform.GetChild(3).GetComponent<Button>().interactable)
        {
            target.GetComponent<PlayerStations>().currentStation = "grabber";
        }
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