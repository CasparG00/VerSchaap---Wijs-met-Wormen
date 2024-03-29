using UnityEngine;
using UnityEngine.UI;

public class QuickAdviceDisplay : MonoBehaviour
{
    [SerializeField] private Gradient colorGradient;
    
    private Image[] dots;
    private Image[] Dots => dots ??= GetComponentsInChildren<Image>();

    private bool isActive;

    // /// <summary>
    // /// Test function. Uses the surface quality instead of the value from the verweid formula
    // /// </summary>
    // public void UpdateDotsBasedOnQuality(float surfaceQuality)
    // {
    //     for (int i = 0; i < Dots.Length; i++)
    //     {
    //         Dots[i].color = 100.0f / Dots.Length * i > 100 - surfaceQuality ? Color.gray : colorGradient.Evaluate(1 - surfaceQuality / 100f);
    //     }
    // }

    public void UpdateDots(float value)
    {
        float fill = Mathf.InverseLerp(-50f, 200f, value);
        fill = Mathf.Clamp01(fill);

        int amount = Mathf.CeilToInt(fill * Dots.Length);

        for (int i = 0; i < Dots.Length; i++)
        {
            Dots[i].color = i < amount ? colorGradient.Evaluate((amount - 1) / (Dots.Length - 1f)) : Color.gray;
        }
    }
    
    public void SetActive(bool value)
    {
        foreach (var dot in dots)
        {
            dot.enabled = value;
        }
    }
}
