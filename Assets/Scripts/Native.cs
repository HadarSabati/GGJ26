using UnityEngine;

public class NativeBehavior : MonoBehaviour
{
    [Header("Visual Settings")]
    public Sprite maskSprite;
    public bool isCorrect;

    [Header("Movement Settings")]
    public float speed = 2f;
    public Collider2D walkingArea;

    private Vector2 targetPosition;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (maskSprite != null && sr != null)
            sr.sprite = maskSprite;

        if (walkingArea != null)
            SetNewRandomTarget();
    }

    void Update()
    {
        if (walkingArea == null) return;

        transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPosition) < 0.2f)
        {
            SetNewRandomTarget();
        }
    }

    void SetNewRandomTarget()
    {
        Vector2 potentialPoint = transform.position;
        int attempts = 0;

        do
        {
            float x = Random.Range(walkingArea.bounds.min.x, walkingArea.bounds.max.x);
            float y = Random.Range(walkingArea.bounds.min.y, walkingArea.bounds.max.y);
            potentialPoint = new Vector2(x, y);
            attempts++;
        }
        while (!walkingArea.OverlapPoint(potentialPoint) && attempts < 30);

        targetPosition = potentialPoint;
    }
}