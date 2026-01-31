using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("Detection Settings")]
    public LayerMask targetLayer;
    public Transform grabPoint;
    [Range(0.1f, 5f)]
    public float grabRadius = 0.5f;

    [Header("Hand Animation (The Tool)")]
    [Tooltip("Drag children here: Body and Mask")]
    public Animator[] handAnimators;
    public string grabAnimationTrigger = "isGrabbed";
    public string releaseAnimationTrigger = "isReleased";

    [Header("Scaling Settings")]
    public float maxObjectScale = 5.0f;
    public float maxHandScale = 3.0f;
    public float scrollSpeed = 0.2f;

    [Header("Live State (Read Only)")]
    [Range(0, 1)]
    public float growthProgress = 0f;

    private Transform grabbedObject = null;
    private Animator[] grabbedObjectAnimators = null;
    private bool isHolding = false;
    
    // We store World Scale now, not Local
    private Vector3 originalHandScale;
    private Vector3 originalObjectScale; 
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        originalHandScale = transform.localScale;
        
        if (handAnimators == null || handAnimators.Length == 0)
        {
            handAnimators = GetComponentsInChildren<Animator>();
        }
    }

    void Update()
    {
        MoveHandWithMouse();

        // 1. NEW: Force the object to follow the hand position without being a child
        if (isHolding && grabbedObject != null && grabPoint != null)
        {
            grabbedObject.position = grabPoint.position;
        }

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

    private void MoveHandWithMouse()
    {
        if (mainCam == null) return;
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; 
        transform.position = mousePos;
    }

    private void TryGrabObject()
    {
        if (grabPoint == null) return;

        Collider2D hitCollider = Physics2D.OverlapCircle(grabPoint.position, grabRadius, targetLayer);

        if (hitCollider != null)
        {
            grabbedObject = hitCollider.transform;
            isHolding = true;
            growthProgress = 0f; 

            // Update Native Script
            Native nativeScript = grabbedObject.GetComponent<Native>();
            if (nativeScript != null) nativeScript.isGrabbed = true;

            // 2. FIX: Save the object's current size (World Scale/LossyScale is safer, but local works if it has no parent)
            originalObjectScale = grabbedObject.localScale; 
            
            // 3. FIX: REMOVED SetParent. 
            // We do NOT parent the object. We simply move it in Update.
            // grabbedObject.SetParent(grabPoint); <--- DELETED THIS LINE

            PlayAnimatorsSafely(handAnimators, grabAnimationTrigger);

            grabbedObjectAnimators = grabbedObject.GetComponentsInChildren<Animator>();
            PlayAnimatorsSafely(grabbedObjectAnimators, grabAnimationTrigger);

            Rigidbody2D rb = grabbedObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic; 
                rb.linearVelocity = Vector2.zero; 
                rb.angularVelocity = 0f; // Stop rotation too
            }
        }
    }

    private void ReleaseObject()
    {
        if (grabbedObject != null)
        {
            Native nativeScript = grabbedObject.GetComponent<Native>();
            if (nativeScript != null) nativeScript.isGrabbed = false;

            // 1. FIX: REMOVED SetParent(null) because we never parented it.
            // Just restore scale.
            grabbedObject.localScale = originalObjectScale;

            PlayAnimatorsSafely(handAnimators, releaseAnimationTrigger);
            PlayAnimatorsSafely(grabbedObjectAnimators, releaseAnimationTrigger);

            Rigidbody2D rb = grabbedObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 1f; 
            }

            grabbedObject = null;
            grabbedObjectAnimators = null;
        }

        growthProgress = 0f;
        transform.localScale = originalHandScale;
        isHolding = false;
    }

    private void HandleProportionalScaling()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        
        if (Mathf.Abs(scrollInput) > 0.001f)
        {
            growthProgress += scrollInput * scrollSpeed;
            growthProgress = Mathf.Clamp01(growthProgress);

            // Scale the Hand
            float handFactor = 1f + (growthProgress * (maxHandScale - 1f));
            transform.localScale = originalHandScale * handFactor;

            // Scale the Object
            // FIX: Because they are not parented, this math now works perfectly 
            // without "Double Scaling" or "Flipping".
            if (grabbedObject != null)
            {
                float objectFactor = 1f + (growthProgress * (maxObjectScale - 1f));
                grabbedObject.localScale = originalObjectScale * objectFactor;
            }
        }
    }

    private void PlayAnimatorsSafely(Animator[] anims, string trigger)
    {
        if (anims == null || anims.Length == 0) return;

        foreach (Animator a in anims)
        {
            if (a != null && a.runtimeAnimatorController != null)
            {
                if (HasParameter(trigger, a))
                {
                    a.SetTrigger(trigger);
                }
            }
        }
    }

    private bool HasParameter(string paramName, Animator animator)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
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