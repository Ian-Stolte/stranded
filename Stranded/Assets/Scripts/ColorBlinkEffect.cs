using UnityEngine;
using UnityEngine.UI;
 
public class ColorBlinkEffect : MonoBehaviour
{
    public Color startColor = Color.red;
    public Color endColor = Color.white;
    [Range(0,10)]
    public float speed = 1;
 
    Image imgComp;
 
    void Awake()
    {
        imgComp = GetComponent<Image>();
        Debug.Log("Awake");
        Blink();
    }
 
    void Blink()
    {
        Debug.Log("Blinking");
        // for (int i = 0; i < 5; i++)
            imgComp.color = Color.Lerp(startColor, endColor, Mathf.PingPong(Time.time * speed, 1));
    }
}