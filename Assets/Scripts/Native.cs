using UnityEngine;
using System.Collections;

public class Native : MonoBehaviour
{
    [Header("Visuals")]
    public string maskType;
    public Transform legsPoint;

    [Header("The Area Object")]
    public GameObject walkingArea;

    [Header("Movement Settings")]
    public float minSpeed = 2f;
    public float maxSpeed = 5f;
    public float minWaitTime = 0.2f;
    public float maxWaitTime = 1f;

    [Header("Landing Settings")]
    public float groundYLevel = -4f;
    public bool isGrabbed = false;
    public int fallingSortingOrder = 3;

    private Collider2D actualCollider;
    private Vector2 targetPosition;
    private Rigidbody2D rb;
    private float currentSpeed;
    private SpriteRenderer[] allRenderers;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        allRenderers = GetComponentsInChildren<SpriteRenderer>();

        if (walkingArea != null)
        {
            actualCollider = walkingArea.GetComponent<Collider2D>() ?? walkingArea.GetComponentInChildren<Collider2D>();
            if (actualCollider != null)
            {
                // Give it a moment to initialize then teleport
                TeleportToRandomValidPoint();
                StartCoroutine(MovementRoutine());
            }
        }
    }

    void Update()
    {
        UpdateSortingOrder();

        if (!isGrabbed && rb != null && rb.gravityScale > 0)
        {
            if (transform.position.y <= groundYLevel)
            {
                Land();
            }
        }
    }

    void UpdateSortingOrder()
    {
        int finalOrder;

        if (isGrabbed)
        {
            finalOrder = 10000; 
        }
        else if (rb != null && rb.gravityScale > 0)
        {
            finalOrder = fallingSortingOrder; 
        }
        else if (legsPoint != null)
        {
            finalOrder = (int)(legsPoint.position.y * -100);
        }
        else
        {
            finalOrder = (int)(transform.position.y * -100);
        }

        foreach (SpriteRenderer s in allRenderers)
        {
            s.sortingOrder = finalOrder;
            if (s.gameObject.name == "mask") s.sortingOrder += 1;
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

                // Use ONLY your green polygon for pathfinding
                if (actualCollider.OverlapPoint(nextStep))
                {
                    transform.position = nextStep;
                }
                else
                {
                    break;
                }

                foreach (var sr in allRenderers) sr.flipX = targetPosition.x < transform.position.x;
                yield return null;
            }
            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));
        }
    }

    void TeleportToRandomValidPoint()
    {
        Vector2 safePoint = FindSafePoint();
        transform.position = new Vector3(safePoint.x, groundYLevel, transform.position.z);
    }

    void SetNewRandomTarget() => targetPosition = FindSafePoint();

    Vector2 FindSafePoint()
    {
        if (actualCollider == null) return transform.position;

        for (int i = 0; i < 500; i++)
        {
            float x = Random.Range(actualCollider.bounds.min.x, actualCollider.bounds.max.x);
            float y = Random.Range(actualCollider.bounds.min.y, actualCollider.bounds.max.y);
            Vector2 potentialPoint = new Vector2(x, y);

            if (actualCollider.OverlapPoint(potentialPoint))
            {
                return potentialPoint;
            }
        }
        // Fallback to a corner of the collider bounds if loop fails
        return new Vector2(actualCollider.bounds.min.x + 0.5f, groundYLevel);
    }

    void Land()
    {
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;
        transform.position = new Vector3(transform.position.x, groundYLevel, transform.position.z);
    }
}