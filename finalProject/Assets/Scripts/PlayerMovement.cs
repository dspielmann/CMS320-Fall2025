using UnityEngine;
using System.Collections; // ✅ Needed for IEnumerator

public class PlayerMovement : MonoBehaviour
{
    [Header("Speed Settings")]
    public float baseSpeed = 0f;
    public float goodRhythmBoost = 1.2f;
    public float badRhythmPenalty = 0.6f;
    public float maxSpeed = 4f;
    public float slowDecay = 2f;

    [Header("Rhythm Settings")]
    public float minRhythmTime = 0.75f;
    public float maxRhythmTime = 1.25f;

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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
    }

    void Update()
    {
        HandleDiveInput();
        HandleRhythmInput();
        ApplyNaturalSlowdown();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(currentSpeed, 0f);
    }

    // ------------------------------
    // DIVE / DUCKING SYSTEM
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
    // RHYTHM MOVEMENT SYSTEM
    // ------------------------------
    private void HandleRhythmInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            KeyCode currentKey = Input.GetKeyDown(KeyCode.LeftArrow) ? KeyCode.LeftArrow : KeyCode.RightArrow;
            float timeSinceLast = Time.time - lastKeyTime;

            if (isUnderwaterKickPhase)
            {
                // During underwater phase: just boost slightly with each press
                currentSpeed += 0.1f;
                currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);
            }
            else
            {
                // Normal rhythm timing check
                if (currentKey != lastKey && timeSinceLast >= minRhythmTime && timeSinceLast <= maxRhythmTime)
                {
                    currentSpeed += goodRhythmBoost;
                    currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);
                }
                else
                {
                    currentSpeed -= badRhythmPenalty;
                    if (currentSpeed < 0f) currentSpeed = 0f;
                }
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
            currentSpeed = 0;
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

        while (Time.time < underwaterKickEndTime)
        {
            yield return null;
        }

        isUnderwaterKickPhase = false;
        Debug.Log("Underwater phase ended — switching to surface swimming.");
    }

    // ------------------------------
    // UI GETTERS / HELPERS
    // ------------------------------
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public float GetLastKeyTime()
    {
        return lastKeyTime;
    }

    public void SetSpeed(float newSpeed)
    {
        currentSpeed = Mathf.Clamp(newSpeed, 0f, maxSpeed);
    }

    public void ResetRhythmTimer()
    {
        lastKeyTime = Time.time;
    }
}
