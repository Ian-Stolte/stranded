using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ResourceSpawner : NetworkBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] float minDelay;
    [SerializeField] float maxDelay;
    [SerializeField] float minDistance;
    [SerializeField] float maxDistance;
    [SerializeField] float minSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float minSize;
    [SerializeField] float maxSize;
    float timer;

    void Update()
    {
        if (IsServer)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                timer = Random.Range(minDelay, maxDelay);
                Vector3 distance = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0);
                distance = Vector3.Normalize(distance) * Random.Range(minDistance, maxDistance);
                GameObject obj = Instantiate(prefab, transform.position + distance, transform.rotation, GameObject.Find("Resources").transform);
                //Set Resource values
                ResourceBehavior res = obj.GetComponent<ResourceBehavior>();
                res.speed.Value = Random.Range(minSpeed, maxSpeed);
                res.direction.Value = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0);
                res.direction.Value = Vector3.Normalize(res.direction.Value);
                obj.GetComponent<NetworkObject>().Spawn();
                Debug.Log("spawned");
            }
        }
    }
}