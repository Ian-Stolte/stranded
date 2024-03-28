using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Boosts/ReplenishHealth")]
public class ReplenishHealth : BoostEffect
{
    public int amount;

    public override void Apply(GameObject target)
    {
        // Update health variable
        target.GetComponent<Spaceship>().shipHealth.Value += amount;
        
        // Update health bar
        GameObject.Find("Ship Damage Bar").GetComponent<ResourceBar>().ChangeResourceToAmount(target.GetComponent<Spaceship>().shipHealth.Value, target.GetComponent<Spaceship>().shipHealthMax);
        GameObject.Find("Sync Object").GetComponent<Sync>().ChangeHealthClientRpc(target.GetComponent<Spaceship>().shipHealth.Value, target.GetComponent<Spaceship>().shipHealthMax);
    }
}