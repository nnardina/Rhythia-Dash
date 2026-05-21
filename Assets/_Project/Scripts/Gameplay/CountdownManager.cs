using UnityEngine;
using System.Collections;

public class CountdownManager : MonoBehaviour
{
    public static CountdownManager instance;

    [Header("Countdown Settings")]
    public Sprite arrowSprite;
    public GameObject arrowPrefab;

    [Header("Lane Positions")]
    public float[] laneXPositions = { -7f, -5f, -3f, -1f };

    [Header("Animation")]
    public float startY = 10f;
    public float targetY = -3.5f;
    public float endY = -15f;
    public float arrowScale = 3.14f;

    private bool countdownFinished = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public IEnumerator StartCountdown(float arValue)
    {
        countdownFinished = false;

        float preempt = NoteObject.ARToPreempt(arValue);
        float scrollMult = GameSettings.Instance != null
            ? GameSettings.Instance.ScrollSpeed
            : 1f;
        
        float adjustedStartY = startY * scrollMult;
        float speed = (adjustedStartY - targetY) / preempt;

        GameObject[] arrows = new GameObject[laneXPositions.Length];

        for (int i = 0; i < laneXPositions.Length; i++)
        {
            GameObject arrow = new GameObject($"CountdownArrow_{i}");
            arrow.transform.position = new Vector3(laneXPositions[i], adjustedStartY, 0f);
            arrow.transform.localScale = Vector3.one * arrowScale;

            SpriteRenderer sr = arrow.AddComponent<SpriteRenderer>();
            sr.sprite = arrowSprite;
            sr.sortingOrder = 10;

            arrows[i] = arrow;
        }

        float elapsed = 0f;
        bool musicStarted = false;

        while (true)
        {
            elapsed += Time.deltaTime;
            float currentY = adjustedStartY - speed * elapsed;

            for (int i = 0; i < arrows.Length; i++)
            {
                if (arrows[i] != null)
                {
                    arrows[i].transform.position = new Vector3(laneXPositions[i], currentY, 0f);
                }
            }

            if (!musicStarted && currentY <= targetY)
            {
                musicStarted = true;
                countdownFinished = true;
            }

            if (currentY <= endY)
            {
                break;
            }

            yield return null;
        }

        for (int i = 0; i < arrows.Length; i++)
        {
            if (arrows[i] != null)
            {
                Destroy(arrows[i]);
            }
        }
    }

    public bool IsCountdownFinished()
    {
        return countdownFinished;
    }
}
