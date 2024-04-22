using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Boosts/EmergencyFuel")]
public class EmergencyFuel : BoostEffect
{
    public int amount;

    public override void Apply(GameObject target)
    {
        // Update fuel variable
        target.GetComponent<Spaceship>().fuelAmount.Value += amount;
        
        // Update fuel bar
        GameObject.Find("Fuel Bar").GetComponent<ResourceBar>().ChangeResourceToAmount(target.GetComponent<Spaceship>().fuelAmount.Value, target.GetComponent<Spaceship>().fuelMax);
        GameObject.Find("Sync Object").GetComponent<Sync>().ChangeFuelClientRpc(target.GetComponent<Spaceship>().fuelAmount.Value, target.GetComponent<Spaceship>().fuelMax);
    }
}