5using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShipwreckSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private float minDelay;
    [SerializeField] private float maxDelay;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    [SerializeField] private int maxAtOnce;
    /*[SerializeField] private float minSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float resourceValue;*/
    private float timer;

    void Update()
    {
        if (IsServer)
        {
            timer -= Time.deltaTime;
            
            if (timer <= 0 && GameObject.FindGameObjectsWithTag("Shipwreck").Length < maxAtOnce)
            {
                SpawnShipwreck();
            }
        }
    }

    public void SpawnShipwreck()
    {
        timer = Random.Range(minDelay, maxDelay);
        Vector3 distance = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0);
        distance = Vector3.Normalize(distance) * Random.Range(minDistance, maxDistance);
        while (Physics2D.OverlapCircle(transform.position + distance, 20, LayerMask.GetMask("Shipwreck")))
        {
            distance = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0);
            distance = Vector3.Normalize(distance) * Random.Range(minDistance, maxDistance);
        }
        GameObject obj = Instantiate(prefab, transform.position + distance, transform.rotation);
        
        //Set resource values
        ResourceBehavior res = obj.GetComponent<ResourceBehavior>();
        /*res.value.Value = resourceValue; //could randomize this between a range
        res.speed.Value = Random.Range(minSpeed, maxSpeed);
        res.direction.Value = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0);
        res.direction.Value = Vector3.Normalize(res.direction.Value);*/
        obj.GetComponent<NetworkObject>().Spawn(true);
    }
}
