// From a tutorial by ChristinaCreatesGames on YouTube. 
// Link: https://www.youtube.com/watch?v=6y4_jwZNYMQ

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ResourceBar : MonoBehaviour
{
    [Header("Core Settings")]
    [SerializeField] private Image bar;

    public void GameOver() //probably only needed if we do a game over UI on the bar
    {
        //add game over UI
        bar.fillAmount = 0;
    }

    public void ChangeResourceToAmount(float current, float max)
    {
        bar.fillAmount = current / max;
    }
}