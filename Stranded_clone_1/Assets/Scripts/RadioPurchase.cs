using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Boosts/RadioPurchase")]
public class RadioPurchase : BoostEffect
{
    public override void Apply(GameObject target)
    {
        // Win condition!
        Debug.Log("You contacted civilization. They said they don't want to talk to you anymore.");
    }
}