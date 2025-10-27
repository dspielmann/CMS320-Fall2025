using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Speed Settings")]
    public float baseSpeed = 2f;
    public float rhythmBoost = 1f;
    public float maxSpeed = 8f;
    public float slowPenalty = 1.5f;
    public float rhythmWindow = 1f; // seconds between alternating keys

    private Rigidbody2D rb;
    private float currentSpeed;
    private float lastKeyTime = -10f;
    private KeyCode lastKey = KeyCode.None;
    private bool isDucking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = baseSpeed;
    }

    void Update()
    {
        // Ducking logic — temporarily ignore rhythm input
        if (Input.GetKey(KeyCode.DownArrow))
        {
            isDucking = true;
            return; // don't process rhythm while ducking
        }
        else
        {
            isDucking = false;
        }

        // Rhythm-based swimming movement
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            KeyCode currentKey = Input.GetKeyDown(KeyCode.LeftArrow) ? KeyCode.LeftArrow : KeyCode.RightArrow;
            float timeSinceLast = Time.time - lastKeyTime;

            if (currentKey != lastKey && timeSinceLast <= rhythmWindow)
            {
                // Good rhythm — speed up
                currentSpeed += rhythmBoost;
                if (currentSpeed > maxSpeed)
                    currentSpeed = maxSpeed;
            }
            else
            {
                // Bad rhythm or same key twice — slow down
                currentSpeed -= slowPenalty;
                if (currentSpeed < baseSpeed)
                    currentSpeed = baseSpeed;
            }

            lastKey = currentKey;
            lastKeyTime = Time.time;
        }
    }

    void FixedUpdate()
    {
        // Constant forward motion
        rb.linearVelocity = new Vector2(currentSpeed, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            if (!isDucking)
            {
                // Hit an obstacle — slow down
                currentSpeed = baseSpeed;
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
