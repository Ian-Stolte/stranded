using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] float minDelay;
    [SerializeField] float maxDelay;
    [SerializeField] float minDistance;
    [SerializeField] float maxDistance;
    float timer;

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            timer = Random.Range(minDelay, maxDelay);
            Vector3 distance = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0);
            distance = Vector3.Normalize(distance) * Random.Range(minDistance, maxDistance);
            Instantiate(prefab, transform.position+distance, transform.rotation, GameObject.Find("Asteroids").transform);
        }
    }
}
