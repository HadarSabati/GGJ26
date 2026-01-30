using UnityEngine;

public class CameraEdgeScroll : MonoBehaviour
{
    [Header("Movement Settings")]
    public float scrollSpeed = 15f;
    public float smoothTime = 0.15f;
    public float edgeThreshold = 50f;

    [Header("Boundaries (Pixels)")]
    public float backgroundWidth = 2500f;
    public float screenWidth = 1920f;
    public float pixelsPerUnit = 100f;

    private float _targetX;
    private float _currentVelocity;
    private float _minX;
    private float _maxX;

    void Start()
    {
        // Calculate the maximum movement range in Unity Units
        // Assuming the background starts at X=0
        float totalMovementRange = (backgroundWidth - screenWidth) / pixelsPerUnit;
        
        _minX = 0f; 
        _maxX = totalMovementRange;

        // Set initial target to current camera position
        _targetX = transform.position.x;
    }

    // LateUpdate is called after all Update functions. 
    // This prevents jittering when the HandController moves in Update.
    void LateUpdate() 
    {
        HandleInput();
        ApplyMovement();
    }

    /// <summary>
    /// Checks if the mouse is near the screen edges and updates the target X position.
    /// </summary>
    void HandleInput()
    {
        float mouseX = Input.mousePosition.x;

        // Move target right
        if (mouseX >= Screen.width - edgeThreshold)
        {
            _targetX += scrollSpeed * Time.deltaTime;
        }
        // Move target left
        else if (mouseX <= edgeThreshold)
        {
            _targetX -= scrollSpeed * Time.deltaTime;
        }

        // Clamp the target position so the camera never sees past the background edges
        _targetX = Mathf.Clamp(_targetX, _minX, _maxX);
    }

    /// <summary>
    /// Smoothly interpolates the camera's position towards the target X.
    /// </summary>
    void ApplyMovement()
    {
        // SmoothDamp provides a more natural "weighted" movement than a simple Lerp
        float newX = Mathf.SmoothDamp(transform.position.x, _targetX, ref _currentVelocity, smoothTime);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }
}