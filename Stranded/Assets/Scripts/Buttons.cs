using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Buttons : MonoBehaviour
{
    public GameObject target;
    GameObject[] players;

    void Update()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        foreach (Transform child in transform)
        {
            bool inUse = false;
            foreach (GameObject p in players)
            {
                if (child.name.ToLower() == p.GetComponent<PlayerNetwork>().station.Value)
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
    }

    public void SetStation(string s)
    {
        target.GetComponent<PlayerStations>().currentStation = s;
    }
}
