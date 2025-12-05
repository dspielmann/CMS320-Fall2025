using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SwimTutorialManager : MonoBehaviour
{

    [Header("References")]
    public Animator animator;
    public PlayerMovement player;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI tutorialText;
    public GameObject rhythmCircle;

    [Header("Countdown Settings")]
    public float preDelay = 1f;
    public float countInterval = 1f;

    [Header("Dive Animation")]
    public float diveAnimDistanceX = 1f;
    public float diveAnimDepth = -0.18f;
    public float diveAnimDuration = 0.25f;

    [Header("Dash Phase")]
    public float dashDuration = 2f;
    public float dashGlideSpeed = 3f;

    [Header("Swim Phase")]
    public float swimStartSpeed = 1.5f;

    private bool isCountdownRunning = false;
    private bool canStart = false;
    private Coroutine countdownRoutine = null;

    // Backup original player values
    private float origGoodBoost, origBadPenalty;
    private bool origTutorialMode;

    // Backup original rhythm window (added)
    private float origMinRhythm, origMaxRhythm;

    void Start()
    {
        // Backup player settings
        origGoodBoost = player.goodRhythmBoost;
        origBadPenalty = player.badRhythmPenalty;
        origTutorialMode = player.tutorialMode;

        // Backup rhythm timing window (added)
        origMinRhythm = player.minRhythmTime;
        origMaxRhythm = player.maxRhythmTime;

        if (rhythmCircle != null)
            rhythmCircle.SetActive(false);

        // Start the intro tutorial sequence
        StartCoroutine(TutorialIntro());
    }

    // -------------------------
    // INTRO SEQUENCE
    // -------------------------
    private IEnumerator TutorialIntro()
    {
        player.enabled = false;

        tutorialText.text = "Great! Now let’s learn how to SWIM.";
        yield return new WaitForSeconds(2f);

        tutorialText.text = "Tap LEFT and RIGHT in a rhythm to swim forward.";
        yield return new WaitForSeconds(3f);

        tutorialText.text = "We will start with the dive, then the dash, then swimming.";
        yield return new WaitForSeconds(3f);

        tutorialText.text = "Ready?";
        yield return new WaitForSeconds(1.2f);

        tutorialText.text = "";

        StartCountdown();
    }

    // -------------------------
    // START COUNTDOWN
    // -------------------------
    private void StartCountdown()
    {
        if (countdownRoutine != null)
            StopCoroutine(countdownRoutine);

        countdownRoutine = StartCoroutine(CountdownSequence());
    }

    private IEnumerator CountdownSequence()
    {
        isCountdownRunning = true;
        canStart = false;
        player.enabled = false;

        countdownText.text = "";
        yield return new WaitForSeconds(preDelay);

        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(countInterval);
        }

        countdownText.text = "START!";
        canStart = true;
        isCountdownRunning = false;

        yield return new WaitForSeconds(0.6f);
        countdownText.text = "";
    }

    void Update()
    {
        // False start detection
        if (isCountdownRunning && Input.GetKeyDown(KeyCode.RightArrow))
        {
            StartCoroutine(FalseStart());
        }

        // Start dive if allowed
        if (canStart && Input.GetKeyDown(KeyCode.RightArrow))
        {
            animator.SetBool("isDiving", true);
            StartCoroutine(DiveStart());
        }
    }

    // -------------------------
    // FALSE START (simplified)
    // -------------------------
    private IEnumerator FalseStart()
    {
        // Stop current countdown
        if (countdownRoutine != null)
            StopCoroutine(countdownRoutine);

        isCountdownRunning = false;
        canStart = false;

        countdownText.text = "FALSE START!";
        tutorialText.text = "Wait for START!";

        // Wait briefly so the player sees the message
        yield return new WaitForSeconds(1f);

        countdownText.text = "";
        tutorialText.text = "";

        // Restart countdown
        StartCountdown();
    }

    // -------------------------
    // DIVE → DASH → SWIM
    // -------------------------
    private IEnumerator DiveStart()
    {
        canStart = false;

        // Dive animation
        Vector3 startPos = player.transform.position;
        Vector3 endPos = startPos + new Vector3(diveAnimDistanceX, diveAnimDepth, 0f);

        float t = 0f;
        while (t < diveAnimDuration)
        {
            t += Time.deltaTime;
            player.transform.position = Vector3.Lerp(startPos, endPos, t / diveAnimDuration);
            yield return null;
        }

        // Enable player movement
        player.enabled = true;

        // DASH phase
        tutorialText.text = "DASH! SPAM SPACE BAR!";
        player.goodRhythmBoost = 0f;
        player.badRhythmPenalty = 0f;

        player.SetSpeed(1.2f);
        player.StartUnderwaterKickPhase(dashDuration);

        yield return new WaitForSeconds(dashDuration + 0.1f);

        // DASH finished
        tutorialText.text = "Nice! Now SWIM!";
        player.SetSpeed(dashGlideSpeed);

        // Restore rhythm system
        player.goodRhythmBoost = origGoodBoost;
        player.badRhythmPenalty = origBadPenalty;
        player.tutorialMode = false;

        // Enable rhythm bar
        if (rhythmCircle != null)
            rhythmCircle.SetActive(true);

        // Swim phase begins — ends only on Finish trigger
        StartSwimPhase();
    }

    // -------------------------
    // SWIM PHASE
    // -------------------------
    private void StartSwimPhase()
    {
        tutorialText.text = "Tap LEFT / RIGHT to swim!";
        player.SetSpeed(swimStartSpeed);

        // ★ Apply easier tutorial-only rhythm window
        player.minRhythmTime = 0.5f;
        player.maxRhythmTime = 5f;
    }

    private void OnDestroy()
    {
        // Restore original player settings if tutorial is interrupted
        player.goodRhythmBoost = origGoodBoost;
        player.badRhythmPenalty = origBadPenalty;
        player.tutorialMode = origTutorialMode;

        // Restore rhythm timings
        player.minRhythmTime = origMinRhythm;
        player.maxRhythmTime = origMaxRhythm;
    }
}
