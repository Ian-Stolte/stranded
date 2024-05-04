using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Boosts/ReplenishHealth")]
public class ReplenishHealth : BoostEffect
{
    public int amount;

    public override void Apply(GameObject target)
    {
        // Update health variable
        target.GetComponent<Spaceship>().fuelAmount.Value = Mathf.Min(target.GetComponent<Spaceship>().shipHealth.Value+amount, target.GetComponent<Spaceship>().shipHealthMax);
        
        // Update health bar
        GameObject.Find("Ship Damage Bar").GetComponent<Image>().fillAmount = target.GetComponent<Spaceship>().shipHealth.Value/target.GetComponent<Spaceship>().shipHealthMax;
        GameObject.Find("Sync Object").GetComponent<Sync>().ChangeHealthClientRpc(target.GetComponent<Spaceship>().shipHealth.Value, target.GetComponent<Spaceship>().shipHealthMax);
    }
}