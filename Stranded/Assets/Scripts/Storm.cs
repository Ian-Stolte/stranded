using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Storm : NetworkBehaviour
{
    public float minDelay;
    public float maxDelay;
    public float timer = 120;
    private bool doingStorm;
    private bool firstStorm = true;

    public List<int> disabledStations;
    public List<int> stationsUnlocked;
    private int index;
    
    public PlayerStations player;

    void Start()
    {
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
                StartStormServerRpc();
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void StartStormServerRpc()
    {
        StartCoroutine(StartStorm());
        StartStormClientRpc();
    }

    [Rpc(SendTo.NotServer)]
    public void StartStormClientRpc()
    {
        StartCoroutine(StartStorm());
    }

    public IEnumerator StartStorm()
    {
        doingStorm = true;
        /*if (firstStorm)
        {
            GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Voice - Storm " + Random.Range(1, 3));
            firstStorm = false;
        }*/
        GameObject.Find("Storm Text").GetComponent<Animator>().Play("StormText");
        yield return new WaitForSeconds(4);
        GameObject.Find("Screen Flash White").GetComponent<Animator>().Play("ScreenFlashLong");
        GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Storm Start");
        StartCoroutine(GameObject.Find("Audio Manager").GetComponent<AudioManager>().StartFade("Storm Start", 1, 0.1f));
        if (IsServer)
        {
            if (stationsUnlocked.Count == 3)
                DisableStationServerRpc();
            else
            {
                DisableStationServerRpc();
                DisableStationServerRpc();
            }
        }
        yield return new WaitForSeconds(10);
        disabledStations = new List<int>();
        StartCoroutine(GameObject.Find("Audio Manager").GetComponent<AudioManager>().StartFade("Storm Start", 1, 0));
        timer = Random.Range(minDelay, maxDelay);
        doingStorm = false;
    }

    [Rpc(SendTo.Server)]
    private void DisableStationServerRpc()
    {
        int index = Random.Range(0, stationsUnlocked.Count);
        disabledStations.Add(stationsUnlocked[index]);
        if ((player.currentStation == "steering" && stationsUnlocked[index] == 1) || (player.currentStation == "thrusters" && stationsUnlocked[index] == 2) || (player.currentStation == "shields" && stationsUnlocked[index] == 3) || (player.currentStation == "grabber" && stationsUnlocked[index] == 4) || (player.currentStation == "radar" && stationsUnlocked[index] == 5))
        {
            if (player.currentStation == "grabber")
                {
                    player.sync.WriteGrabberFiringRpc(false, false);
                    player.grabberFired = false;
                }
            player.currentStation = "none";
            player.HideInstructions();
        }
        DisableStationClientRpc(index);
    }

    [Rpc(SendTo.NotServer)]
    private void DisableStationClientRpc(int index)
    {
        disabledStations.Add(stationsUnlocked[index]);
        if ((player.currentStation == "steering" && stationsUnlocked[index] == 1) || (player.currentStation == "thrusters" && stationsUnlocked[index] == 2) || (player.currentStation == "shields" && stationsUnlocked[index] == 3) || (player.currentStation == "grabber" && stationsUnlocked[index] == 4) || (player.currentStation == "radar" && stationsUnlocked[index] == 5))
        {
            if (player.currentStation == "grabber")
                {
                    player.grabberFired = false;
                }
            player.currentStation = "none";
            player.HideInstructions();
        }
    }
}
