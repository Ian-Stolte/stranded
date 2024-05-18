using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Storm : NetworkBehaviour
{
    public float minDelay;
    public float maxDelay;
    private float timer;
    private bool doingStorm;

    public List<int> disabledStations;
    public List<int> stationsUnlocked;
    private int index;
    
    public PlayerStations player;

    void Start()
    {
        timer = 5; //could change based on difficulty
        stationsUnlocked.Add(1);
        stationsUnlocked.Add(2);
        stationsUnlocked.Add(3);
    }

    void Update()
    {
        if (IsServer && !doingStorm)
        {
            timer -= Time.deltaTime;
            
            if (timer <= 0)
            {
                StartCoroutine(StartStorm());
            }
        }
    }

    public IEnumerator StartStorm()
    {
        doingStorm = true;
        GameObject.Find("Storm Text").GetComponent<Animator>().Play("StormText");
        yield return new WaitForSeconds(4);
        GameObject.Find("Screen Flash White").GetComponent<Animator>().Play("ScreenFlashLong");

        if (stationsUnlocked.Count == 3)
            DisableStation();
        else
        {
            DisableStation();
            DisableStation();
        }
        yield return new WaitForSeconds(10);
        disabledStations = new List<int>();
        timer = Random.Range(minDelay, maxDelay);
        doingStorm = false;
    }

    private void DisableStation()
    {
        int index = Random.Range(0, stationsUnlocked.Count);
        disabledStations.Add(stationsUnlocked[index]);
        if ((player.currentStation == "steering" && stationsUnlocked[index] == 1) || (player.currentStation == "thrusters" && stationsUnlocked[index] == 2) || (player.currentStation == "shields" && stationsUnlocked[index] == 3) || (player.currentStation == "grabber" && stationsUnlocked[index] == 4) || (player.currentStation == "radar" && stationsUnlocked[index] == 5))
        {
            Debug.Log("Kick player out!");
            if (player.currentStation == "grabber")
                {
                    player.sync.WriteGrabberFiringRpc(false);
                    player.grabberFired = false;
                }
            player.currentStation = "none";
            player.HideInstructions();
        }
    }
}
