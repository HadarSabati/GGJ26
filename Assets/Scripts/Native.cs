using UnityEngine;
using System.Collections;

public class Native : MonoBehaviour
{
    [Header("Visuals")]
    public Sprite maskSprite;
    public string maskType;

    [Header("The Area Object")]
    public GameObject walkingArea;

    [Header("Movement Settings")]
    public float minSpeed = 2f;
    public float maxSpeed = 5f;
    public float minWaitTime = 0.2f;
    public float maxWaitTime = 1f;

    [Header("Landing Settings")]
    public float groundYLevel = -3.5f; // The Y level of your island
    public bool isGrabbed = false;

    private Collider2D actualCollider;
    private Vector2 targetPosition;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private float currentSpeed;
    private bool isFalling = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody

        if (maskSprite != null && sr != null) sr.sprite = maskSprite;

        if (walkingArea != null)
        {
            actualCollider = walkingArea.GetComponent<Collider2D>() ?? walkingArea.GetComponentInChildren<Collider2D>();
            if (actualCollider != null) StartCoroutine(MovementRoutine());
        }
    }

    void Update()
    {
        // Check if the native has landed after being released
        if (!isGrabbed && rb != null && rb.gravityScale > 0)
        {
            if (transform.position.y <= groundYLevel)
            {
                Land();
            }
        }
    }

    void Land()
    {
        rb.gravityScale = 0; // Stop falling
        rb.linearVelocity = Vector2.zero; // Stop any residual fall speed

        // Snap to the exact ground level so they don't sink
        transform.position = new Vector3(transform.position.x, groundYLevel, transform.position.z);

        Debug.Log("Native has landed and resumed movement.");
    }

    IEnumerator MovementRoutine()
    {
        while (true)
        {
            // If grabbed or currently falling, wait and do nothing
            if (isGrabbed || (rb != null && rb.gravityScale > 0))
            {
                yield return null;
                continue;
            }

            SetNewRandomTarget();
            currentSpeed = Random.Range(minSpeed, maxSpeed);

            while (Vector2.Distance(transform.position, targetPosition) > 0.2f)
            {
                // Break if grabbed during movement
                if (isGrabbed || (rb != null && rb.gravityScale > 0)) break;

                transform.position = Vector2.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);

                if (sr != null) sr.flipX = targetPosition.x < transform.position.x;

                yield return null;
            }

            if (!isGrabbed && (rb == null || rb.gravityScale == 0))
            {
                yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));
            }
        }
    }

    void SetNewRandomTarget()
    {
        if (actualCollider == null) return;
        Vector2 potentialPoint = transform.position;
        int attempts = 0;
        do
        {
            float x = Random.Range(actualCollider.bounds.min.x, actualCollider.bounds.max.x);
            float y = Random.Range(actualCollider.bounds.min.y, actualCollider.bounds.max.y);
            potentialPoint = new Vector2(x, y);
            attempts++;
        } while (!actualCollider.OverlapPoint(potentialPoint) && attempts < 30);
        targetPosition = potentialPoint;
    }
}