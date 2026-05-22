using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossHealthBar : MonoBehaviour
{
    public static BossHealthBar Instance { get; private set; }

    [Header("Сегменты")]
    public BossHexSegment[] segments = new BossHexSegment[5];

    [Header("Крестики")]
    public GameObject[] crosses = new GameObject[5];

    [Header("HP")]
    public float maxHP = 100f;
    public float damagePerHit300 = 2.0f;
    public float damagePerHit100 = 1.2f;
    public float damagePerHit50 = 0.5f;

    [Header("Множитель HP от числа нот")]
    public float hpPerNote = 1.0f;

    [Header("Panels")]
    public GameObject normalPanel;
    public GameObject deathPanel;

    [Header("Boss Image (UI)")]
    public Image bossImage;

    [Header("Boss Damage Animation")]
    public float shakeStrength = 15f;
    public float shakeDuration = 0.3f;
    public float flashDuration = 0.15f;
    public Color flashColor = new Color(1f, 0.3f, 0.3f, 1f);

    [Header("Boss Death Animation")]
    public Image moggedImage1;
    public Image moggedImage2;
    public Image moggedImage3;
    public Vector2 mogged1Position = new Vector2(0, 50);
    public Vector2 mogged2Position = new Vector2(-30, -20);
    public Vector2 mogged3Position = new Vector2(30, -20);
    public Vector2 mogged1Size = new Vector2(100, 100);
    public Vector2 mogged2Size = new Vector2(100, 100);
    public Vector2 mogged3Size = new Vector2(100, 100);
    public float mogged1Rotation = 0f;
    public float mogged2Rotation = -15f;
    public float mogged3Rotation = 15f;
    public float moggedFadeDuration = 0.5f;

    private float hp;
    private bool isDead;
    private float HpPerSegment => maxHP / segments.Length;

    private Vector2 bossOriginalPos;
    private Color bossOriginalColor;
    private bool isAnimating = false;

    private bool[] segmentDead;

    private bool isTutorialMode = false;

    public System.Action OnBossDeath;
    public System.Action<float> OnHPChanged;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        hp = maxHP;
        isDead = false;
        segmentDead = new bool[segments.Length];

        if (bossImage != null)
        {
            bossOriginalPos = bossImage.rectTransform.anchoredPosition;
            bossOriginalColor = bossImage.color;
        }

        foreach (var cross in crosses)
            if (cross != null) cross.SetActive(false);

        if (moggedImage1 != null)
        {
            moggedImage1.gameObject.SetActive(false);
            moggedImage1.rectTransform.anchoredPosition = mogged1Position;
            moggedImage1.rectTransform.sizeDelta = mogged1Size;
            moggedImage1.rectTransform.localRotation = Quaternion.Euler(0, 0, mogged1Rotation);
        }
        if (moggedImage2 != null)
        {
            moggedImage2.gameObject.SetActive(false);
            moggedImage2.rectTransform.anchoredPosition = mogged2Position;
            moggedImage2.rectTransform.sizeDelta = mogged2Size;
            moggedImage2.rectTransform.localRotation = Quaternion.Euler(0, 0, mogged2Rotation);
        }
        if (moggedImage3 != null)
        {
            moggedImage3.gameObject.SetActive(false);
            moggedImage3.rectTransform.anchoredPosition = mogged3Position;
            moggedImage3.rectTransform.sizeDelta = mogged3Size;
            moggedImage3.rectTransform.localRotation = Quaternion.Euler(0, 0, mogged3Rotation);
        }

        RefreshSegments(instant: true);

        if (deathPanel) deathPanel.SetActive(false);
        if (normalPanel) normalPanel.SetActive(true);
    }

    public void InitFromNoteCount(int totalNotes)
    {
        if (totalNotes <= 0) return;

        maxHP = totalNotes * hpPerNote;
        hp = maxHP;
        var damageDecrease = GameSettings.Instance.DamageDecrease != null
            ? GameSettings.Instance.DamageDecrease
            : 1f;

        damagePerHit300 = maxHP / (totalNotes * 0.20f * damageDecrease);
        damagePerHit100 = maxHP / (totalNotes * 0.40f * damageDecrease);
        damagePerHit50 = maxHP / (totalNotes * 0.60f * damageDecrease);

        segmentDead = new bool[segments.Length];
        RefreshSegments(instant: true);
    }

    public void SetTutorialMode(bool enabled)
    {
        isTutorialMode = enabled;

        if (isTutorialMode)
        {
            if (normalPanel != null)
                normalPanel.SetActive(false);

            foreach (var seg in segments)
                if (seg != null) seg.gameObject.SetActive(false);

            foreach (var cross in crosses)
                if (cross != null) cross.SetActive(false);
        }
    }

    public void HideTutorialBoss()
    {
        if (bossImage != null)
            StartCoroutine(FadeTutorialBoss());
    }

    private IEnumerator FadeTutorialBoss()
    {
        if (bossImage == null) yield break;

        float elapsed = 0f;
        float duration = 1f;
        Color startColor = bossImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            bossImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        bossImage.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        bossImage.gameObject.SetActive(false);
    }


    public void RegisterJudgement(Judgement j)
    {
        switch (j)
        {
            case Judgement.Hit300: TakeDamage(damagePerHit300); break;
            case Judgement.Hit100: TakeDamage(damagePerHit100); break;
            case Judgement.Hit50: TakeDamage(damagePerHit50); break;
            case Judgement.Miss: HealBoss(damagePerHit100); break;
        }
    }

    private void TakeDamage(float amount)
    {
        if (isDead || isTutorialMode) return;

        hp = Mathf.Max(0f, hp - amount);
        OnHPChanged?.Invoke(hp);

        RefreshSegments(instant: false);
        CheckSegmentDeaths();

        if (!isAnimating && bossImage != null)
            StartCoroutine(BossDamageAnimation());

        Debug.Log($"[Boss] HP: {hp:F1}/{maxHP}");

        if (hp <= 0f && !isDead)
            StartCoroutine(DeathSequence());
    }

    private void HealBoss(float amount)
    {
        if (isDead || isTutorialMode) return;

        hp = Mathf.Min(maxHP, hp + amount);
        OnHPChanged?.Invoke(hp);

        RefreshSegments(instant: false);
        CheckSegmentRevival();

        Debug.Log($"[Boss] Healed! HP: {hp:F1}/{maxHP}");
    }

    private void CheckSegmentRevival()
    {
        for (int i = segments.Length - 1; i >= 0; i--)
        {
            if (!segmentDead[i]) continue;

            var segMin = i * HpPerSegment;
            var segMax = (i + 1) * HpPerSegment;

            bool shouldRevive;
            if (i == 4)
                shouldRevive = hp > segMin + HpPerSegment * 0.2f;
            else
                shouldRevive = hp > segMin;

            if (!shouldRevive) continue;

            segmentDead[i] = false;

            if (crosses[i] != null)
                crosses[i].SetActive(false);

            if (segments[i] != null)
                segments[i].gameObject.SetActive(true);
        }
    }


    private void CheckSegmentDeaths()
    {
        for (int i = 0; i < segments.Length; i++)
        {
            if (segmentDead[i]) continue;

            var segMin = i * HpPerSegment;

            bool isDead;
            if (i == 4)
                isDead = hp <= segMin + HpPerSegment * 0.2f;
            else
                isDead = hp <= segMin;

            if (!isDead) continue;

            segmentDead[i] = true;
            StartCoroutine(ReplaceWithCross(i));

            if (DebuffManager.Instance != null)
                DebuffManager.Instance.OnSegmentDestroyed(i);
        }
    }


    private IEnumerator ReplaceWithCross(int index)
    {
        yield return new WaitForSeconds(0.1f);

        if (segments[index] != null)
            segments[index].gameObject.SetActive(false);

        if (index < crosses.Length && crosses[index] != null)
        {
            crosses[index].SetActive(true);
            yield return StartCoroutine(PopIn(crosses[index]));
        }
    }

    private IEnumerator PopIn(GameObject target)
    {
        RectTransform rt = target.GetComponent<RectTransform>();
        if (rt == null) yield break;

        var elapsed = 0f;
        var duration = 0.2f;
        rt.localScale = Vector3.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / duration;
            var scale = Mathf.LerpUnclamped(0f, 1f, EaseOutBack(t));
            rt.localScale = Vector3.one * scale;
            yield return null;
        }
        rt.localScale = Vector3.one;
    }

    private float EaseOutBack(float t)
    {
        var c1 = 1.70158f;
        var c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }


    private IEnumerator BossDamageAnimation()
    {
        isAnimating = true;
        yield return StartCoroutine(BossShakeAndFlash());
        isAnimating = false;
    }

    private IEnumerator BossShakeAndFlash()
    {
        if (bossImage == null) yield break;

        RectTransform rt = bossImage.rectTransform;
        var elapsed = 0f;
        var shakeDur = shakeDuration;
        var flashDur = flashDuration;
        bossImage.color = flashColor;

        while (elapsed < shakeDur)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / shakeDur;

            var strength = Mathf.Lerp(shakeStrength, 0f, t);
            var offsetX = Random.Range(-strength, strength);
            var offsetY = Random.Range(-strength * 0.5f, strength * 0.5f);

            rt.anchoredPosition = bossOriginalPos + new Vector2(offsetX, offsetY);

            if (elapsed < flashDur)
            {
                var flashT = elapsed / flashDur;
                bossImage.color = Color.Lerp(flashColor,
                    bossOriginalColor, flashT);
            }
            else
                bossImage.color = bossOriginalColor;

            yield return null;
        }

        rt.anchoredPosition = bossOriginalPos;
        bossImage.color = bossOriginalColor;
    }

    private void RefreshSegments(bool instant)
    {
        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] == null) continue;
            if (segmentDead != null && segmentDead[i]) continue;

            var segMin = i * HpPerSegment;
            var segMax = (i + 1) * HpPerSegment;

            float fill;
            if (hp >= segMax) fill = 1f;
            else if (hp <= segMin) fill = 0f;
            else fill = (hp - segMin) / HpPerSegment;

            segments[i].SetFill(fill, instant);

            if (!instant && fill > 0f && fill < 1f)
                segments[i].FlashDamage();
        }
    }



    private IEnumerator DeathSequence()
    {
        isDead = true;

        yield return new WaitForSeconds(0.5f);
        if (normalPanel) normalPanel.SetActive(false);

        foreach (var cross in crosses)
            if (cross != null) cross.SetActive(true);

        if (bossImage != null)
            yield return StartCoroutine(BossDeathAnimation());

        OnBossDeath?.Invoke();
        Debug.Log("[Boss] Босс побеждён!");
    }

    private IEnumerator BossDeathAnimation()
    {
        if (bossImage == null) yield break;

        Color darkenedColor = new Color(
            bossOriginalColor.r * 0.2f,
            bossOriginalColor.g * 0.2f,
            bossOriginalColor.b * 0.2f,
            bossOriginalColor.a);

        float elapsed = 0f;
        float darkenDuration = 0.5f;

        while (elapsed < darkenDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / darkenDuration;
            bossImage.color = Color.Lerp(bossOriginalColor, darkenedColor, t);
            yield return null;
        }

        bossImage.color = darkenedColor;

        if (moggedImage1 != null)
        {
            moggedImage1.gameObject.SetActive(true);
            yield return StartCoroutine(FadeInMogged(moggedImage1));
        }

        if (moggedImage2 != null)
        {
            moggedImage2.gameObject.SetActive(true);
            yield return StartCoroutine(FadeInMogged(moggedImage2));
        }

        if (moggedImage3 != null)
        {
            moggedImage3.gameObject.SetActive(true);
            yield return StartCoroutine(FadeInMogged(moggedImage3));
        }
    }

    private IEnumerator FadeInMogged(Image moggedImage)
    {
        Color startColor = moggedImage.color;
        startColor.a = 0f;
        moggedImage.color = startColor;

        float elapsed = 0f;
        while (elapsed < moggedFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moggedFadeDuration;
            Color newColor = moggedImage.color;
            newColor.a = Mathf.Lerp(0f, 1f, t);
            moggedImage.color = newColor;
            yield return null;
        }

        Color finalColor = moggedImage.color;
        finalColor.a = 1f;
        moggedImage.color = finalColor;
    }

    private IEnumerator FadeIn(GameObject target, float duration)
    {
        CanvasGroup cg = target.GetComponent<CanvasGroup>();
        if (cg == null) cg = target.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    public float GetHP() => hp;
    public float GetPercent() => hp / maxHP;
    public bool IsDead() => isDead;
    public bool IsTutorialMode() => isTutorialMode;
}