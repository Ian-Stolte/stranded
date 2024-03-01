using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    GameObject ship;
    [SerializeField] Vector2 offset;
    Vector3 pastPos;

    void Start()
    {
        ship = GameObject.Find("Spaceship");
    }

    void Update()
    {
        if (pastPos != ship.transform.position)
        {
            offset = new Vector2(ship.transform.position.x - pastPos.x, ship.transform.position.y - pastPos.y)*60;
            Debug.Log(Vector3.Magnitude(offset));
            if (Vector3.Magnitude(offset) > 6)
            {
                offset *= Mathf.Pow(Vector3.Magnitude(offset)-6, 1.5f)/30;
                print("New magnitude: " + Vector3.Magnitude(offset));
            }
            else
            {
                offset = new Vector2(0, 0);
            }
            pastPos = ship.transform.position;
        }
        transform.position = new Vector3(ship.transform.position.x + offset.x, ship.transform.position.y + offset.y, transform.position.z);
        float speed = Mathf.Sqrt(Mathf.Pow(ship.GetComponent<Rigidbody2D>().velocity.x, 2) + Mathf.Pow(ship.GetComponent<Rigidbody2D>().velocity.y, 2));
        if (speed > 5)
        {
            GetComponent<Camera>().orthographicSize = 15 + Mathf.Pow((speed - 5), 2)/5;
        }
//TODO: reduce shaking
//TODO: have camera move ahead of ship after a certain time/velocity
    }
}
