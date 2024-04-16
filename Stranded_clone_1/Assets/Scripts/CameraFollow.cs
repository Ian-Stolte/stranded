using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    GameObject ship;
    [SerializeField] Vector2 offset;
    Vector3 pastPos;
    float speed;
    float pastSpeed;
    bool inDecel;

    void Start()
    {
        ship = GameObject.Find("Spaceship");
    }

    /*void Update()
    {*/
    public void UpdateCamera(Vector3 posChange)
    {
        speed = Mathf.Sqrt(Mathf.Pow(ship.GetComponent<Rigidbody2D>().velocity.x, 2) + Mathf.Pow(ship.GetComponent<Rigidbody2D>().velocity.y, 2));
        if (speed - pastSpeed < -1)
        {
            inDecel = true;
            StartCoroutine(Decelerate());
        }
        pastSpeed = speed;

        if (!inDecel)
        {
            /*if (Vector3.Magnitude(posChange) != 0)
            {
                offset = new Vector2(posChange.x, posChange.y) * 60;
                if (Vector3.Magnitude(offset) > 6)
                {
                    offset *= Mathf.Pow(Vector3.Magnitude(offset)-6, 1.5f) / 30; //slight exponential growth (^1.5) from 6 onwards
                }
                else
                {
                    offset = new Vector2(0, 0);
                }
            }
            transform.position = new Vector3(ship.transform.position.x + offset.x, ship.transform.position.y + offset.y, transform.position.z);*/
            transform.position = new Vector3(ship.transform.position.x, ship.transform.position.y, transform.position.z);
            if (speed > 5)
            {
                GetComponent<Camera>().orthographicSize = 15 + Mathf.Pow((speed-5), 2) / 5; //quadratic growth from 5 to 10
            }
        }
    }

    IEnumerator Decelerate()
    {
        for (float i = 0; i < 1; i += 0.01f)
        {
            offset *= 0.95f;
            transform.position = new Vector3(ship.transform.position.x + offset.x, ship.transform.position.y + offset.y, transform.position.z);
            GetComponent<Camera>().orthographicSize = ((GetComponent<Camera>().orthographicSize-15) * 0.95f) + 15;
            yield return new WaitForSeconds(0.01f);
        }
        inDecel = false;
        pastPos = ship.transform.position;
    }
}