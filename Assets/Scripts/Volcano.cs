using UnityEngine;

public class Volcano : MonoBehaviour
{
    public GameManager gameManager;
    public float lavaSuccessReward = 15f;
    public float lavaFailurePenalty = 5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Native native = other.GetComponent<Native>();

        // Only proceed if it's a Native AND it's NOT currently being grabbed
        if (native != null && !native.isGrabbed && gameManager != null)
        {
            if (native.maskType == gameManager.activeGod.targetMaskType)
            {
                gameManager.DecreaseLava(lavaSuccessReward);
                Debug.Log("Sacrifice Accepted!");
            }
            else
            {
                gameManager.currentLava += lavaFailurePenalty;
                Debug.Log("Wrong Mask Sacrifice!");
            }

            Destroy(other.gameObject);
        }
    }
}