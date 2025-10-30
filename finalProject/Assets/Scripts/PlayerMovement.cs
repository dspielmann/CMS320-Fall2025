using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Speed Settings")]
    public float baseSpeed = 0f;
    public float goodRhythmBoost = 2f;
    public float badRhythmPenalty = 1f;
    public float maxSpeed = 8f;
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

            // TEMP visual dive feedback (for now)
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
        // ✅ allow rhythm taps even while diving
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            KeyCode currentKey = Input.GetKeyDown(KeyCode.LeftArrow) ? KeyCode.LeftArrow : KeyCode.RightArrow;
            float timeSinceLast = Time.time - lastKeyTime;

            if (currentKey != lastKey && timeSinceLast >= minRhythmTime && timeSinceLast <= maxRhythmTime)
            {
                // ✅ Good timing
                currentSpeed += goodRhythmBoost;
                currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);
            }
            else
            {
                // ❌ Bad timing or same key twice
                currentSpeed -= badRhythmPenalty;
                if (currentSpeed < 0f) currentSpeed = 0f;
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
        // lose speed if idle too long
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
                // 💥 Hit obstacle = lose speed
                currentSpeed = Mathf.Max(currentSpeed - 3f, 0f);
                Debug.Log("💥 Hit obstacle! Slowed down!");
            }
            else
            {
                Debug.Log("✅ Dove under obstacle safely!");
            }
        }

        if (other.CompareTag("Finish"))
        {
            Debug.Log("🏁 You win!");
            rb.linearVelocity = Vector2.zero;
            currentSpeed = 0;
        }
    }

    // ------------------------------
    // UI GETTERS
    // ------------------------------
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public float GetLastKeyTime()
    {
        return lastKeyTime;
    }
}
