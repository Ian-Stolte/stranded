using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidBehavior : MonoBehaviour
{
    public float speed;
    public Vector3 direction;
    [SerializeField] float despawnDistance;

    GameObject ship;

    void Start()
    {
        ship = GameObject.Find("Spaceship");
    }

    void FixedUpdate()
    {
        transform.position += speed*direction*Time.deltaTime;
        if (Vector3.Distance(transform.position, ship.transform.position) > despawnDistance)
        {
            Destroy(gameObject);
        }
    }
}
