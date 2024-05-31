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
            AsteroidSoundServerRpc();
            if (IsServer)
                collider.gameObject.GetComponent<NetworkObject>().Despawn(true);
        }
    }

    [Rpc(SendTo.Server)]
    void AsteroidSoundServerRpc()
    {
        GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Asteroid Destroy");
        AsteroidSoundClientRpc();
    }

    [Rpc(SendTo.NotServer)]
    void AsteroidSoundClientRpc()
    {
        GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Asteroid Destroy");
    }
}
