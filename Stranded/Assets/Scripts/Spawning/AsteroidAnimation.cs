using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AsteroidAnimation : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    public float minSpeed;
    public float maxSpeed;
    [SerializeField] private float minSize;
    [SerializeField] private float maxSize;
    private float timer;

    [SerializeField] private int delay;
    private int[] prefabNums;
    private Vector3[] distances;
    private float[] speeds;
    private Vector3[] directions;
    private Vector3[] scales;
    private Vector3[] rots;
    private int index;

    void Start()
    {
        prefabNums = new int[20/delay];
        distances = new Vector3[20/delay];
        speeds = new float[20/delay];
        directions = new Vector3[20/delay];
        scales = new Vector3[20/delay];
        rots = new Vector3[20/delay];
        for (int i = 0; i < 20/delay; i++)
        {
            prefabNums[i] = Random.Range(0, prefabs.Length);
            Vector3 distance = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0f, 1.0f), 0);
            distances[i] = Vector3.Normalize(distance) * Random.Range(minDistance, maxDistance);
            float scale = Random.Range(minSize, maxSize);
            if (minSize != maxSize)
                scales[i] = new Vector3(scale*Random.Range(0.5f, 1), scale*Random.Range(0.5f, 1), 1);
            else
                scales[i] = new Vector3(2, 2, 2);
            var euler = transform.eulerAngles;
            euler.z = Random.Range(0, 360);
            rots[i] = euler;
            speeds[i] = Random.Range(minSpeed, maxSpeed);
            directions[i] = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0);
            directions[i] = Vector3.Normalize(directions[i]);
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            SpawnAsteroid(minDistance, maxDistance);
        }
    }

    public void SpawnAsteroid(float minDistance, float maxDistance)
    {
        timer = delay;

        GameObject obj = Instantiate(prefabs[prefabNums[index]], transform.position + distances[index], transform.rotation);
        //Set asteroid values
        obj.transform.eulerAngles = rots[index];
        obj.transform.localScale = scales[index];
        AsteroidBehavior ast = obj.GetComponent<AsteroidBehavior>();
        if (ast != null)
        {
            ast.speed.Value = speeds[index];
            ast.direction.Value = directions[index];
        }
        else
        {
            ResourceBehavior res = obj.GetComponent<ResourceBehavior>();
            res.speed.Value = speeds[index];
            res.direction.Value = directions[index];
        }
        index = (index+1) % (20/delay);
    }
}
