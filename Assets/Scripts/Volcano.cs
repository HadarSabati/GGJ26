using UnityEngine;
using System.Collections;

public class Volcano : MonoBehaviour
{
    public GameManager gameManager;
    public float lavaSuccessReward = 15f;
    public float lavaFailurePenalty = 5f;
    public Animator smokeAnimator;
    public Animator lavaAnimator;
    public GameObject LostPanel;
    public GameObject WinPanel;

    // משתנה קריטי: מונע מהמשחק לנסות להפסיד/לנצח כמה פעמים במקביל
    private bool isEnding = false;

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
        isEnding = false;
        Time.timeScale = 1f; // ודוא שהזמן רץ בתחילת משחק
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // אם המשחק כבר נגמר (בזמן ההמתנה של ה-5 שניות), אל תעשה כלום
        if (isEnding) return;

        Native native = other.GetComponent<Native>();

        if (native != null && !native.isGrabbed && gameManager != null)
        {
            if (splashSource != null) splashSource.Play();

            // בדיקת התאמה
            if (native.maskType == gameManager.activeGod.targetMaskType)
            {
                if (successSource != null) successSource.Play();
                if (smokeAnimator != null) smokeAnimator.SetTrigger("ActivateSmoke");
                gameManager.DecreaseLava(lavaSuccessReward);
            }
            else
            {
                if (failureSource != null) failureSource.Play();
                if (smokeAnimator != null) smokeAnimator.SetTrigger("ActivateSmokeSkull");
                gameManager.currentLava += lavaFailurePenalty;
            }

            // בדיקת תנאי סיום
            if (gameManager.currentLava >= 60)
            {
                isEnding = true; // נועל את הפונקציה
                StartCoroutine(HandleGameOver());
            }
            else if (gameManager.currentLava <= 0)
            {
                isEnding = true;
                if (lavaAnimator != null) lavaAnimator.SetTrigger("win");
                if (WinPanel != null) WinPanel.SetActive(true);
                Time.timeScale = 0f;
            }

            // שיגור הנייטיב חזרה למעלה
            TeleportToRandomPoint(other.transform);
        }
    }

    IEnumerator HandleGameOver()
    {
        Debug.Log("Game Ending... Playing Animation");

        // 1. הפעלת האנימציה
        if (lavaAnimator != null)
        {
            lavaAnimator.SetTrigger("lost");
        }

        // 2. המתנה בזמן אמת (חשוב!)
        // yield return new WaitForSecondsRealtime(5f); 
        // אם תשתמשי ב-WaitForSeconds רגיל ומישהו יעצור את הזמן, זה לעולם לא יקרה
        yield return new WaitForSeconds(5f);

        // 3. הצגת הפאנל
        if (LostPanel != null)
        {
            LostPanel.SetActive(true);
        }

        // 4. עצירת המשחק
        Time.timeScale = 0f;
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