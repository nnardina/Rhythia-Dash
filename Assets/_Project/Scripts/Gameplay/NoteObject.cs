using UnityEngine;

public class NoteObject : MonoBehaviour
{
    [Header("Timing")]
    public float beatTime;
    public float endTime;
    public float AR = 8f;

    [Header("Type")]
    public bool isLongNote = false;

    [Header("Long Note Body Offsets")]
    public float bodyTopOffset = 0f;
    public float bodyBottomOffset = 0f;

    [HideInInspector] public bool isBeingHeld = false;
    [HideInInspector] public bool isMissed = false;

    private const float START_Y = 10f;
    private const float TARGET_Y = -3.5f;
    private const float DIM = 0.7f;

    private float preempt;
    private float window50;
    private bool headMissRegistered = false;

    private Transform bodyTransform;
    private Transform tailTransform;
    private float bodyNativeHeight = 1f;
    private float bodyLocalX = 0f;
    private float tailLocalX = 0f;
    private SpriteRenderer headRenderer;
    private SpriteRenderer bodyRenderer;
    private SpriteRenderer tailRenderer;

    void Start()
    {
        preempt = ARToPreempt(AR);
        window50 = (200f - 10f * 8f) / 1000f;

        bodyTransform = transform.Find("Body");
        tailTransform = transform.Find("Tail");

        headRenderer = GetComponent<SpriteRenderer>();

        if (bodyTransform != null)
        {
            bodyLocalX = bodyTransform.localPosition.x;
            bodyRenderer = bodyTransform.GetComponent<SpriteRenderer>();
            var sr = bodyRenderer;
            if (sr != null && sr.sprite != null)
                bodyNativeHeight = sr.sprite.bounds.size.y;

            bodyTransform.gameObject.SetActive(isLongNote);
        }

        if (tailTransform != null)
        {
            tailLocalX = tailTransform.localPosition.x;
            tailRenderer = tailTransform.GetComponent<SpriteRenderer>();
            tailTransform.gameObject.SetActive(isLongNote);
        }
    }

    void Update()
    {
        float songPos = Conductor.instance.songPosition;

        UpdateHeadPosition(songPos);

        if (isLongNote)
        {
            UpdateBody(songPos);
            if (!isMissed) CheckLNHeadAutoMiss(songPos);
            if (!isMissed) CheckTailAutoMiss(songPos);
        }
        else
        {
            if (!isMissed) CheckHeadAutoMiss(songPos);
        }
    }

    void UpdateHeadPosition(float songPos)
    {
        if (isBeingHeld)
        {
            transform.position = new Vector3(transform.position.x, TARGET_Y, 0f);
            return;
        }

        float progress = GetProgress(beatTime, songPos);
        float y = Mathf.LerpUnclamped(START_Y, TARGET_Y, progress);
        transform.position = new Vector3(transform.position.x, y, 0f);

        if (progress > 1.5f)
            Destroy(gameObject);
    }

    void CheckHeadAutoMiss(float songPos)
    {
        if (songPos > beatTime + window50)
        {
            ScoreManager.instance.RegisterHit(Judgement.Miss);
            isMissed = true;
        }
    }

    void CheckLNHeadAutoMiss(float songPos)
    {
        if (!headMissRegistered && !isBeingHeld && songPos > beatTime + window50)
        {
            headMissRegistered = true;
            ScoreManager.instance.RegisterHit(Judgement.Miss);
            SetMissed();
        }
    }

    void CheckTailAutoMiss(float songPos)
    {
        if (isBeingHeld && songPos > endTime + window50)
        {
            ScoreManager.instance.RegisterHit(Judgement.Miss);
            SetMissed();
            isBeingHeld = false;
        }
    }

    void UpdateBody(float songPos)
    {
        if (bodyTransform == null) return;

        float headY = transform.position.y;
        float tailY = Mathf.LerpUnclamped(START_Y, TARGET_Y, GetProgress(endTime, songPos));

        float headHalfHeight = headRenderer != null ? headRenderer.bounds.extents.y : 0f;
        float tailHalfHeight = tailRenderer != null ? tailRenderer.bounds.extents.y : 0f;

        float bodyStart = headY + headHalfHeight + bodyTopOffset;
        float bodyEnd = tailY - tailHalfHeight - bodyBottomOffset;
        float bodyLength = bodyEnd - bodyStart;

        if (bodyLength > 0f)
        {
            bodyTransform.gameObject.SetActive(true);
            bodyTransform.localPosition = new Vector3(bodyLocalX, (bodyStart + bodyLength * 0.5f) - headY, 0f);
            bodyTransform.localScale = new Vector3(bodyTransform.localScale.x, bodyLength / bodyNativeHeight, 1f);
        }
        else
        {
            bodyTransform.gameObject.SetActive(false);
        }

        if (tailTransform != null)
        {
            if (bodyEnd > headY)
            {
                tailTransform.gameObject.SetActive(true);
                tailTransform.localPosition = new Vector3(tailLocalX, tailY - headY, 0f);
            }
            else
            {
                tailTransform.gameObject.SetActive(false);
            }
        }
    }

    public void SetMissed()
    {
        isMissed = true;
        Color dim = new Color(DIM, DIM, DIM, 1f);
        if (headRenderer != null) headRenderer.color = dim;
        if (bodyRenderer != null) bodyRenderer.color = dim;
        if (tailRenderer != null) tailRenderer.color = dim;
    }

    public Judgement GetTailJudgement(float songPos, float w300, float w100, float w50)
    {
        float delta = Mathf.Abs(songPos - endTime);
        if (delta <= w300) return Judgement.Hit300;
        if (delta <= w100) return Judgement.Hit100;
        if (delta <= w50) return Judgement.Hit50;
        return Judgement.Miss;
    }

    public static float ARToPreempt(float ar)
    {
        if (ar < 5f) return (1200f + 120f * (5f - ar)) / 1000f;
        if (ar > 5f) return (1200f - 150f * (ar - 5f)) / 1000f;
        return 1200f / 1000f;
    }

    float GetProgress(float targetBeatTime, float songPos)
    {
        float spawnTime = targetBeatTime - preempt;
        return (songPos - spawnTime) / preempt;
    }
}