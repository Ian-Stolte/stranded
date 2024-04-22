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

    public void ChangeResourceToAmount(float current, float max)
    {
        bar.fillAmount = current / max;
    }
}