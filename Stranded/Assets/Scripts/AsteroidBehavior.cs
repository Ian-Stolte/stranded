using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidBehavior : MonoBehaviour
{
    [Header("Spawn Parameters")]
    [SerializeField] float minSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float minSize;
    [SerializeField] float maxSize;
    [SerializeField] float despawnDistance;

    [Header("Movement")]
    [SerializeField] float speed;
    [SerializeField] Vector3 direction;

    GameObject ship;

    void Start()
    {
        speed = Random.Range(minSpeed, maxSpeed);
        transform.localScale = new Vector3(Random.Range(minSize, maxSize), Random.Range(minSize, maxSize), 1);
    	var euler = transform.eulerAngles;
	    euler.z = Random.Range(0, 360);
	    transform.eulerAngles = euler;
        direction = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0);
        direction = Vector3.Normalize(direction);

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
