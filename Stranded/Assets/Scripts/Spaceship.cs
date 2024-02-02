using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spaceship : MonoBehaviour
{
    public float turnSpeed;
    public float thrustSpeed;
    public float decelSpeed;
    public float maxSpeed;

    void Update()
    {
        Debug.Log(GetComponent<Rigidbody2D>().velocity);
        GetComponent<Rigidbody2D>().velocity *= (1 - decelSpeed);
        Vector2 vel = GetComponent<Rigidbody2D>().velocity;
        //reduce to max speed
        while (Mathf.Sqrt(Mathf.Pow(vel.x, 2) + Mathf.Pow(vel.y, 2)) > maxSpeed)
        {
            //reduce velcity linearly so it is at maxSpeed
            //vel.x *= 0.99f;
            //vel.y *= 0.99f;
        }
    }
}
