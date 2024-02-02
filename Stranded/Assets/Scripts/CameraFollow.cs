using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] float followSpeed;
    GameObject ship;

    void Start()
    {
        ship = GameObject.Find("Spaceship");
    }

    void Update()
    {
        Vector3 pos = ship.transform.position;
//TODO: reduce shaking
//TODO: have camera move ahead of ship after a certain time/velocity
        transform.position += new Vector3((pos.x - transform.position.x)/followSpeed, (pos.y - transform.position.y)/followSpeed, 0);
    }
}
