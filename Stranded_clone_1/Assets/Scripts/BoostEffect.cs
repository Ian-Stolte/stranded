using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// From https://www.youtube.com/watch?v=PkNRPOrtyls
// A template for boosts
public abstract class BoostEffect : ScriptableObject
{
    public string itemName;
    public string itemDescription;
    public int baseCost;
    public abstract void Apply(GameObject target);
}
