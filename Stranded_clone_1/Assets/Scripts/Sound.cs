//Author: Ian Stolte
//Date: 7/13/23
//Desc: Stores info about audio objects (not attached to any GameObjects)

using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;
    [Range(-3f, 3f)]
    public float pitch;

    public bool loop;

    public AudioSource source;
}