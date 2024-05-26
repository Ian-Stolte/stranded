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
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
        {
            g.GetComponent<PlayerStations>().enabled = false;
        }
        GameObject.Find("Button Functions").GetComponent<ButtonFunctions>().LoadSceneServerRpc("Win Screen");
    }
}