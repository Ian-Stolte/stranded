using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Boosts/EmergencyFuel")]
public class EmergencyFuel : BoostEffect
{
    public int amount;

    public override void Apply(GameObject target)
    {
        // Update fuel variable
        target.GetComponent<Spaceship>().fuelAmount.Value = Mathf.Min(target.GetComponent<Spaceship>().fuelAmount.Value+amount, target.GetComponent<Spaceship>().fuelMax);
        
        // Update fuel bar
        GameObject.Find("Fuel Bar").GetComponent<Image>().fillAmount = target.GetComponent<Spaceship>().fuelAmount.Value/target.GetComponent<Spaceship>().fuelMax;
        GameObject.Find("Sync Object").GetComponent<Sync>().ChangeFuelClientRpc(target.GetComponent<Spaceship>().fuelAmount.Value, target.GetComponent<Spaceship>().fuelMax);
    }
}