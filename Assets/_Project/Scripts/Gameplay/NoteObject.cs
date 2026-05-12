using UnityEngine;
using System.Collections.Generic;

public class NoteObject : MonoBehaviour
{
    [Header("Timing")]
    public float beatTime;
    public float endTime;
    public float AR = 8f;
    public float OD = 8f;

    [Header("Type")]
    public bool isLongNote = false;

    [Header("Long Note Body Offsets")]
    public float bodyTopOffset = 0f;
    public float bodyBottomOffset = 0f;

    [HideInInspector] public bool isBeingHeld = false;
    [HideInInspector] public bool isMissed = false;

    public static readonly HashSet<NoteObject> All = new HashSet<NoteObject>();

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    private const float START_Y = 10f;
    private const float TARGET_Y = -3.5f;
    private const float DIM = 0.7f;
    private const float DESTROY_Y = -15f;

    private float preempt;
    private float window50;
    private float speed;

    private bool headMissRegistered = false;

    private float missedHeadY = 0f;
    private float missedTailY = 0f;
    private float missedSongPos = 0f;

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
        window50 = (200f - 10f * OD) / 1000f;
        speed = (START_Y - TARGET_Y) / preempt;

        bodyTransform = transform.Find("Body");
        tailTransform = transform.Find("Tail");
        headRenderer = GetComponent<SpriteRenderer>();

        if (bodyTransform != null)
        {
            bodyLocalX = bodyTransform.localPosition.x;
            bodyRenderer = bodyTransform.GetComponent<SpriteRenderer>();
            if (bodyRenderer != null && bodyRenderer.sprite != null)
                bodyNativeHeight = bodyRenderer.sprite.bounds.size.y;
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

        if (!isLongNote)
        {
            UpdateRegularNote(songPos);
            if (!isMissed) CheckHeadAutoMiss(songPos);
            return;
        }

        float headY = CalcHeadY(songPos);
        float tailY = CalcTailY(songPos);

        transform.position = new Vector3(transform.position.x, headY, 0f);

        UpdateBody(headY, tailY);

        if (!isMissed)
        {
            CheckLNHeadAutoMiss(songPos);
            CheckTailAutoMiss(songPos);
        }

        if (tailY < DESTROY_Y) Destroy(gameObject);
    }

    float CalcHeadY(float songPos)
    {
        if (isBeingHeld) return TARGET_Y;

        if (isMissed) return missedHeadY - speed * (songPos - missedSongPos);

        return TARGET_Y + (beatTime - songPos) * speed;
    }

    float CalcTailY(float songPos)
    {
        if (isMissed) return missedTailY - speed * (songPos - missedSongPos);

        return TARGET_Y + (endTime - songPos) * speed;
    }

    void UpdateRegularNote(float songPos)
    {
        float y = TARGET_Y + (beatTime - songPos) * speed;
        transform.position = new Vector3(transform.position.x, y, 0f);
        if (y < DESTROY_Y) Destroy(gameObject);
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

    void UpdateBody(float headY, float tailY)
    {
        if (bodyTransform == null) return;

        float displayTailY = Mathf.Min(tailY, START_Y);

        float headHalf = headRenderer != null ? headRenderer.bounds.extents.y : 0f;
        float tailHalf = (displayTailY < START_Y && tailRenderer != null) ? tailRenderer.bounds.extents.y : 0f;

        float bodyStart = headY + headHalf + bodyTopOffset;
        float bodyEnd = displayTailY - tailHalf + Mathf.Abs(bodyBottomOffset);
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
            bool tailOnScreen = displayTailY < START_Y && bodyEnd > headY;
            tailTransform.gameObject.SetActive(tailOnScreen);
            if (tailOnScreen)
                tailTransform.localPosition = new Vector3(tailLocalX, displayTailY - headY, 0f);
        }
    }

    public void SetMissed()
    {
        float songPos = Conductor.instance.songPosition;

        float currentHeadY = isBeingHeld
            ? TARGET_Y
            : TARGET_Y + (beatTime - songPos) * speed;
        float currentTailY = TARGET_Y + (endTime - songPos) * speed;

        isMissed = true;
        missedSongPos = songPos;
        missedHeadY = currentHeadY;
        missedTailY = currentTailY;

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
}
