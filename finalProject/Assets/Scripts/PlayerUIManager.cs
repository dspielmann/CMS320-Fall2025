using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUIManager : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement player;
    public Slider rhythmBar;
    public Image rhythmFill;
    public TextMeshProUGUI speedText;

    [Header("Colors")]
    public Color normalColor = Color.red;
    public Color goodTimingColor = Color.green;
    public Color badTimingColor = Color.red;

    private float lastKeyTime;

    void Start()
    {
        lastKeyTime = Time.time;

        if (rhythmBar)
        {
            rhythmBar.value = 0;
        }
    }

    void Update()
    {
        // --- Rhythm Bar Update ---
        float elapsed = Time.time - player.GetLastKeyTime();

        // Use LIVE rhythm timing from the player, not cached values
        float minRhythmTime = player.minRhythmTime;
        float maxRhythmTime = player.maxRhythmTime;

        float normalized = Mathf.Clamp01(elapsed / maxRhythmTime);
        rhythmBar.value = normalized;

        if (elapsed >= minRhythmTime && elapsed <= maxRhythmTime)
            rhythmFill.color = goodTimingColor;
        else
            rhythmFill.color = normalColor;

        // --- Speed Text Update ---
        float speed = player.GetCurrentSpeed();
        speedText.text = $"Speed: {speed:F1} m/s";

        if (Mathf.Approximately(speed, player.maxSpeed))
        {
            speedText.color = Color.yellow;
        }
        else
        {
            speedText.color = Color.white;
        }
    }
}
