using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [Header("Speed Settings")]
    public float baseSpeed = 0f;
    public float goodRhythmBoost = 1.2f;
    public float badRhythmPenalty = 0.6f;
    public float maxSpeed = 4f;
    public float slowDecay = 2f;

    [Header("Rhythm Settings")]
    public float minRhythmTime = 0.5f;
    public float maxRhythmTime = 2.5f;

    [Header("Dive Settings")]
    public float diveDuration = 1.5f;
    public float diveDepth = -0.5f;

    private bool isDiving = false;
    private float diveTimer = 0f;
    private Vector3 startPosition;

    private Rigidbody2D rb;
    private float currentSpeed = 0f;
    private float lastKeyTime = -10f;
    private KeyCode lastKey = KeyCode.None;

    // Underwater kick phase
    private bool isUnderwaterKickPhase = false;
    private float underwaterKickEndTime = 0f;

    [Header("UI References")]
    public TextMeshProUGUI underwaterTimerText;

    [Header("Underwater Timer Colors")]
    public Color fullTimeColor = Color.green;
    public Color midTimeColor = Color.yellow;
    public Color lowTimeColor = Color.red;

    // -------------------------------
    // Tutorial Mode Flag
    // -------------------------------
    [Header("Tutorial Mode")]
    public bool tutorialMode = false;

    [Header("Audio")]
    AudioManager AudioManager;

    void Awake()
    {
        if (underwaterTimerText != null)
            underwaterTimerText.gameObject.SetActive(false);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        AudioManager = GameObject.FindGameObjectWithTag("music").GetComponent<AudioManager>();
    }

    void Update()
    {
        // ---------------------------------------------------
        // Block swimming/rhythm/underwater if in tutorial
        // ---------------------------------------------------
        if (!tutorialMode)
        {
            HandleDiveInput();
            HandleRhythmInput();
        }

        // Slowdown works in all modes
        ApplyNaturalSlowdown();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(currentSpeed, 0f);
    }

    // ------------------------------
    // DIVE INPUT
    // ------------------------------
    private void HandleDiveInput()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow) && !isDiving)
        {
            isDiving = true;
            diveTimer = diveDuration;

            transform.position = new Vector3(transform.position.x, startPosition.y + diveDepth, transform.position.z);
            Debug.Log("Diving under obstacle!");
        }

        if (isDiving)
        {
            diveTimer -= Time.deltaTime;
            if (diveTimer <= 0f)
            {
                isDiving = false;
                transform.position = new Vector3(transform.position.x, startPosition.y, transform.position.z);
                Debug.Log("Surfaced again!");
            }
        }
    }

    // ------------------------------
    // RHYTHM INPUT
    // ------------------------------
    private void HandleRhythmInput()
    {
        if (isUnderwaterKickPhase)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                currentSpeed += 0.1f;
                currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);
                lastKeyTime = Time.time;
                Debug.Log("Underwater kick!");
                AudioManager.PlaySplashSXF(AudioManager.splashSound);
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            KeyCode currentKey = Input.GetKeyDown(KeyCode.LeftArrow) ? KeyCode.LeftArrow : KeyCode.RightArrow;
            float timeSinceLast = Time.time - lastKeyTime;

            if (currentKey != lastKey && timeSinceLast >= minRhythmTime && timeSinceLast <= maxRhythmTime)
            {
                currentSpeed += goodRhythmBoost;
                currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);
                AudioManager.PlaySplashSXF(AudioManager.splashSound);
            }
            else
            {
                currentSpeed -= badRhythmPenalty;
                if (currentSpeed < 0f) currentSpeed = 0f;
                AudioManager.PlayMissSXF(AudioManager.missedSound);
            }

            lastKey = currentKey;
            lastKeyTime = Time.time;
        }
    }

    // ------------------------------
    // NATURAL SLOWDOWN
    // ------------------------------
    private void ApplyNaturalSlowdown()
    {
        if (Time.time - lastKeyTime > maxRhythmTime)
        {
            currentSpeed -= slowDecay * Time.deltaTime;
            if (currentSpeed < 0f) currentSpeed = 0f;
        }
    }

    // ------------------------------
    // COLLISIONS
    // ------------------------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            if (!isDiving)
            {
                currentSpeed = Mathf.Max(currentSpeed - 3f, 0f);
                Debug.Log("Hit obstacle! Slowed down!");
            }
            else
            {
                Debug.Log("Dove under obstacle safely!");
            }
        }

        if (other.CompareTag("Finish"))
        {
            Debug.Log("You win!");
            rb.linearVelocity = Vector2.zero;
            currentSpeed = 0f;
            this.enabled = false;

            var uiText = GameObject.Find("CountdownText");
            if (uiText != null)
            {
                var tmp = uiText.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                    tmp.text = "FINISH!";
            }

            RaceStartManager raceManager = Object.FindFirstObjectByType<RaceStartManager>();
            if (raceManager != null)
            {
                raceManager.StopRaceTimer();
            }
        }
    }

    // ------------------------------
    // UNDERWATER KICK PHASE
    // ------------------------------
    public void StartUnderwaterKickPhase(float duration)
    {
        StartCoroutine(UnderwaterKickRoutine(duration));
    }

    private IEnumerator UnderwaterKickRoutine(float duration)
    {
        isUnderwaterKickPhase = true;
        underwaterKickEndTime = Time.time + duration;

        if (underwaterTimerText != null)
            underwaterTimerText.gameObject.SetActive(true);

        while (Time.time < underwaterKickEndTime)
        {
            float remaining = underwaterKickEndTime - Time.time;

            if (underwaterTimerText != null)
            {
                underwaterTimerText.text = $"Underwater: {remaining:F1}s";

                float t = remaining / duration;
                if (t > 0.6f)
                    underwaterTimerText.color = fullTimeColor;
                else if (t > 0.3f)
                    underwaterTimerText.color = midTimeColor;
                else
                    underwaterTimerText.color = lowTimeColor;
            }

            yield return null;
        }

        isUnderwaterKickPhase = false;
        if (underwaterTimerText != null)
            underwaterTimerText.gameObject.SetActive(false);

        Debug.Log("Underwater phase ended — switching to surface swimming.");
    }

    // ------------------------------
    // HELPERS
    // ------------------------------
    public float GetCurrentSpeed() => currentSpeed;
    public float GetLastKeyTime() => lastKeyTime;
    public void SetSpeed(float newSpeed) => currentSpeed = Mathf.Clamp(newSpeed, 0f, maxSpeed);
    public void ResetRhythmTimer() => lastKeyTime = Time.time;

    
}