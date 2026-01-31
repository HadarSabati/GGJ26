using UnityEngine;

public class Volcano : MonoBehaviour
{
    public GameManager gameManager;
    public float lavaSuccessReward = 15f;
    public float lavaFailurePenalty = 5f;
    public Animator smokeAnimator;

    public Animator lavaAnimator;
    public GameObject LostPanel;
    public GameObject WinPanel;

    [Header("Teleport Settings")]
    public Transform[] randomSpawnPoints;

    [Header("Audio Sources")]
    public AudioSource splashSource;
    public AudioSource successSource;
    public AudioSource failureSource;

    void Start()
    {
        if (WinPanel != null) WinPanel.SetActive(false);
        if (LostPanel != null) LostPanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Native native = other.GetComponent<Native>();

        if (native != null && !native.isGrabbed && gameManager != null)
        {
            if (splashSource != null) splashSource.Play();

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

            if (gameManager.currentLava >=60)
            {
                if (lavaAnimator != null) lavaAnimator.SetTrigger("lost");
                if (LostPanel != null) LostPanel.SetActive(true);
                Time.timeScale = 0f; 
            }
            else if (gameManager.currentLava <=0)
            {
                if (lavaAnimator != null) lavaAnimator.SetTrigger("win");
                if (WinPanel != null) WinPanel.SetActive(true);
                Time.timeScale = 0f;
            }

            TeleportToRandomPoint(other.transform);
        }
    }

    private void TeleportToRandomPoint(Transform targetTransform)
    {
        if (randomSpawnPoints != null && randomSpawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, randomSpawnPoints.Length);
            targetTransform.position = randomSpawnPoints[randomIndex].position;

            Rigidbody2D rb = targetTransform.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}