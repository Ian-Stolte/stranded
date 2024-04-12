using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AsteroidSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private float minDelay;
    [SerializeField] private float maxDelay;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    [SerializeField] private float minSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float minSize;
    [SerializeField] private float maxSize;
    private float timer;

    void Update()
    {
        if (IsServer)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                SpawnAsteroid(minDistance, maxDistance);
            }
        }
    }

    public void SpawnAsteroid(float minDistance, float maxDistance)
    {
        timer = Random.Range(minDelay, maxDelay);
        Vector3 distance = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0);
        Vector3 veloAdd = new Vector3(GameObject.Find("Spaceship").GetComponent<Rigidbody2D>().velocity.x, GameObject.Find("Spaceship").GetComponent<Rigidbody2D>().velocity.y, 0);
        while (Vector3.Magnitude(distance) < minDistance || Vector3.Magnitude(distance) > maxDistance)
        {
            distance = Vector3.Normalize(distance) * Random.Range(0, maxDistance) + veloAdd * 4;
        }
        GameObject obj = Instantiate(prefab, transform.position + distance, transform.rotation);
        //Set asteroid values
        obj.transform.localScale = new Vector3(Random.Range(minSize, maxSize), Random.Range(minSize, maxSize), 1);
        var euler = transform.eulerAngles;
        euler.z = Random.Range(0, 360);
        obj.transform.eulerAngles = euler;
        AsteroidBehavior ast = obj.GetComponent<AsteroidBehavior>();
        ast.speed.Value = Random.Range(minSpeed, maxSpeed);
        ast.direction.Value = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0);
        ast.direction.Value = Vector3.Normalize(ast.direction.Value);
        obj.GetComponent<NetworkObject>().Spawn(true);
    }
}
