using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Sync : NetworkBehaviour
{
//TODO: Fix inability for client to write to vars
    public NetworkVariable<Vector3> shipPos = new NetworkVariable<Vector3>();
    public NetworkVariable<Quaternion> shipRot = new NetworkVariable<Quaternion>();
}
