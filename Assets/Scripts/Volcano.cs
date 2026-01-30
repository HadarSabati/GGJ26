using UnityEngine;

public class Volcano : MonoBehaviour
{
    public GameManager gameManager;
    public float lavaSuccessReward = 15f;
    public float lavaFailurePenalty = 5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Search for the Native script on the colliding object
        Native native = other.GetComponent<Native>();

        if (native != null && gameManager != null)
        {
            // Compare the mask type with the active god's requirement
            if (native.maskType == gameManager.activeGod.targetMaskType)
            {
                gameManager.DecreaseLava(lavaSuccessReward);
                Debug.Log("Sacrifice Accepted: " + native.maskType);
            }
            else
            {
                gameManager.currentLava += lavaFailurePenalty;
                Debug.Log("Sacrifice Rejected: Wrong Mask!");
                Debug.Log("Expected:" + native.maskType);
                Debug.Log("Got:" + gameManager.activeGod.targetMaskType);
            }

            // Destroy the native object immediately
            Destroy(other.gameObject);
        }
    }
}