using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BlackHole : NetworkBehaviour
{
    public CircleCollider2D pullRadius;
    public CircleCollider2D killRadius;
    [SerializeField] private float pullStrength;

    void Update()
    {
        Collider2D[] nearbyObjs = Physics2D.OverlapCircleAll(transform.position, pullRadius.radius*transform.localScale.x, LayerMask.GetMask("Asteroid", "Resource", "Shipwreck", "Spaceship"));
        foreach (Collider2D g in nearbyObjs)
        {
            Vector3 dist = transform.position - g.transform.position;
            float proximity = 1 - (Vector3.Magnitude(dist) / (pullRadius.radius * transform.localScale.x));
            g.transform.position += Vector3.Normalize(dist) * pullStrength * proximity;
        }

        Collider2D[] killObjs = Physics2D.OverlapCircleAll(transform.position, killRadius.radius*transform.localScale.x, LayerMask.GetMask("Asteroid", "Resource", "Shipwreck"));
        foreach (Collider2D g in killObjs)
        {
            if (IsServer)
            {
                Debug.Log(g.name);
                g.GetComponent<NetworkObject>().Despawn(true);
            }
        }
    }
}
