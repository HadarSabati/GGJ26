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

    [Header("Spawn Settings")]
    public Transform spawnPointObject;

    private Collider2D actualCollider;
    private Vector2 targetPosition;
    private Rigidbody2D rb;
    private float currentSpeed;
    private SpriteRenderer[] allRenderers;
    private bool isWalking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        allRenderers = GetComponentsInChildren<SpriteRenderer>();

        if (walkingArea != null)
        {
            actualCollider = walkingArea.GetComponent<Collider2D>() ?? walkingArea.GetComponentInChildren<Collider2D>();

            if (spawnPointObject != null)
            {
                transform.position = new Vector3(spawnPointObject.position.x, spawnPointObject.position.y, transform.position.z);
            }

            if (actualCollider != null)
            {
                SetNewRandomTarget();
                currentSpeed = Random.Range(minSpeed, maxSpeed);
                isWalking = true;

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
        if (isGrabbed) finalOrder = 10000;
        else if (rb != null && rb.gravityScale > 0) finalOrder = fallingSortingOrder;
        else if (legsPoint != null) finalOrder = (int)(legsPoint.position.y * -100);
        else finalOrder = (int)(transform.position.y * -100);

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
                isWalking = false;
                yield return null;
                continue;
            }

            isWalking = true;

            while (Vector2.Distance(transform.position, targetPosition) > 0.15f)
            {
                if (isGrabbed || (rb != null && rb.gravityScale > 0)) break;

                Vector3 nextStep = Vector2.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);
                Vector2 checkPos = legsPoint != null ? (Vector2)legsPoint.position + (Vector2)(nextStep - transform.position) : (Vector2)nextStep;

                if (actualCollider.OverlapPoint(checkPos))
                {
                    transform.position = nextStep;
                }
                else break;

                foreach (var sr in allRenderers) sr.flipX = targetPosition.x < transform.position.x;
                yield return null;
            }

            isWalking = false;

            if (!isGrabbed && (rb == null || rb.gravityScale == 0))
            {
                yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));

                SetNewRandomTarget();
                currentSpeed = Random.Range(minSpeed, maxSpeed);
            }
        }
    }

    void SetNewRandomTarget() => targetPosition = FindSafePoint();

    Vector2 FindSafePoint()
    {
        if (actualCollider == null) return transform.position;
        for (int i = 0; i < 200; i++)
        {
            float x = Random.Range(actualCollider.bounds.min.x, actualCollider.bounds.max.x);
            float y = Random.Range(actualCollider.bounds.min.y, actualCollider.bounds.max.y);
            Vector2 potentialPoint = new Vector2(x, y);
            if (actualCollider.OverlapPoint(potentialPoint)) return potentialPoint;
        }
        return transform.position;
    }

    void Land()
    {
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;
        transform.position = new Vector3(transform.position.x, groundYLevel, transform.position.z);
    }
}