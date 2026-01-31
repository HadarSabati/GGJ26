using UnityEngine;

public class Volcano : MonoBehaviour
{
    public GameManager gameManager;
    public float lavaSuccessReward = 15f;
    public float lavaFailurePenalty = 5f;

    [Header("Audio Sources")]
    // This plays every time a native hits the lava
    public AudioSource splashSource; 
    // This plays only on correct sacrifice
    public AudioSource successSource; 
    // This plays only on wrong sacrifice
    public AudioSource failureSource; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        Native native = other.GetComponent<Native>();

        // Check if the object is a Native and is not being held by the player
        if (native != null && !native.isGrabbed && gameManager != null)
        {
            // 1. Always play the splash sound
            if (splashSource != null) splashSource.Play();

            // 2. Check for mask match
            if (native.maskType == gameManager.activeGod.targetMaskType)
            {
                // Play success sound in addition to splash
                if (successSource != null) successSource.Play();
                
                gameManager.DecreaseLava(lavaSuccessReward);
                Debug.Log("Sacrifice Accepted!");
            }
            else
            {
                // Play failure sound in addition to splash
                if (failureSource != null) failureSource.Play();
                
                gameManager.currentLava += lavaFailurePenalty;
                Debug.Log("Wrong Mask Sacrifice!");
            }

            // Remove the native from the game
            Destroy(other.gameObject);
        }
    }
}