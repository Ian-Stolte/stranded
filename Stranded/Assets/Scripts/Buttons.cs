using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Buttons : MonoBehaviour
{
    public GameObject target;
    private GameObject[] players;
    private Transform buttons;
    public Storm storm;

    void Start()
    {
        buttons = transform.GetChild(0);
    }

    void Update()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < 5; i++)
            CheckInUse(i);

        //swap between stations with number keys
        if (Input.GetKeyDown(KeyCode.Alpha1) && buttons.GetChild(0).GetComponent<Button>().interactable && !storm.disabledStations.Contains(1))
        {
            ChangeTo("steering");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && buttons.GetChild(1).GetComponent<Button>().interactable && !storm.disabledStations.Contains(2))
        {
            ChangeTo("thrusters");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && buttons.GetChild(2).GetComponent<Button>().interactable && !storm.disabledStations.Contains(3))
        {
            ChangeTo("shields");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && buttons.GetChild(3).GetComponent<Button>().interactable && GameObject.Find("Spaceship").GetComponent<Spaceship>().grabberUnlocked && !storm.disabledStations.Contains(4))
        {
            ChangeTo("grabber");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5) && buttons.GetChild(4).GetComponent<Button>().interactable && GameObject.Find("Spaceship").GetComponent<Spaceship>().radarUnlocked && !storm.disabledStations.Contains(5))
        {
            ChangeTo("radar");
        }
    }

    void ChangeTo(string station)
    {
        if (!GameObject.Find("Sync Object").GetComponent<Sync>().paused.Value)
        {
            PlayerStations p = target.GetComponent<PlayerStations>();
            p.HideInstructions();

            if (p.currentStation == "grabber")
            {
                GameObject.Find("Sync Object").GetComponent<Sync>().WriteGrabberFiringRpc(false);
                p.grabberFired = false;
            }
            p.currentStation = station;
            if (station == "radar")
                p.usedRadar = true;
            if (GameObject.Find("Shop Manager").GetComponent<ShopManager>().shop.activeSelf)
                GameObject.Find("Shop Manager").GetComponent<ShopManager>().CloseShopServerRpc();
        }
    }
    
    public void CheckInUse(int index)
    {
        Transform child = buttons.GetChild(index);
        bool inUse = false;
        foreach (GameObject p in players)
        {
            if (child.name.ToLower() == p.GetComponent<PlayerStations>().station.Value)
            {
                child.GetComponent<Button>().interactable = false;
                inUse = true;
            }
        }
        child.GetComponent<Button>().interactable = (!inUse && !storm.disabledStations.Contains(index+1));
        child.GetChild(2).gameObject.SetActive(storm.disabledStations.Contains(index+1));
    }

    public void SetStation(string s)
    {
        if (!GameObject.Find("Shop Manager").GetComponent<ShopManager>().shop.activeSelf)
        {
            target.GetComponent<PlayerStations>().currentStation = s;
            if (s == "radar")
                target.GetComponent<PlayerStations>().usedRadar = true;
        }
    }
}