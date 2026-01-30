using UnityEngine;

public class CameraEdgeScroll : MonoBehaviour
{
    public SpriteRenderer backgroundSprite; 
    public float scrollSpeed = 15f;
    public float smoothTime = 0.15f;
    public float edgeThreshold = 50f;

    private float _targetX;
    private float _currentVelocity;
    private float _minX;
    private float _maxX;

    void Start()
    {
        if (backgroundSprite == null) return;
        
        // נחשב את הגבולות פעם אחת ב-Start
        UpdateBoundaries();
        _targetX = transform.position.x;
    }

    // יצרתי פונקציה נפרדת כדי שתוכל לקרוא לה אם הרקע זז
    public void UpdateBoundaries()
    {
        Camera cam = GetComponent<Camera>();
        float camHalfWidth = cam.orthographicSize * cam.aspect;

        // bounds.min ו-bounds.max עובדים ב-World Space.
        // זה לא משנה מי האבא של האובייקט או מה ה-Local Position שלו.
        float bgLeftEdge = backgroundSprite.bounds.min.x;
        float bgRightEdge = backgroundSprite.bounds.max.x;

        _minX = bgLeftEdge + camHalfWidth;
        _maxX = bgRightEdge - camHalfWidth;
    }

    void LateUpdate()
    {
        if (backgroundSprite == null) return;

        // אם האובייקט האבא זז כל הזמן, כדאי לקרוא ל-UpdateBoundaries גם כאן
        // אבל למען הביצועים, עדיף להשאיר את זה ב-Start אם הרקע סטטי
        HandleInput();
        ApplyMovement();
    }

    void HandleInput()
    {
        float mouseX = Input.mousePosition.x;

        if (mouseX >= Screen.width - edgeThreshold)
            _targetX += scrollSpeed * Time.deltaTime;
        else if (mouseX <= edgeThreshold)
            _targetX -= scrollSpeed * Time.deltaTime;

        _targetX = Mathf.Clamp(_targetX, _minX, _maxX);
    }

    void ApplyMovement()
    {
        float newX = Mathf.SmoothDamp(transform.position.x, _targetX, ref _currentVelocity, smoothTime);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }
}