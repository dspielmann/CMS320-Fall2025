using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUIManager : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement player;
    public Image rhythmCircle;     // NEW — CIRCLE ONLY
    public TextMeshProUGUI speedText;

    [Header("Colors")]
    public Color normalColor = Color.red;
    public Color goodColor = Color.green;

    void Update()
    {
        float elapsed = Time.time - player.GetLastKeyTime();

        // Rhythm window from player
        float minTime = player.minRhythmTime;
        float maxTime = player.maxRhythmTime;

        // COLOR CHANGE ONLY
        if (elapsed >= minTime && elapsed <= maxTime)
            rhythmCircle.color = goodColor;
        else
            rhythmCircle.color = normalColor;

        // Speed text
        float speed = player.GetCurrentSpeed();
        speedText.text = $"Speed: {speed:F1} m/s";
    }
}
