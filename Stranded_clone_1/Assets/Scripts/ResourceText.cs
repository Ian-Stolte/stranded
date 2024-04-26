using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceText : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(DestroyAfterDelay());
    }

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}
