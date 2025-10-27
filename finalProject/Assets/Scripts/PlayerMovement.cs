using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Speed Settings")]
    public float baseSpeed = 0f;       // no movement when idle
    public float rhythmBoost = 2f;     // how much speed to add on a good rhythm
    public float maxSpeed = 8f;        // clamp the top speed
    public float slowPenalty = 1.5f;   // reduce speed when out of rhythm
    public float rhythmWindow = 1f;    // time between alternating key presses

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
            return;
        }
        else
        {
            isDucking = false;
        }

        // --- Rhythm movement ---
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            KeyCode currentKey = Input.GetKeyDown(KeyCode.LeftArrow) ? KeyCode.LeftArrow : KeyCode.RightArrow;
            float timeSinceLast = Time.time - lastKeyTime;

            if (currentKey != lastKey && timeSinceLast <= rhythmWindow)
            {
                // Good rhythm — increase speed
                currentSpeed += rhythmBoost;
                currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);
            }
            else
            {
                // Bad rhythm or same key twice — slow down
                currentSpeed -= slowPenalty;
                if (currentSpeed < 0f) currentSpeed = 0f;
            }

            lastKey = currentKey;
            lastKeyTime = Time.time;
        }

        // Natural slow decay over time (so player eventually slows if idle)
        if (currentSpeed > 0f)
        {
            currentSpeed -= Time.deltaTime * 0.5f;
            if (currentSpeed < 0f) currentSpeed = 0f;
        }
    }

    void FixedUpdate()
    {
        // Only move when speed > 0
        rb.linearVelocity = new Vector2(currentSpeed, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            if (!isDucking)
            {
                currentSpeed = 0f;
                Debug.Log("Hit obstacle! Slowed down!");
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
}
