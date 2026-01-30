using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("Detection Settings")]
    public LayerMask targetLayer; 
    public Transform grabPoint; 
    [Range(0.1f, 5f)] 
    public float grabRadius = 0.5f;

    [Header("Animation Triggers")]
    public string grabAnimationTrigger = "isGrabbed";
    public string releaseAnimationTrigger = "isReleased";

    [Header("Scaling Settings")]
    [Tooltip("The MAX scale the Object can reach")]
    public float maxObjectScale = 5.0f; 
    [Tooltip("The MAX scale the Hand can reach when Object is at max")]
    public float maxHandScale = 3.0f; 
    [Tooltip("How fast the zoom happens")]
    public float scrollSpeed = 0.2f;

    [Header("Live State (Read Only)")]
    [Range(0, 1)]
    public float growthProgress = 0f; // 0 = Original size, 1 = Max size

    private Transform grabbedObject = null;
    private Animator grabbedAnimator = null;
    private bool isHolding = false;
    
    private Vector3 originalHandScale;
    private Vector3 originalObjectScale;

    void Start()
    {
        originalHandScale = transform.localScale;
    }

    void Update()
    {
        MoveHandWithMouse();

        if (Input.GetMouseButtonDown(0))
        {
            if (isHolding) ReleaseObject();
            else TryGrabObject();
        }

        if (isHolding)
        {
            HandleProportionalScaling();
        }
    }

    void MoveHandWithMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; 
        transform.position = mousePos;
    }

    void HandleProportionalScaling()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) < 0.001f) return;

        // 1. Update the progress percentage (0 to 1)
        growthProgress += scrollInput * scrollSpeed;
        growthProgress = Mathf.Clamp01(growthProgress); // Strictly keep between 0 and 1

        // 2. Calculate Hand Scale based on progress
        // Formula: StartScale + (Progress * (MaxScale - StartScale))
        float handTargetMultiplier = 1f + (growthProgress * (maxHandScale - 1f));
        transform.localScale = originalHandScale * handTargetMultiplier;

        // 3. Calculate Object Scale based on same progress
        if (grabbedObject != null)
        {
            float objectTargetMultiplier = 1f + (growthProgress * (maxObjectScale - 1f));
            grabbedObject.localScale = originalObjectScale * objectTargetMultiplier;
        }
    }

    void TryGrabObject()
    {
        if (grabPoint == null) return;

        Collider2D hitCollider = Physics2D.OverlapCircle(grabPoint.position, grabRadius, targetLayer);

        if (hitCollider != null)
        {
            grabbedObject = hitCollider.transform;
            Native nativeScript = grabbedObject.GetComponent<Native>();
            if (nativeScript != null) nativeScript.isGrabbed = true;
            isHolding = true;
            growthProgress = 0f; // Start at 0% growth
            originalObjectScale = grabbedObject.localScale;

            grabbedObject.SetParent(grabPoint);
            grabbedObject.localPosition = Vector3.zero; 

            grabbedAnimator = grabbedObject.GetComponent<Animator>();
            if (grabbedAnimator != null)
                grabbedAnimator.SetTrigger(grabAnimationTrigger);

            Rigidbody2D rb = grabbedObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic; 
                rb.linearVelocity = Vector2.zero; 
            }
        }
    }

    void ReleaseObject()
    {
        if (grabbedObject != null)
        {
            Native nativeScript = grabbedObject.GetComponent<Native>();
            if (nativeScript != null) nativeScript.isGrabbed = false;
            grabbedObject.SetParent(null);
            grabbedObject.localScale = originalObjectScale;

            if (grabbedAnimator != null)
                grabbedAnimator.SetTrigger(releaseAnimationTrigger);

            Rigidbody2D rb = grabbedObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 1f; 
            }

            grabbedObject = null;
            grabbedAnimator = null;
        }

        // Reset
        growthProgress = 0f;
        transform.localScale = originalHandScale;
        isHolding = false;
    }

    private void OnDrawGizmos()
    {
        if (grabPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(grabPoint.position, grabRadius);
        }
    }
}