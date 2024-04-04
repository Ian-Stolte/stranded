using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Shield : NetworkBehaviour
{
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Asteroid")
        {
            if (IsServer)
                collider.gameObject.GetComponent<NetworkObject>().Despawn(true);
        }
    }
}
