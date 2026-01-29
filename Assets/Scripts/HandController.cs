using UnityEngine;

public class HandController : MonoBehaviour
{
    // Assign "Natives" layer in the Inspector
    public LayerMask targetLayer; 
    // Drag your "GrabPoint" child object here
    public Transform grabPoint; 

    private Transform grabbedObject = null;
    private bool isHolding = false;

    void Update()
    {
        MoveHandWithMouse();

        if (Input.GetMouseButtonDown(0))
        {
            if (isHolding)
                ReleaseObject();
            else
                TryGrabObject();
        }

        // Attach the object to the grab point while holding
        if (isHolding && grabbedObject != null && grabPoint != null)
        {
            grabbedObject.position = grabPoint.position;
        }
    }

    void MoveHandWithMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; 
        transform.position = mousePos;
    }

    void TryGrabObject()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 0f, targetLayer);

        if (hit.collider != null)
        {
            Debug.Log("<color=cyan>Hand hit:</color> " + hit.collider.name);
            
            grabbedObject = hit.transform;
            isHolding = true;
            
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
            Debug.Log("<color=yellow>Hand released:</color> " + grabbedObject.name);
            
            Rigidbody2D rb = grabbedObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                
                // Change Gravity Scale upon release
                // Set this value to whatever you need (e.g., 1f for normal, 2f for heavy)
                rb.gravityScale = 1f; 
            }

            grabbedObject = null;
        }
        isHolding = false;
    }
}