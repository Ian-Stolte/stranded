using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour
{
    [SerializeField] private float delay;

    void Start()
    {
        StartCoroutine(DestroySoon());
    }

    IEnumerator DestroySoon()
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
