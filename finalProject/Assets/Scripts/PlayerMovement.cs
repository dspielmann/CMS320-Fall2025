using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Speed Settings")]
    public float baseSpeed = 0f;          // idle speed
    public float goodRhythmBoost = 2f;    // speed added on perfect rhythm
    public float badRhythmPenalty = 1f;   // slow down on poor timing
    public float maxSpeed = 8f;           // clamp max
    public float slowDecay = 2f;          // natural slow down when idle

    [Header("Rhythm Settings")]
    public float minRhythmTime = 0.75f;   // fastest allowed interval
    public float maxRhythmTime = 1.25f;   // slowest allowed interval

    private Rigidbody2D rb;
    private float currentSpeed = 0f;
    private float lastKeyTime = -10f;
    private KeyCode lastKey = KeyCode.None;
    private bool isDucking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // --- Ducking ---
        if (Input.GetKey(KeyCode.DownArrow))
        {
            isDucking = true;
            return; // pause rhythm input while ducking
        }
        else
        {
            isDucking = false;
        }

        // --- Rhythm input ---
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            KeyCode currentKey = Input.GetKeyDown(KeyCode.LeftArrow) ? KeyCode.LeftArrow : KeyCode.RightArrow;
            float timeSinceLast = Time.time - lastKeyTime;

            if (currentKey != lastKey && timeSinceLast >= minRhythmTime && timeSinceLast <= maxRhythmTime)
            {
                // ✅ Perfect rhythm hit: speed up a bit
                currentSpeed += goodRhythmBoost;
                currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);
            }
            else
            {
                // ❌ Off-beat or same key: small slowdown
                currentSpeed -= badRhythmPenalty;
                if (currentSpeed < 0f) currentSpeed = 0f;
            }

            lastKey = currentKey;
            lastKeyTime = Time.time;
        }

        // --- Natural slowdown if no input ---
        if (Time.time - lastKeyTime > maxRhythmTime)
        {
            currentSpeed -= slowDecay * Time.deltaTime;
            if (currentSpeed < 0f) currentSpeed = 0f;
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(currentSpeed, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            if (!isDucking)
            {
                currentSpeed = Mathf.Max(currentSpeed - 3f, 0f);
                Debug.Log("Hit obstacle! Slow down!");
            }
            else
            {
                Debug.Log("Ducked under obstacle!");
            }
        }

        if (other.CompareTag("Finish"))
        {
            Debug.Log("🏁 You win!");
            rb.linearVelocity = Vector2.zero;
            currentSpeed = 0;
        }
    }

        // --- Expose current speed and timing for UI ---
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public float GetLastKeyTime()
    {
        return lastKeyTime;
    }

}
