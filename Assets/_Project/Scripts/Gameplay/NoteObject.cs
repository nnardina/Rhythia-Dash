using UnityEngine;

public class NoteObject : MonoBehaviour
{
    [Header("Timing")]
    public float beatTime;
    public float endTime;
    public float AR = 8f;

    [Header("Type")]
    public bool isLongNote = false;

    [HideInInspector] public bool isBeingHeld = false;

    private const float START_X = 10f;
    private const float TARGET_X = -7f;

    private float preempt;
    private float window50;

    private bool headMissed = false;

    private Transform bodyTransform;
    private Transform tailTransform;
    private float bodyNativeWidth = 1f;

    void Start()
    {
        preempt = ARToPreempt(AR);
        window50 = (200f - 10f * 8f) / 1000f;

        bodyTransform = transform.Find("Body");
        tailTransform = transform.Find("Tail");

        if (bodyTransform != null)
        {
            var sr = bodyTransform.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
                bodyNativeWidth = sr.sprite.bounds.size.x;

            bodyTransform.gameObject.SetActive(isLongNote);
        }

        if (tailTransform != null)
            tailTransform.gameObject.SetActive(isLongNote);
    }

    void Update()
    {
        float songPos = Conductor.instance.songPosition;

        UpdateHeadPosition(songPos);

        if (isLongNote)
        {
            UpdateBody(songPos);
            CheckTailAutoMiss(songPos);
        }
        else
        {
            CheckHeadAutoMiss(songPos);
        }
    }

    void UpdateHeadPosition(float songPos)
    {
        if (isBeingHeld)
        {

            transform.position = new Vector3(TARGET_X, transform.position.y, 0f);
            return;
        }

        float progress = GetProgress(beatTime, songPos);
        float x = Mathf.Lerp(START_X, TARGET_X, progress);
        transform.position = new Vector3(x, transform.position.y, 0f);

        if (!isLongNote && progress > 1.5f)
            Destroy(gameObject);
    }

    void CheckHeadAutoMiss(float songPos)
    {
        if (!headMissed && songPos > beatTime + window50)
        {
            headMissed = true;
            ScoreManager.instance.RegisterHit(Judgement.Miss);
            Destroy(gameObject);
        }
    }

    void UpdateBody(float songPos)
    {
        if (bodyTransform == null) return;

        float headX = transform.position.x;
        float tailX = Mathf.Lerp(START_X, TARGET_X, GetProgress(endTime, songPos));

        float bodyLength = tailX - headX;

        if (bodyTransform != null)
        {
            if (bodyLength > 0f)
            {
                bodyTransform.gameObject.SetActive(true);
                bodyTransform.position = new Vector3(headX + bodyLength * 0.5f, transform.position.y, 0f);
                bodyTransform.localScale = new Vector3(bodyLength / bodyNativeWidth, bodyTransform.localScale.y, 1f);
            }
            else
            {
                bodyTransform.gameObject.SetActive(false);
            }
        }

        if (tailTransform != null)
        {
            if (bodyLength > 0f)
            {
                tailTransform.gameObject.SetActive(true);
                tailTransform.position = new Vector3(tailX, transform.position.y, 0f);
            }
            else
            {
                tailTransform.gameObject.SetActive(false);
            }
        }
    }

    void CheckTailAutoMiss(float songPos)
    {

        if (songPos > endTime + window50)
        {
            ScoreManager.instance.RegisterHit(Judgement.Miss);
            Destroy(gameObject);
        }
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