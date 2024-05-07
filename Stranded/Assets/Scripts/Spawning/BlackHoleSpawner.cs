using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BlackHoleSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private GameObject exclamationPoint;
    public float minDelay;
    public float maxDelay;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    private float timer;

    void Start()
    {
        timer = 10; //could change based on difficulty
    }

    void Update()
    {
        if (IsServer)
        {
            timer -= Time.deltaTime;
            
            if (timer <= 0)
            {
                Spawn();
            }
        }
    }

    public void Spawn()
    {
        timer = Random.Range(minDelay, maxDelay);
        Vector3 distance = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0);
        distance = Vector3.Normalize(distance) * Random.Range(minDistance, maxDistance);
        GameObject obj = Instantiate(prefab, transform.position + distance, transform.rotation);
        
        //Set values
        /*BlackHole script = obj.GetComponent<BlackHole>();
        script.lifetime = __; //could randomize this between a range*/
        obj.GetComponent<NetworkObject>().Spawn(true);


        //Display exclamation point
        Rect canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>().rect;
        Vector3 canvasScale = GameObject.Find("Canvas").GetComponent<RectTransform>().localScale;
        float screenX = canvasRect.width * canvasScale.x/2;
        float screenY = canvasRect.height * canvasScale.y/2;
        //Screen is 26.67 x 15 @ 15 size
        float cameraScale = GameObject.Find("Main Camera").GetComponent<Camera>().orthographicSize/15;
        if (Mathf.Abs(distance.x) > 26.67f*cameraScale || Mathf.Abs(distance.y) > 15*cameraScale)
        {
            GameObject ePoint = Instantiate(exclamationPoint, transform.position, Quaternion.identity, GameObject.Find("Canvas").transform);
            if (Mathf.Abs((distance.y/distance.x)*screenX) < screenY) //going off side of screen
            {
                Debug.Log("Off the side");
                ePoint.GetComponent<RectTransform>().anchoredPosition = Mathf.Sign(distance.x) * new Vector2(screenX - 20, (distance.y/distance.x)*screenX - 30);
            }
            else //going off top/bottom of screen
            {
                Debug.Log("Off the top/bottom");
                ePoint.GetComponent<RectTransform>().anchoredPosition = Mathf.Sign(distance.y) * new Vector2((distance.x/distance.y)*screenY - 20, screenY - 30);
            }
        }
    }
}
