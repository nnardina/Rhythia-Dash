using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScoreDisplay : MonoBehaviour
{
    public static ScoreDisplay instance;

    [Header("Score Sprites")]
    public Sprite[] numberSprites;
    public Sprite commaSprite;
    public Sprite percentSprite;

    [Header("Score UI")]
    public RectTransform scoreContainer;
    public Image[] scoreDigits;

    [Header("Accuracy UI")]
    public RectTransform accuracyContainer;
    public Image[] accuracyDigits;
    public Image accuracyDotImage;
    public Image accuracyPercentImage;

    [Header("Animation")]
    public float lerpSpeed = 5f;

    private float displayedScore = 0f;
    private float targetScore = 0f;
    private float displayedAccuracy = 100f;
    private float targetAccuracy = 100f;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (accuracyPercentImage != null && percentSprite != null)
        {
            accuracyPercentImage.sprite = percentSprite;
            accuracyPercentImage.SetNativeSize();
        }

        if (accuracyDotImage != null && commaSprite != null)
        {
            accuracyDotImage.sprite = commaSprite;
            accuracyDotImage.SetNativeSize();
        }
    }

    void Update()
    {
        if (ScoreManager.instance == null) return;

        targetScore = ScoreManager.instance.score;
        targetAccuracy = ScoreManager.instance.GetAccuracy();

        displayedScore = Mathf.Lerp(displayedScore, targetScore, Time.deltaTime * lerpSpeed);
        displayedAccuracy = Mathf.Lerp(displayedAccuracy, targetAccuracy, Time.deltaTime * lerpSpeed);

        UpdateScoreDisplay();
        UpdateAccuracyDisplay();
    }

    void UpdateScoreDisplay()
    {
        if (scoreContainer == null || scoreDigits == null || scoreDigits.Length == 0) return;

        int score = Mathf.RoundToInt(displayedScore);
        string scoreStr = score.ToString("D8");

        for (int i = 0; i < scoreDigits.Length && i < scoreStr.Length; i++)
        {
            if (scoreDigits[i] != null)
            {
                int digit = int.Parse(scoreStr[i].ToString());
                scoreDigits[i].sprite = numberSprites[digit];
                scoreDigits[i].SetNativeSize();
                scoreDigits[i].gameObject.SetActive(true);
            }
        }
    }

    void UpdateAccuracyDisplay()
    {
        if (accuracyContainer == null || accuracyDigits == null || accuracyDigits.Length < 5) return;

        int wholePart = Mathf.FloorToInt(displayedAccuracy);
        int decimalPart = Mathf.RoundToInt((displayedAccuracy - wholePart) * 100f);

        string wholeStr = wholePart.ToString();

        for (int i = 0; i < 3; i++)
        {
            if (accuracyDigits[i] != null)
            {
                accuracyDigits[i].gameObject.SetActive(false);
            }
        }

        int startIndex = 3 - wholeStr.Length;
        for (int i = 0; i < wholeStr.Length; i++)
        {
            int digitIndex = startIndex + i;
            if (digitIndex >= 0 && digitIndex < 3 && accuracyDigits[digitIndex] != null)
            {
                int digit = int.Parse(wholeStr[i].ToString());
                accuracyDigits[digitIndex].sprite = numberSprites[digit];
                accuracyDigits[digitIndex].SetNativeSize();
                accuracyDigits[digitIndex].gameObject.SetActive(true);
            }
        }

        if (accuracyDotImage != null)
        {
            accuracyDotImage.gameObject.SetActive(true);
        }

        string decimalStr = decimalPart.ToString("00");
        if (accuracyDigits[3] != null)
        {
            int digit = int.Parse(decimalStr[0].ToString());
            accuracyDigits[3].sprite = numberSprites[digit];
            accuracyDigits[3].SetNativeSize();
            accuracyDigits[3].gameObject.SetActive(true);
        }

        if (accuracyDigits[4] != null)
        {
            int digit = int.Parse(decimalStr[1].ToString());
            accuracyDigits[4].sprite = numberSprites[digit];
            accuracyDigits[4].SetNativeSize();
            accuracyDigits[4].gameObject.SetActive(true);
        }

        if (accuracyPercentImage != null)
        {
            accuracyPercentImage.gameObject.SetActive(true);
        }
    }
}
