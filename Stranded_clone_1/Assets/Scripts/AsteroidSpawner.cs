using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AsteroidSpawner : NetworkBehaviour
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
                GameObject obj = Instantiate(prefab, transform.position + distance, transform.rotation, GameObject.Find("Asteroids").transform);
                //Set asteroid values
                obj.transform.localScale = new Vector3(Random.Range(minSize, maxSize), Random.Range(minSize, maxSize), 1);
                var euler = transform.eulerAngles;
                euler.z = Random.Range(0, 360);
                obj.transform.eulerAngles = euler;
                AsteroidBehavior ast = obj.GetComponent<AsteroidBehavior>();
                ast.speed.Value = Random.Range(minSpeed, maxSpeed);
                ast.direction.Value = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0);
                ast.direction.Value = Vector3.Normalize(ast.direction.Value);
                obj.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}