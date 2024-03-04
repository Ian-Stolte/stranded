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

    // [SerializeField] private int resourceAbsoluteMax = 1000;
    //[Space]
    //[SerializeField] private bool overkillPossible;

    
    // [Header("Animation Speed")]
    // [SerializeField, Range(0,0.5f)] private float _animationTime = 0.25f;
    // private Coroutine _fillRoutine;

    
    // [Header("Text Settings")]
    // [SerializeField] private DisplayType howToDisplayValueText = DisplayType.Percentage;
    // [SerializeField] private TMP_Text resourceValueTextField;
    
    // public enum DisplayType
    // {
    //     [InspectorName("Long (50|100)")]
    //     LongValue,
    //     [InspectorName("Short (50)")]
    //     ShortValue,
    //     [InspectorName("Percent (85%)")]
    //     Percentage,
    //     None
    // }
    
    // [Header("Events")]
    // [SerializeField] private UnityEvent barIsFilledUp;
    // private float _previousFillAmount;
    
    // [Header("Test mode")] 
    // [SerializeField] private bool enableTesting;

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