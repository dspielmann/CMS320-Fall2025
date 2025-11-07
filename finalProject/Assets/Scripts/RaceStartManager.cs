using UnityEngine;
using TMPro;
using System.Collections;

public class RaceStartManager : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement player;          
    public TextMeshProUGUI countdownText;  
    public GameObject divingBlock;         
    public GameObject rhythmBar;           

    [Header("Timing Settings")]
    public float preDelay = 1f;
    public float countInterval = 1f;
    public float falseStartPenalty = 2f;

    private bool canDive = false;
    private bool raceStarted = false;
    private bool isCountdownRunning = false;
    private Coroutine countdownRoutine;

    void Start()
    {
        StartCountdown();
    }

    private void StartCountdown()
    {
        if (countdownRoutine != null)
            StopCoroutine(countdownRoutine);

        countdownRoutine = StartCoroutine(StartCountdownSequence());
    }

    private IEnumerator StartCountdownSequence()
    {
        isCountdownRunning = true;
        canDive = false;
        raceStarted = false;
        player.enabled = false;

        if (rhythmBar != null)
            rhythmBar.SetActive(false);

        countdownText.text = "";
        yield return new WaitForSeconds(preDelay);

        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(countInterval);
        }

        // Show START! and allow diving
        countdownText.text = "START!";
        canDive = true;
        isCountdownRunning = false;

        yield return new WaitForSeconds(0.75f);
        countdownText.text = "";
    }

    void Update()
    {
        // FALSE START (only if countdown still running)
        if (isCountdownRunning && Input.GetKeyDown(KeyCode.RightArrow))
        {
            StartCoroutine(HandleFalseStart());
        }

        // NORMAL START
        if (canDive && !raceStarted && Input.GetKeyDown(KeyCode.RightArrow))
        {
            StartCoroutine(HandleDiveStart());
        }
    }

    private IEnumerator HandleFalseStart()
    {
        if (countdownRoutine != null)
            StopCoroutine(countdownRoutine);

        isCountdownRunning = false;
        canDive = false;
        raceStarted = false;

        countdownText.text = "FALSE START!";
        Debug.Log("False Start!");

        yield return new WaitForSeconds(falseStartPenalty);

        countdownText.text = "";
        StartCountdown();
    }

    private IEnumerator HandleDiveStart()
    {
        canDive = false;
        raceStarted = true;
        isCountdownRunning = false;

        // Small dive animation
        Vector3 startPos = player.transform.position;
        Vector3 endPos = startPos + new Vector3(1f, -0.2f, 0f);
        float t = 0f;
        float duration = 0.3f;
        while (t < duration)
        {
            t += Time.deltaTime;
            player.transform.position = Vector3.Lerp(startPos, endPos, t / duration);
            yield return null;
        }

        // Enable movement
        player.enabled = true;

        // Enable rhythm bar and reset to empty
        if (rhythmBar != null)
        {
            rhythmBar.SetActive(true);
            yield return null; // ensure UI updates

            var slider = rhythmBar.GetComponent<UnityEngine.UI.Slider>();
            if (slider != null)
            {
                slider.minValue = 0f;
                slider.maxValue = 1f;
                slider.value = 0f;
                slider.normalizedValue = 0f;
                Debug.Log("Rhythm bar reset to empty.");
            }
            else
            {
                Debug.LogWarning("RhythmBar does not have a Slider component!");
            }
        }

        // Give the player initial dive speed
        player.SetSpeed(2f);

        // Reset rhythm timer so bar starts empty
        player.ResetRhythmTimer();

        // Underwater kick phase for 2 seconds
        player.StartUnderwaterKickPhase(2f);

        Debug.Log("Dive Start + Underwater Kicks!");
    }
}