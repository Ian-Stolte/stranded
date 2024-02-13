using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TestMovement : NetworkBehaviour
{
    public float speed;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            Destroy(this);
    }

    void FixedUpdate()
    {
        transform.position += new Vector3(Input.GetAxisRaw("Horizontal")*speed, Input.GetAxisRaw("Vertical")*speed, 0);
    }
}
