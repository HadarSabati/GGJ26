using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("Detection Settings")]
    public LayerMask targetLayer; 
    public Transform grabPoint; 
    [Range(0.1f, 5f)] 
    public float grabRadius = 0.5f;

    [Header("Hand Animation (The Tool)")]
    [Tooltip("גרור לכאן את הילדים: גוף ומסכה")]
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
    private Vector3 originalHandScale;
    private Vector3 originalObjectScale;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        originalHandScale = transform.localScale;
        
        // אם לא גררת ידנית, הסקריפט ימצא את כל האנימטורים שמתחת לאובייקט הזה
        if (handAnimators == null || handAnimators.Length == 0)
        {
            handAnimators = GetComponentsInChildren<Animator>();
            Debug.Log($"<color=cyan>HandController:</color> Found {handAnimators.Length} animators in children.");
        }
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
            growthProgress = 0f; // איפוס הגדילה בתפיסה חדשה

            // 1. עדכון סקריפט ה-Native של האובייקט
            Native nativeScript = grabbedObject.GetComponent<Native>();
            if (nativeScript != null) nativeScript.isGrabbed = true;

            // 2. שמירת הגודל המקורי לפני ההצמדה
            originalObjectScale = grabbedObject.localScale;
            
            // 3. הצמדה פיזית ליד
            grabbedObject.SetParent(grabPoint);
            grabbedObject.localPosition = Vector3.zero; 

            // 4. הפעלת אנימציות היד (גוף + מסכה)
            PlayAnimatorsSafely(handAnimators, grabAnimationTrigger);

            // 5. סריקת כל האנימטורים בתוך ה-Prefab שנתפס והפעלתם
            grabbedObjectAnimators = grabbedObject.GetComponentsInChildren<Animator>();
            PlayAnimatorsSafely(grabbedObjectAnimators, grabAnimationTrigger);

            // 6. ניטרול פיזיקה כדי שלא יווצרו "קפיצות"
            Rigidbody2D rb = grabbedObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic; 
                rb.linearVelocity = Vector2.zero; 
            }
        }
    }

    private void ReleaseObject()
    {
        if (grabbedObject != null)
        {
            // 1. שחרור סקריפט ה-Native
            Native nativeScript = grabbedObject.GetComponent<Native>();
            if (nativeScript != null) nativeScript.isGrabbed = false;

            // 2. ניתוק מהיד והחזרת הגודל המקורי
            grabbedObject.SetParent(null);
            grabbedObject.localScale = originalObjectScale;

            // 3. הפעלת אנימציות שחרור ליד ולחפץ
            PlayAnimatorsSafely(handAnimators, releaseAnimationTrigger);
            PlayAnimatorsSafely(grabbedObjectAnimators, releaseAnimationTrigger);

            // 4. החזרת פיזיקה דינמית
            Rigidbody2D rb = grabbedObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 1f; 
            }

            grabbedObject = null;
            grabbedObjectAnimators = null;
        }

        // 5. איפוס היד למצבה המקורי
        growthProgress = 0f;
        transform.localScale = originalHandScale;
        isHolding = false;
    }

    private void HandleProportionalScaling()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        
        // ביצוע שינוי רק אם יש תנועה בגלגלת
        if (Mathf.Abs(scrollInput) > 0.001f)
        {
            growthProgress += scrollInput * scrollSpeed;
            growthProgress = Mathf.Clamp01(growthProgress);

            // עדכון גודל היד
            float handFactor = 1f + (growthProgress * (maxHandScale - 1f));
            transform.localScale = originalHandScale * handFactor;

            // עדכון גודל האובייקט
            if (grabbedObject != null)
            {
                float objectFactor = 1f + (growthProgress * (maxObjectScale - 1f));
                grabbedObject.localScale = originalObjectScale * objectFactor;
            }
        }
    }

    // פונקציה חכמה שבודקת קיום פרמטרים לפני הפעלה
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

    // בדיקה טכנית אם הטריגר קיים ב-Controller
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