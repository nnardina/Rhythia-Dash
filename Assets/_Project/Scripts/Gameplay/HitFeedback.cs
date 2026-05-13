using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HitFeedback : MonoBehaviour
{
    public static HitFeedback instance;

    [Header("Judgment Sprites")]
    public Sprite sprite300;
    public Sprite sprite100;
    public Sprite sprite50;
    public Sprite spriteMiss;

    [Header("Combo Number Sprites")]
    public Sprite[] numberSprites;

    [Header("UI Elements")]
    public Image judgmentImage;
    public RectTransform comboContainer;
    public Image[] comboDigits;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hitsound;

    [Header("Animation Settings")]
    public float judgmentFadeDuration = 0.5f;
    public float judgmentScaleStart = 1.5f;
    public float comboBumpScale = 1.3f;
    public float comboBumpDuration = 0.2f;
    public float comboBreakDuration = 0.5f;
    public float comboBreakScale = 1.5f;
    public Color comboBreakColor = Color.red;

    private Coroutine judgmentCoroutine;
    private Coroutine comboCoroutine;
    private int lastCombo = 0;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("[HitFeedback] AudioSource not assigned! Hitsounds will not play.");
        }

        if (hitsound == null)
        {
            Debug.LogWarning("[HitFeedback] Hitsound clip not assigned!");
        }
    }

    public void ShowJudgment(Judgement judgement)
    {
        Sprite sprite = judgement switch
        {
            Judgement.Hit300 => sprite300,
            Judgement.Hit100 => sprite100,
            Judgement.Hit50 => sprite50,
            Judgement.Miss => spriteMiss,
            _ => null
        };

        if (sprite != null && judgmentImage != null)
        {
            if (judgmentCoroutine != null) StopCoroutine(judgmentCoroutine);
            judgmentCoroutine = StartCoroutine(AnimateJudgment(sprite));
        }

        if (judgement != Judgement.Miss && audioSource != null && hitsound != null)
        {
            audioSource.PlayOneShot(hitsound);
        }
    }

    public void UpdateCombo(int combo)
    {
        if (comboContainer == null) return;

        if (combo == 0 && lastCombo > 0)
        {
            if (comboCoroutine != null) StopCoroutine(comboCoroutine);
            comboCoroutine = StartCoroutine(AnimateComboBreak());
            lastCombo = 0;
            return;
        }

        if (combo == 0)
        {
            comboContainer.gameObject.SetActive(false);
            lastCombo = 0;
            return;
        }

        comboContainer.gameObject.SetActive(true);

        string comboStr = combo.ToString();
        int digitCount = comboStr.Length;

        for (int i = 0; i < comboDigits.Length; i++)
        {
            if (i < digitCount)
            {
                comboDigits[i].gameObject.SetActive(true);
                int digit = int.Parse(comboStr[i].ToString());
                comboDigits[i].sprite = numberSprites[digit];
                comboDigits[i].SetNativeSize();
                comboDigits[i].color = Color.white;
            }
            else
            {
                comboDigits[i].gameObject.SetActive(false);
            }
        }

        if (comboCoroutine != null) StopCoroutine(comboCoroutine);
        comboCoroutine = StartCoroutine(AnimateCombo(digitCount));
        
        lastCombo = combo;
    }

    IEnumerator AnimateCombo(int digitCount)
    {
        float elapsed = 0f;

        while (elapsed < comboBumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / comboBumpDuration;

            float scale = Mathf.Lerp(comboBumpScale, 1f, t);

            for (int i = 0; i < digitCount; i++)
            {
                comboDigits[i].transform.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        for (int i = 0; i < digitCount; i++)
        {
            comboDigits[i].transform.localScale = Vector3.one;
        }
    }

    IEnumerator AnimateComboBreak()
    {
        int digitCount = 0;
        for (int i = 0; i < comboDigits.Length; i++)
        {
            if (comboDigits[i].gameObject.activeSelf) digitCount++;
        }

        float elapsed = 0f;

        while (elapsed < comboBreakDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / comboBreakDuration;

            float scale = Mathf.Lerp(1f, comboBreakScale, t);
            Color color = Color.Lerp(comboBreakColor, new Color(comboBreakColor.r, comboBreakColor.g, comboBreakColor.b, 0f), t);

            for (int i = 0; i < digitCount; i++)
            {
                comboDigits[i].transform.localScale = Vector3.one * scale;
                comboDigits[i].color = color;
            }

            yield return null;
        }

        comboContainer.gameObject.SetActive(false);

        for (int i = 0; i < comboDigits.Length; i++)
        {
            comboDigits[i].transform.localScale = Vector3.one;
            comboDigits[i].color = Color.white;
        }
    }

    IEnumerator AnimateJudgment(Sprite sprite)
    {
        judgmentImage.sprite = sprite;
        judgmentImage.SetNativeSize();
        judgmentImage.gameObject.SetActive(true);

        float elapsed = 0f;
        Color startColor = Color.white;
        Color endColor = new Color(1f, 1f, 1f, 0f);

        while (elapsed < judgmentFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / judgmentFadeDuration;

            float scale = Mathf.Lerp(judgmentScaleStart, 1f, t);
            judgmentImage.transform.localScale = Vector3.one * scale;

            judgmentImage.color = Color.Lerp(startColor, endColor, t);

            yield return null;
        }

        judgmentImage.gameObject.SetActive(false);
        judgmentImage.transform.localScale = Vector3.one;
        judgmentImage.color = Color.white;
    }
}
