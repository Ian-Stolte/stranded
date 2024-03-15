using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Shield : MonoBehaviour
{
    public GameObject anchor;
    public float velocity;

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.RotateAround(anchor.transform.localPosition, Vector3.forward, velocity);
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.RotateAround(anchor.transform.localPosition, Vector3.back, velocity);
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.name == "AsteroidClone")
        {
            collider.gameObject.GetComponent<NetworkObject>().Despawn(true);
        }
    }
}
