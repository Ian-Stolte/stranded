//Author: Ian Stolte
//Date: 7/13/23
//Desc: Manages the game audio (sound effects are called from other scripts, level music is called here)

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [Header("Editable")]
    public Sound[] music;
    public Sound[] sfx;

    [Header("Don't edit")]
    public AudioSource[] audios;

    [HideInInspector] public string currentSong;

    void Awake()
    {
        foreach (Sound s in music)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
        foreach (Sound s in sfx)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
        audios = gameObject.GetComponents<AudioSource>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        /*foreach (Sound s in music)
        {
            s.source.Stop();
        }*/
        StartCoroutine(FadeOutAll(1));
        if (scene.name == "Multiplayer")
        {
            Play("Stranded");
            Play("Shop");
            Play("Danger 1");
            Play("Danger 2");
            Sound s = Array.Find(music, sound => sound.name == "Stranded");
            s.source.volume = 0;
            StartCoroutine(StartFade("Stranded", 2, 0.4f));
            
        }
        else if (scene.name == "Start Screen")
        {
            Play("Start Screen");
            Sound s = Array.Find(music, sound => sound.name == "Start Screen");
            s.source.volume = 0;
            StartCoroutine(StartFade("Start Screen", 1, 0.4f));
        }
        else if (scene.name == "Game Over")
        {
            Time.timeScale = 1;
            Play("Game Over");
            Sound s = Array.Find(music, sound => sound.name == "Game Over");
            s.source.volume = 0;
            StartCoroutine(StartFade("Game Over", 2, 0.3f));
        }
        else if (scene.name == "Win Screen")
        {
            Time.timeScale = 1;
            Play("Win Game");
            Sound s = Array.Find(music, sound => sound.name == "Win Game");
            s.source.volume = 0;
            StartCoroutine(StartFade("Win Game", 2, 0.3f));
        }
    }
    
    public IEnumerator FadeOutAll(float duration)
    {
        List<string> songsToFade = new List<string>();
        foreach (Sound s in music)
        {
            if (s.source.volume != 0)
            {
                StartCoroutine(StartFade(s.name, duration, 0));
                songsToFade.Add(s.name);
            }
        }
        yield return new WaitForSeconds(duration);
        foreach (Sound s in music)
        {
            if (songsToFade.Contains(s.name))
                s.source.Stop();
        }
    }

    public IEnumerator QuietAll(float duration, float n)
    {
        foreach (Sound s in music)
        {
            if (s.source.volume != 0)
                StartCoroutine(StartFade(s.name, duration, s.source.volume*n));
        }
        yield return new WaitForSeconds(duration);
        foreach (Sound s in music)
        {
            if (s.source.volume != 0)
                StartCoroutine(StartFade(s.name, duration, s.source.volume/n));
        }

    }

    public void Play(string name)
    {
        Sound s = Array.Find(sfx, sound => sound.name == name);
        if (s == null)
            s = Array.Find(music, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("Sound: " + name + " not found!");
            return;
        }
        s.source.Play();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sfx, sound => sound.name == name);
        if (s == null)
            s = Array.Find(music, sound => sound.name == name);        
        if (s == null)
        {
            Debug.Log("Sound: " + name + " not found!");
            return;
        }
        s.source.Stop();
    }

    public IEnumerator StartFade(string name, float duration, float end)
    {
        Sound s = Array.Find(sfx, sound => sound.name == name);
        if (s == null)
            s = Array.Find(music, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("Sound: " + name + " not found!");
            yield break;
        }

        float currentTime = 0;
        float start = s.source.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            s.source.volume = Mathf.Lerp(start, end, currentTime / duration);
            yield return null;
        }

        //if (end == 0)
        //    s.source.Stop();
    }
}