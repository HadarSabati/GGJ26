using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public struct GodInfo
    {
        public string godName;
        public GameObject godPrefab; // שינוי מ-Sprite ל-GameObject (ה-Prefab עם האנימציה)
        public string targetMaskType;
    }

    public List<GodInfo> godsList;
    public float timeBetweenGods = 3f;

    [Header("Placement")]
    public Transform godSpawnPoint; // המקום שבו האנימציה תיווצר (למשל מעל הר הגעש)
    private GameObject currentGodInstance; // משתנה לשמירת האובייקט הנוכחי כדי שנוכל למחוק אותו

    [Header("Lava Mask Control")]
    public Transform lavaMask;
    public float minYPosition;
    public float maxYPosition;

    [Header("Lava Logic")]
    public float currentLava = 50f;
    public float lavaRiseSpeed = 10f;
    public float dampingFactor = 0.5f;

    private float elapsedTime = 0f;
    private float godTimer = 0f;
    private int lastGodIndex = -1;
    [HideInInspector] public GodInfo activeGod;

    void Start() => SwitchGod();

    void Update()
    {
        elapsedTime += Time.deltaTime;

        float speedMultiplier = 1f / (1f + (elapsedTime * dampingFactor));
        currentLava += lavaRiseSpeed * speedMultiplier * Time.deltaTime;
        currentLava = Mathf.Clamp(currentLava, 0, 100);

        if (lavaMask != null)
        {
            float newY = Mathf.Lerp(minYPosition, maxYPosition, currentLava / 100f);
            lavaMask.localPosition = new Vector2(lavaMask.localPosition.x, newY);
        }

        godTimer += Time.deltaTime;
        if (godTimer >= timeBetweenGods)
        {
            SwitchGod();
        }
    }

    public void DecreaseLava(float amount)
    {
        currentLava -= amount;
        currentLava = Mathf.Clamp(currentLava, 0, 100);
        Debug.Log("Sacrifice Successful! Lava Decreased.");

        SwitchGod();
    }

    public void SwitchGod()
    {
        if (godsList.Count <= 1) return;

        // 1. בחירת אל חדש
        int newIndex;
        do { newIndex = Random.Range(0, godsList.Count); } while (newIndex == lastGodIndex);
        lastGodIndex = newIndex;
        activeGod = godsList[newIndex];

        // 2. השמדת האנימציה הקודמת אם קיימת
        if (currentGodInstance != null)
        {
            Destroy(currentGodInstance);
        }

        // 3. יצירת האנימציה החדשה מה-Prefab
        if (activeGod.godPrefab != null && godSpawnPoint != null)
        {
            currentGodInstance = Instantiate(activeGod.godPrefab, godSpawnPoint.position, Quaternion.identity);
            // הצמדת האנימציה לנקודת הספאון (אופציונלי, עוזר אם המצלמה זזה)
            currentGodInstance.transform.SetParent(godSpawnPoint);
        }

        godTimer = 0f;
    }
}