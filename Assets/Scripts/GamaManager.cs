using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public struct GodInfo
    {
        public string godName;
        public Sprite godVisual;
        public string targetMaskType;
    }

    public List<GodInfo> godsList;
    public float timeBetweenGods = 3f;
    public SpriteRenderer godDisplay;

    [Header("Lava Mask Control")]
    public Transform lavaMask;
    public float minYPosition;
    public float maxYPosition;

    [Header("Lava Logic")]
    public float currentLava = 50f;
    public float lavaRiseSpeed = 10f;
    public float dampingFactor = 0.5f; // Dramatic slowdown

    private float elapsedTime = 0f;
    private float godTimer = 0f;
    private int lastGodIndex = -1;
    [HideInInspector] public GodInfo activeGod;

    void Start() => SwitchGod();

    void Update()
    {
        elapsedTime += Time.deltaTime;

        // Dynamic speed with negative acceleration
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
            godTimer = 0f;
        }
    }

    // Public method to decrease lava from other scripts
    public void DecreaseLava(float amount)
    {
        currentLava -= amount;
        currentLava = Mathf.Clamp(currentLava, 0, 100);
        Debug.Log("Lava Decreased! Current: " + currentLava);
    }

    void SwitchGod()
    {
        if (godsList.Count <= 1) return;
        int newIndex;
        do { newIndex = Random.Range(0, godsList.Count); } while (newIndex == lastGodIndex);
        lastGodIndex = newIndex;
        activeGod = godsList[newIndex];
        if (godDisplay != null) godDisplay.sprite = activeGod.godVisual;
    }
}