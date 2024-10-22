using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShipwreckSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private GameObject arrowPrefab;
    public float minDelay;
    public float maxDelay;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    public int maxAtOnce;
    [SerializeField] private int minValue;
    [SerializeField] private int maxValue;
    private float timer;
    private float elapsedTime;

    private GameObject ship;

    void Start()
    {
        ship = GameObject.Find("Spaceship");
    }

    void Update()
    {
        if (IsServer)
        {
            timer -= Time.deltaTime;
            elapsedTime += Time.deltaTime;

            if (timer <= 0 && GameObject.FindGameObjectsWithTag("Shipwreck").Length < maxAtOnce - Mathf.Min(0, 2*(elapsedTime/300 - 1)))
            {
                SpawnShipwreck();
            }
        }
    }

    public void SpawnShipwreck()
    {
        timer = Random.Range(minDelay, maxDelay) + Mathf.Min(0, 10*(elapsedTime/300 - 1));
        Vector3 distance = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0);
        Vector3 veloAdd = new Vector3(GameObject.Find("Spaceship").GetComponent<Rigidbody2D>().velocity.x, GameObject.Find("Spaceship").GetComponent<Rigidbody2D>().velocity.y, 0);
        while (Vector3.Magnitude(distance) < minDistance + Mathf.Min(0, 30*(elapsedTime/300 - 1)) || Vector3.Magnitude(distance) > maxDistance + Mathf.Min(0, 30*(elapsedTime/300 - 1)) || Physics2D.OverlapCircle(transform.position + distance, 30, LayerMask.GetMask("Shipwreck")))
        {
            distance = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0);
            distance = Vector3.Normalize(distance) * (Random.Range(minDistance, maxDistance) + Mathf.Min(0, 30*(elapsedTime/300 - 1))) + veloAdd * 4; 
        }
        GameObject wreck = Instantiate(prefab, transform.position + distance, transform.rotation);
        GameObject arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
        
        //Set arrow values
        Vector3 radarDir = Vector3.Normalize(wreck.transform.position - ship.transform.position);
        Vector3 rot = new Vector3(0, 0, Mathf.Atan2(radarDir.y, radarDir.x) * Mathf.Rad2Deg - 90);
        arrow.transform.GetChild(0).transform.rotation = (Quaternion.Euler(rot));
        arrow.GetComponent<TMPro.TextMeshPro>().text = Mathf.Round(Vector3.Distance(ship.transform.position, wreck.transform.position)) + " km";
        float cameraZoom = GameObject.Find("Main Camera").GetComponent<Camera>().orthographicSize;
        arrow.transform.position = ship.transform.position + Vector3.Normalize(wreck.transform.position - ship.transform.position) * (cameraZoom - 5);
        arrow.transform.localScale = new Vector3(1 + (cameraZoom-15)/50, 1 + (cameraZoom-15)/50, 1);
        arrow.SetActive(false);
        arrow.GetComponent<NetworkObject>().Spawn(true);

        //Set resource values
        ShipwreckBehavior script = wreck.GetComponent<ShipwreckBehavior>();
        script.value.Value = Random.Range(minValue, maxValue+1);
        script.radarArrow = arrow;
        wreck.GetComponent<NetworkObject>().Spawn(true);
        wreck.transform.SetParent(GameObject.Find("Shipwrecks").transform);

        wreck.GetComponent<ShipwreckBehavior>().SetArrowClientRpc(arrow);
    }
}