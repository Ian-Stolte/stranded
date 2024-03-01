using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class ResourceBehavior : NetworkBehaviour
{
    public NetworkVariable<float> speed;
    public NetworkVariable<Vector3> direction;
    [SerializeField] float despawnDistance;
    //public float resourcesCollected = 0;
    //public TMP_Text resourcesText;

    GameObject ship;

    void Start()
    {
        ship = GameObject.Find("Spaceship");
    }

    void Update()
    {
        //resourcesText.SetText("Resources Collected:" + resourcesCollected);
    }

    void OnCollisionEnter2D()
    {
        //resourcesCollected = resourcesCollected + 1;
    }

    void FixedUpdate()
    {
        transform.position += speed.Value * direction.Value * Time.deltaTime;
        if (Vector3.Distance(transform.position, ship.transform.position) > despawnDistance && IsServer)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
    }
}
