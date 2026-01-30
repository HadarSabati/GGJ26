using UnityEngine;
using System.Collections;

public class Native : MonoBehaviour
{
    [Header("Visuals")]
    public Sprite maskSprite;
    public string maskType;

    [Header("The Area Object")]
    public GameObject walkingArea; 

    [Header("Organic Movement Settings")]
    public float minSpeed = 2f;
    public float maxSpeed = 5f;
    public float minWaitTime = 0.2f;
    public float maxWaitTime = 1f;

    private Collider2D actualCollider;
    private Vector2 targetPosition;
    private SpriteRenderer sr;
    private float currentSpeed;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (maskSprite != null && sr != null) sr.sprite = maskSprite;

        if (walkingArea != null)
        {
            actualCollider = walkingArea.GetComponent<Collider2D>() ?? walkingArea.GetComponentInChildren<Collider2D>();
            
            if (actualCollider != null) 
            {
                StartCoroutine(MovementRoutine());
            }
        }
    }

    IEnumerator MovementRoutine()
    {
        while (true)
        {
            SetNewRandomTarget();
            currentSpeed = Random.Range(minSpeed, maxSpeed);

            while (Vector2.Distance(transform.position, targetPosition) > 0.2f)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);
                
                if (targetPosition.x > transform.position.x) sr.flipX = false;
                else sr.flipX = true;

                yield return null;
            }

            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);
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
        } 
        while (!actualCollider.OverlapPoint(potentialPoint) && attempts < 30);

        targetPosition = potentialPoint;
    }
}