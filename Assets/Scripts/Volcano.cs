using UnityEngine;

public class Volcano : MonoBehaviour
{
    public GameManager gameManager;
    public float lavaSuccessReward = 15f;
    public float lavaFailurePenalty = 5f;
    public Animator smokeAnimator;

    [Header("Teleport Settings")]
    // Drag your two empty GameObjects (spawn points) here in the inspector
    public Transform[] randomSpawnPoints; 

    [Header("Audio Sources")]
    public AudioSource splashSource; 
    public AudioSource successSource; 
    public AudioSource failureSource; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        Native native = other.GetComponent<Native>();

        if (native != null && !native.isGrabbed && gameManager != null)
        {
            // 1. Always play the splash sound
            if (splashSource != null) splashSource.Play();

            // 2. Logic for mask match
            if (native.maskType == gameManager.activeGod.targetMaskType)
            {
                if (successSource != null) successSource.Play();

                smokeAnimator.SetTrigger("ActivateSmoke");

                gameManager.DecreaseLava(lavaSuccessReward);
                Debug.Log("Sacrifice Accepted!");
            }
            else
            {
                if (failureSource != null) failureSource.Play();


                smokeAnimator.SetTrigger("ActivateSmokeSkull");

                gameManager.currentLava += lavaFailurePenalty;
                Debug.Log("Wrong Mask Sacrifice!");
            }

            // 3. Teleport the object instead of destroying it
            TeleportToRandomPoint(other.transform);
        }
    }

    private void TeleportToRandomPoint(Transform targetTransform)
    {
        if (randomSpawnPoints != null && randomSpawnPoints.Length > 0)
        {
            // Pick a random index (0 or 1 if you provided two points)
            int randomIndex = Random.Range(0, randomSpawnPoints.Length);
            
            // Move the object to the chosen point
            targetTransform.position = randomSpawnPoints[randomIndex].position;

            // Optional: Reset physics velocity so they don't keep falling/moving
            Rigidbody2D rb = targetTransform.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            Debug.LogWarning("No spawn points assigned in the Volcano script!");
        }
    }
}