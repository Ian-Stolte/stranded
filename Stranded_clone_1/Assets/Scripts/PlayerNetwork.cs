using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class PlayerNetwork : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> station = new NetworkVariable<FixedString64Bytes>(writePerm: NetworkVariableWritePermission.Owner);
    GameObject ship;
    Sync sync;

    void Start()
    {
        ship = GameObject.Find("Spaceship");
        sync = GameObject.Find("Sync Object").GetComponent<Sync>();
    }

    void Update()
    {
        if (IsOwner) {
            station.Value = GetComponent<PlayerStations>().currentStation;
        }
        else {
            GetComponent<PlayerStations>().currentStation = "" + station.Value;
        }
    }
}
