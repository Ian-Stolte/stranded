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
    private string[] stationNames = new string[] { "steering", "thrusters", "shields", "grabber", "radar" };
    private int index;

    void Start()
    {
        buttons = transform.GetChild(1);
    }

    void Update()
    {
        if (Input.mouseScrollDelta.y < 0)
        {
            index = (index + 1) % 3;
            ChangeTo(index);
        }
        else if (Input.mouseScrollDelta.y > 0)
        {
            index = (index + 2) % 3; //same as (index - 1) % 3, except for negatives
            ChangeTo(index);
        }
        
        players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < 5; i++)
            CheckInUse(i);

        //swap between stations with number keys
        if (Input.GetKeyDown(KeyCode.Alpha1) && buttons.GetChild(0).GetComponent<Button>().interactable && !storm.disabledStations.Contains(1))
        {
            ChangeTo(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && buttons.GetChild(1).GetComponent<Button>().interactable && !storm.disabledStations.Contains(2))
        {
            ChangeTo(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && buttons.GetChild(2).GetComponent<Button>().interactable && !storm.disabledStations.Contains(3))
        {
            ChangeTo(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && buttons.GetChild(3).GetComponent<Button>().interactable && GameObject.Find("Spaceship").GetComponent<Spaceship>().grabberUnlocked && !storm.disabledStations.Contains(4))
        {
            ChangeTo(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5) && buttons.GetChild(4).GetComponent<Button>().interactable && GameObject.Find("Spaceship").GetComponent<Spaceship>().radarUnlocked && !storm.disabledStations.Contains(5))
        {
            ChangeTo(4);
        }
    }

    void ChangeTo(int n)
    {
        if (!GameObject.Find("Sync Object").GetComponent<Sync>().paused.Value)
        {
            index = n;
            PlayerStations p = target.GetComponent<PlayerStations>();
            p.HideInstructions();

            if (p.currentStation == "grabber")
            {
                GameObject.Find("Sync Object").GetComponent<Sync>().WriteGrabberFiringRpc(false);
                p.grabberFired = false;
            }
            p.currentStation = stationNames[n];
            if (stationNames[n] == "radar")
                p.usedRadar = true;
            if (GameObject.Find("Shop Manager").GetComponent<ShopManager>().shop.activeSelf)
                GameObject.Find("Shop Manager").GetComponent<ShopManager>().CloseShopServerRpc();
        }
    }
    
    public void CheckInUse(int index)
    {
        Transform child = buttons.GetChild(index);
        bool inUse = false;
        transform.GetChild(0).GetChild(index).gameObject.SetActive(false);
        
        foreach (GameObject p in players)
        {
            if (child.name.ToLower() == p.GetComponent<PlayerStations>().station.Value)
            {
                if (p == target)
                {
                    transform.GetChild(0).GetChild(index).gameObject.SetActive(true);
                }
                else
                {
                    child.GetComponent<Button>().interactable = false;
                    inUse = true;
                }   
            }
        }
        //if (storm.disabledStations.Contains(index+1))
        //    child.GetComponent<Button>().interactable = false;
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