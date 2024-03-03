// From a tutorial by ChristinaCreatesGames on YouTube. 
// Link: https://www.youtube.com/watch?v=6y4_jwZNYMQ

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShipDamage : MonoBehaviour
{
    [Header("Core Settings")]
    [SerializeField] private Image bar;
    [SerializeField] private int resourceCurrent = 10;
    [SerializeField] private int resourceMax = 10;
    // [SerializeField] private int resourceAbsoluteMax = 1000;
    [Space]
    [SerializeField] private bool overkillPossible;
    // [SerializeField] private ShapeType shapeOfBar;

    // public enum ShapeType
    // {
    //     [InspectorName("Rectangle (Horizontal)")]
    //     RectangleHorizontal,
    //     [InspectorName("Rectangle (Vertical)")]
    //     RectangleVertical,
    //     [InspectorName("Circle")] 
    //     Circle,
    //     Arc
    // }
    
    // [Header("Arc Settings")]
    // [SerializeField, Range(0, 360)] private int endDegreeValue = 360;
    
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
    
    private void Start()
    {
        UpdateBarAndResourceText();
    }

    private void UpdateBarAndResourceText()
    {
        if (resourceMax <= 0)
        {
            bar.fillAmount = 0;
            return;
        }

        float fillAmount = (float) resourceCurrent/resourceMax;

        bar.fillAmount = fillAmount;
    }

    public void ChangeResourceToAmount(int amount)
    {
        resourceCurrent = amount;
        resourceCurrent = Mathf.Clamp(value: resourceCurrent, min: 0, resourceMax);

        bar.fillAmount = (float) resourceCurrent / resourceMax;

        return;
    }
}