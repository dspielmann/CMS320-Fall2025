using System.Collections;
using UnityEngine;
using TMPro;

public class DashTutorialManager : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement player;               // Reference to PlayerMovement script
    public TextMeshProUGUI countdownText;       // The numeric countdown UI
    public TextMeshProUGUI tutorialText;        // The text messages UI
    public GameObject rhythmBar;                // The rhythm bar (disabled during tutorial)

    [Header("Countdown Settings")]
    public float preDelay = 1f;
    public float countInterval = 1f;
    public float falseStartPenalty = 2f;

    [Header("Dive / Start Animation")]
    public float diveAnimDistanceX = 1f;        // X movement during dive
    public float diveAnimDepth = -0.2f;         // Z depth movement for dive
    public float diveAnimDuration = 0.25f;

    [Header("Dash Phase")]
    public float dashDuration = 2f;             // Time user must spam space
    public float tutorialGlideSpeed = 3f;       // Speed after dash ends

    private Coroutine countdownRoutine = null;
    private bool isCountdownRunning = false;
    private bool canDive = false;

    private float origGoodBoost, origBadPenalty;

    void Start()
    {
        if (rhythmBar != null)
            rhythmBar.SetActive(false);

        // Start the special tutorial text sequence
        StartCoroutine(TutorialIntroSequence());
    }

    // ------------------------------------------------------
    //  INTRO TEXT SEQUENCE (YOUR REQUESTED TIMING)
    // ------------------------------------------------------
    private IEnumerator TutorialIntroSequence()
    {
        tutorialText.text = "Congrats on learning how to Dive!";
        yield return new WaitForSeconds(2f);

        tutorialText.text = "Let's Learn how to Dash!";
        yield return new WaitForSeconds(2f);

        tutorialText.text = "Right after the dive, you must spam the SPACE BAR for 2 SECONDS to move forward!";
        yield return new WaitForSeconds(4f);

        tutorialText.text = "Good Luck!";
        yield return new WaitForSeconds(1.5f);

        tutorialText.text = "";

        // Now begin countdown
        StartCountdown();
    }

    private void StartCountdown()
    {
        if (countdownRoutine != null)
            StopCoroutine(countdownRoutine);

        countdownRoutine = StartCoroutine(CountdownSequence());
    }

    private IEnumerator CountdownSequence()
    {
        isCountdownRunning = true;
        canDive = false;

        // Disable movement until dive
        player.enabled = false;

        countdownText.text = "";
        yield return new WaitForSeconds(preDelay);

        // 3 → 2 → 1 countdown
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(countInterval);
        }

        countdownText.text = "START!";
        canDive = true;
        isCountdownRunning = false;

        yield return new WaitForSeconds(0.6f);
        countdownText.text = "";
    }

    void Update()
    {
        // False start (press right arrow early)
        if (isCountdownRunning && Input.GetKeyDown(KeyCode.RightArrow))
        {
            StartCoroutine(HandleFalseStart());
        }

        // Correct dive input
        if (canDive && Input.GetKeyDown(KeyCode.RightArrow))
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

        countdownText.text = "FALSE START!";
        yield return new WaitForSeconds(falseStartPenalty);

        countdownText.text = "";
        StartCountdown();
    }

    private IEnumerator HandleDiveStart()
    {
        canDive = false;
        isCountdownRunning = false;

        // Dive animation movement
        Vector3 startPos = player.transform.position;
        Vector3 endPos = startPos + new Vector3(diveAnimDistanceX, diveAnimDepth, 0f);
        float t = 0f;

        while (t < diveAnimDuration)
        {
            t += Time.deltaTime;
            player.transform.position = Vector3.Lerp(startPos, endPos, t / diveAnimDuration);
            yield return null;
        }

        // Lock on player movement
        player.enabled = true;

        // Backup rhythm stats
        origGoodBoost = player.goodRhythmBoost;
        origBadPenalty = player.badRhythmPenalty;

        // Disable rhythm penalties/bonuses
        player.goodRhythmBoost = 0f;
        player.badRhythmPenalty = 0f;

        // Allow dashing without rhythm mode
        player.tutorialMode = false;

        // Slow initial speed
        player.SetSpeed(1.5f);

        // -----------------------------
        // DASH TEXT SHOWN HERE
        // -----------------------------
        tutorialText.text = "SPAM SPACE BAR!";

        // Force player into dash phase
        player.StartUnderwaterKickPhase(dashDuration);

        yield return new WaitForSeconds(dashDuration + 0.05f);

        // Restore rhythm system
        player.goodRhythmBoost = origGoodBoost;
        player.badRhythmPenalty = origBadPenalty;

        // Speed after dash
        player.SetSpeed(tutorialGlideSpeed);

        tutorialText.text = "Nice! Dash complete.";
    }
}
