using UnityEngine;
using System.Collections;

public class Native : MonoBehaviour
{
    [Header("Visuals")]
    public string maskType;

    [Header("The Area Object")]
    public GameObject walkingArea;

    [Header("Movement Settings")]
    public float minSpeed = 2f;
    public float maxSpeed = 5f;
    public float minWaitTime = 0.2f;
    public float maxWaitTime = 1f;

    [Header("Landing Settings")]
    public float groundYLevel = -3.5f;
    public bool isGrabbed = false;

    private Collider2D actualCollider;
    private Vector2 targetPosition;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private float currentSpeed;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        if (walkingArea != null)
        {
            actualCollider = walkingArea.GetComponent<Collider2D>() ?? walkingArea.GetComponentInChildren<Collider2D>();

            // If starting outside, snap to a valid position immediately
            if (actualCollider != null && !actualCollider.OverlapPoint(transform.position))
            {
                SnapToValidArea();
            }

            if (actualCollider != null) StartCoroutine(MovementRoutine());
        }
    }

    void Update()
    {
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
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;
        transform.position = new Vector3(transform.position.x, groundYLevel, transform.position.z);

        // Ensure we didn't land in the water/outside polygon
        if (actualCollider != null && !actualCollider.OverlapPoint(transform.position))
        {
            SnapToValidArea();
        }
    }

    IEnumerator MovementRoutine()
    {
        while (true)
        {
            if (isGrabbed || (rb != null && rb.gravityScale > 0))
            {
                yield return null;
                continue;
            }

            SetNewRandomTarget();
            currentSpeed = Random.Range(minSpeed, maxSpeed);

            while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
            {
                if (isGrabbed || (rb != null && rb.gravityScale > 0)) break;

                Vector3 nextStep = Vector2.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);

                // --- STICK TO AREA LOGIC ---
                // Only move if the next step is still inside the polygon
                if (actualCollider.OverlapPoint(nextStep))
                {
                    transform.position = nextStep;
                }
                else
                {
                    // If the path tries to leave the area, stop and pick a new target
                    break;
                }

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

        Vector2 potentialPoint;
        int attempts = 0;

        do
        {
            float x = Random.Range(actualCollider.bounds.min.x, actualCollider.bounds.max.x);
            float y = Random.Range(actualCollider.bounds.min.y, actualCollider.bounds.max.y);
            potentialPoint = new Vector2(x, y);
            attempts++;
        } while (!actualCollider.OverlapPoint(potentialPoint) && attempts < 100);

        targetPosition = potentialPoint;
    }

    void SnapToValidArea()
    {
        if (actualCollider == null) return;
        // Move to the closest point inside the collider bounds
        transform.position = new Vector3(actualCollider.bounds.center.x, groundYLevel, transform.position.z);
    }
}