using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BlackHole : NetworkBehaviour
{
    [SerializeField] private CircleCollider2D pullCollider;
    private float pullRadius;
    [SerializeField] private CircleCollider2D killCollider;
    private float killRadius;
    [SerializeField] private float maxPull;
    [SerializeField] private float pullStrength;
    [SerializeField] private float lifetime;
    private float timer;
    
    

    void Start()
    {
        timer = lifetime;
        pullRadius = pullCollider.radius * transform.localScale.x;
        killRadius = killCollider.radius * transform.localScale.x;
    }

    void Update()
    {
        if (!GameObject.Find("Sync Object").GetComponent<Sync>().paused.Value)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                GetComponent<NetworkObject>().Despawn(true);
            }
            pullStrength = maxPull * timer/lifetime;
    //TODO: fix bug where scale is assigned (NaN, NaN, NaN)
            transform.localScale = new Vector3(6, 6, 6) * Mathf.Sqrt((timer/lifetime));
            killRadius = killCollider.radius * transform.localScale.x;

            Collider2D[] nearbyObjs = Physics2D.OverlapCircleAll(transform.position, pullRadius, LayerMask.GetMask("Asteroid", "Resource", "Shipwreck", "Spaceship"));
            foreach (Collider2D g in nearbyObjs)
            {
                Vector3 dist = transform.position - g.transform.position;
                float proximity = 1 - (Vector3.Magnitude(dist)/pullRadius);
                if (g.name == "Spaceship")
                    g.transform.position += Vector3.Normalize(dist) * pullStrength * (proximity/2 + 0.5f);
                else
                    g.transform.position += 1.5f * Vector3.Normalize(dist) * pullStrength * (proximity/2 + 0.5f);
            }

            Collider2D[] killObjs = Physics2D.OverlapCircleAll(transform.position, killRadius, LayerMask.GetMask("Asteroid", "Resource", "Shipwreck", "Spaceship"));
            foreach (Collider2D g in killObjs)
            {
                if (IsServer)
                {
                    if (g.name == "Spaceship")
                        Debug.Log("Damage ship!"); //Damage, shoot ship out other side?
                    else
                        g.GetComponent<NetworkObject>().Despawn(true);
                }
            }
        }
    }
}
