using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AsteroidSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] prefabs;
    public float minDelay;
    public float maxDelay;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    public float minSpeed;
    public float maxSpeed;
    [SerializeField] private float minSize;
    [SerializeField] private float maxSize;
    private float timer;
    [SerializeField] private float elapsedTime;

    void Update()
    {
        if (IsServer)
        {
            timer -= Time.deltaTime;
            elapsedTime += Time.deltaTime;

            if (timer <= 0)
            {
                SpawnAsteroid(minDistance, maxDistance);
            }
        }
    }

    public void SpawnAsteroid(float minDistance, float maxDistance)
    {
        //Set timer
        int asteroidDensity = Physics2D.OverlapCircleAll(transform.position, 40, LayerMask.GetMask("Asteroid")).Length;
        timer = Mathf.Max(0.5f, Random.Range(minDelay, maxDelay)*asteroidDensity/5 - elapsedTime/600);
        //Debug.Log(timer);

        Vector3 distance = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0);
        Vector3 veloAdd = new Vector3(GameObject.Find("Spaceship").GetComponent<Rigidbody2D>().velocity.x, GameObject.Find("Spaceship").GetComponent<Rigidbody2D>().velocity.y, 0);
        while (Vector3.Magnitude(distance) < minDistance || Vector3.Magnitude(distance) > maxDistance)
        {
            distance = Vector3.Normalize(distance) * Random.Range(0, maxDistance) + veloAdd * 4;
        }
        GameObject obj = Instantiate(prefabs[Random.Range(0, 2)], transform.position + distance, transform.rotation);
        //Set asteroid values
        float scale = Random.Range(minSize, maxSize);
        obj.transform.localScale = new Vector3(scale*Random.Range(0.5f, 1), scale*Random.Range(0.5f, 1), 1);
        var euler = transform.eulerAngles;
        euler.z = Random.Range(0, 360);
        obj.transform.eulerAngles = euler;
        AsteroidBehavior ast = obj.GetComponent<AsteroidBehavior>();
        ast.speed.Value = Random.Range(minSpeed, maxSpeed);
        ast.direction.Value = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0);
        ast.direction.Value = Vector3.Normalize(ast.direction.Value);
        obj.GetComponent<NetworkObject>().Spawn(true);
        obj.transform.SetParent(GameObject.Find("Asteroids").transform);
    }
}
