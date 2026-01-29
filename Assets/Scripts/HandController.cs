using UnityEngine;

public class HandController : MonoBehaviour
{
    // Assign the "Natives" layer in the Inspector
    public LayerMask targetLayer; 
    
    private Transform grabbedObject = null;
    private bool isHolding = false;

    void Update()
    {
        // 1. Hand movement logic
        MoveHandWithMouse();

        // 2. Interaction logic (Toggle Grab/Release)
        if (Input.GetMouseButtonDown(0))
        {
            if (isHolding)
            {
                ReleaseObject();
            }
            else
            {
                TryGrabObject();
            }
        }

        // 3. Keep the object attached to the hand position while holding
        if (isHolding && grabbedObject != null)
        {
            grabbedObject.position = transform.position;
        }
    }

    void MoveHandWithMouse()
    {
        Vector3 mousePos = Input.mousePosition;
        // Adjust this value based on your camera distance
        mousePos.z = 10f; 
        transform.position = Camera.main.ScreenToWorldPoint(mousePos);
    }

    void TryGrabObject()
    {
        // Cast a ray from the camera to the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Check if the ray hits an object on the "Natives" layer
        if (Physics.Raycast(ray, out hit, 100f, targetLayer))
        {
            grabbedObject = hit.transform;
            isHolding = true;
            
            // Disable physics influence while carrying the object
            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }
    }

    void ReleaseObject()
    {
        if (grabbedObject != null)
        {
            // Re-enable physics so the object can fall
            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            grabbedObject = null;
        }
        isHolding = false;
    }
}