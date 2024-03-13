using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Shield : MonoBehaviour
{
    public GameObject anchor;
    public int velocity;

    // Start is called before the first frame update
    void Start()
    {
        velocity = 20;
    }

    // Update is called once per frame
    void Update()
    {
        
        
        if (Input.GetKey(KeyCode.Z))
        {
            transform.RotateAround(anchor.transform.localPosition, Vector3.forward, velocity);
        }

        if (Input.GetKey(KeyCode.C))
        {
            transform.RotateAround(anchor.transform.localPosition, Vector3.back, velocity);
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.gameObject.name == "AsteroidClone")
        {
            collider.gameObject.GetComponent<NetworkObject>().Despawn(true);
        }
    }
}
