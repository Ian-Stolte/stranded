using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AsteroidBehavior : NetworkBehaviour
{
    public NetworkVariable<float> speed;
    public NetworkVariable<Vector3> direction;
    [SerializeField] private float despawnDistance;
    private bool isGrabbed;

    private GameObject ship;
    private GameObject grabber;

    void Start()
    {
        ship = GameObject.Find("Spaceship");
        grabber = GameObject.Find("Grabber");
    }

    void FixedUpdate()
    {
        /*Bounds b = grabber.GetComponent<BoxCollider2D>().bounds;
        Collider2D[] overlapObjs = Physics2D.OverlapBoxAll(b.center, b.extents * 2, 0, LayerMask.GetMask("Asteroid"));
        isGrabbed = false;
        foreach (Collider2D c in overlapObjs)
        {
            if (c.gameObject == gameObject && grabber.GetComponent<Grabber>().asteroidGrabbed.Value)
                isGrabbed = true;
        }
        if (!isGrabbed)*/
        transform.position += speed.Value * direction.Value * Time.deltaTime;
        if (Vector3.Distance(transform.position, ship.transform.position) > despawnDistance && IsServer)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
    }
}
