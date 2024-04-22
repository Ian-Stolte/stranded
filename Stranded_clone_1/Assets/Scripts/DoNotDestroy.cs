using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoNotDestroy : MonoBehaviour
{
    [SerializeField] string objectTag;

    void Awake()
    {
        GameObject[] obj = GameObject.FindGameObjectsWithTag(objectTag);
        if (obj.Length > 1)
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }
}