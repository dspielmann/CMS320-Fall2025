using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RaceStartManager : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement player;
    public TextMeshProUGUI countdownText;
    public GameObject divingBlock;
    public GameObject rhythmBar;

    [Header("Timer UI")]
    public TextMeshProUGUI raceTimerText;

    [Header("Timing Settings")]
    public float preDelay = 1f;
    public float countInterval = 1f;
    public float falseStartPenalty = 2f;

    private bool canDive = false;
    private bool raceStarted = false;
    private bool isCountdownRunning = false;
    private Coroutine countdownRoutine;

    // Timer vars
    private float raceStartTime = 0f;
    private bool raceTimerRunning = false;

    // -------------------------------
    // TUTORIAL MODE
    // -------------------------------
    [Header("Tutorial Mode")]
    public bool tutorialMode = false;
    public TextMeshProUGUI tutorialText;

    void Start()
    {
        if (tutorialMode)
        {
            StartCoroutine(TutorialSequence());
        }
        else
        {
            StartCountdown();
        }
    }

    // -----------------------------------------------------------
    // TUTORIAL SEQUENCE
    // -----------------------------------------------------------
    private IEnumerator TutorialSequence()
    {
        player.enabled = false;
        countdownText.text = "";
        if (raceTimerText != null) raceTimerText.text = "00.00";

        // Hide rhythm bar during tutorial
        if (rhythmBar != null)
            rhythmBar.SetActive(false);

        // 1) “Let's Learn How to Dive!”
        if (tutorialText != null)
            tutorialText.text = "Let's Learn How to Dive!";
        yield return new WaitForSeconds(2f);

        // 2) Instructions
        if (tutorialText != null)
            tutorialText.text = "When the countdown displays \"Start!\", press the RIGHT ARROW KEY";
        yield return new WaitForSeconds(5f);

        // 3) “Good Luck!”
        if (tutorialText != null)
            tutorialText.text = "Good Luck!";
        yield return new WaitForSeconds(2f);

        // Clear text
        if (tutorialText != null)
            tutorialText.text = "";

        // Start standard countdown
        StartCountdown();
    }

    // -----------------------------------------------------------
    // NORMAL COUNTDOWN
    // -----------------------------------------------------------
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

        // Hide rhythm bar for tutorial AND normal countdown
        if (rhythmBar != null)
            rhythmBar.SetActive(false);

        if (raceTimerText != null)
            raceTimerText.text = "00.00";

        countdownText.text = "";
        yield return new WaitForSeconds(preDelay);

        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(countInterval);
        }

        countdownText.text = "START!";
        canDive = true;
        isCountdownRunning = false;

        raceStartTime = Time.time;
        raceTimerRunning = true;

        yield return new WaitForSeconds(0.75f);
        countdownText.text = "";
    }

    // -----------------------------------------------------------
    // UPDATE LOOP
    // -----------------------------------------------------------
    void Update()
    {
        // Timer
        if (raceTimerRunning && raceTimerText != null)
        {
            float elapsed = Time.time - raceStartTime;
            raceTimerText.text = $"{elapsed:F2}";
        }

        // FALSE START
        if (isCountdownRunning && Input.GetKeyDown(KeyCode.RightArrow))
        {
            StartCoroutine(HandleFalseStart());
        }

        // NORMAL DIVE
        if (canDive && !raceStarted && Input.GetKeyDown(KeyCode.RightArrow))
        {
            StartCoroutine(HandleDiveStart());
        }
    }

    // -----------------------------------------------------------
    // FALSE START
    // -----------------------------------------------------------
    private IEnumerator HandleFalseStart()
    {
        if (countdownRoutine != null)
            StopCoroutine(countdownRoutine);

        isCountdownRunning = false;
        canDive = false;
        raceStarted = false;

        countdownText.text = "FALSE START!";

        yield return new WaitForSeconds(falseStartPenalty);

        countdownText.text = "";
        StartCountdown();
    }

    // -----------------------------------------------------------
    // DIVE START
    // -----------------------------------------------------------
    private IEnumerator HandleDiveStart()
    {
        canDive = false;
        raceStarted = true;
        isCountdownRunning = false;

        // Dive animation
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

        player.enabled = true;

        // ======================================================
        // TUTORIAL MODE BEHAVIOR
        // NO RHYTHM
        // NO UNDERWATER KICKS
        // JUST GLIDE FORWARD
        // ======================================================
        if (tutorialMode)
        {
            player.SetSpeed(3f); // simple glide to finish
            yield break; // skip the rest
        }

        // ======================================================
        // NORMAL LEVEL BEHAVIOR
        // ======================================================

        // Rhythm bar appears only in normal mode
        if (rhythmBar != null)
        {
            rhythmBar.SetActive(true);
            yield return null;

            var slider = rhythmBar.GetComponent<Slider>();
            if (slider != null)
            {
                slider.minValue = 0f;
                slider.maxValue = 1f;
                slider.value = 0f;
                slider.normalizedValue = 0f;
            }
        }

        // Initial dive speed
        player.SetSpeed(2f);

        // Rhythm + underwater mechanics
        player.ResetRhythmTimer();
        player.StartUnderwaterKickPhase(2f);
    }

    // -----------------------------------------------------------
    // FINISH LINE
    // -----------------------------------------------------------
    public void StopRaceTimer()
    {
        if (raceTimerRunning)
        {
            raceTimerRunning = false;

            float finalTime = Time.time - raceStartTime;
            if (raceTimerText != null)
            {
                raceTimerText.text = $"Final Time: {finalTime:F2}s";
            }
        }
    }
}
