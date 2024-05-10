using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;

[CreateAssetMenu(menuName = "Boosts/RadioPurchase")]
public class RadioPurchase : BoostEffect
{
    public override void Apply(GameObject target)
    {
        // Win condition!
        Debug.Log("You contacted civilization. They said they don't want to talk to you anymore.");
        
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
        {
            g.GetComponent<PlayerStations>().enabled = false;
        }
        // if (IsServer)
        SceneManager.LoadScene("Win Screen", LoadSceneMode.Single);
    }
}