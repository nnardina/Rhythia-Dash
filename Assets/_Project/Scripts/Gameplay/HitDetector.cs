using UnityEngine;

public enum Judgement { Hit300, Hit100, Hit50, Miss }

public class HitDetector : MonoBehaviour
{
    public KeyCode laneKey;
    public float laneX;
    public float OD = 8f;

    [Header("Receiver Sprites")]
    public SpriteRenderer idleRenderer;
    public SpriteRenderer pressedRenderer;

    [Header("Lane Index (0-3)")]
    public int laneIndex = 0;

    private float window300;
    private float window100;
    private float window50;
    private NoteObject heldNote = null;

    void Start()
    {
        if (GameSettings.Instance != null)
            laneKey = GameSettings.Instance.LaneKeys[laneIndex];

        window300 = (80f - 6f * OD) / 1000f;
        window100 = (140f - 8f * OD) / 1000f;
        window50 = (200f - 10f * OD) / 1000f;

        if (idleRenderer != null) idleRenderer.enabled = true;
        if (pressedRenderer != null) pressedRenderer.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(laneKey)) HandleKeyDown();
        if (Input.GetKeyUp(laneKey)) HandleKeyUp();
    }

    void HandleKeyDown()
    {
        DebuffManager.Instance?.OnKeyPressed();

        if (idleRenderer != null) idleRenderer.enabled = false;
        if (pressedRenderer != null) pressedRenderer.enabled = true;

        NoteObject closest = FindClosestNote();
        if (closest == null) return;

        float delta = Conductor.instance.songPosition - closest.beatTime;
        float absDelta = Mathf.Abs(delta);
        float windowMiss = (300f - 10f * OD) / 1000f;

        if (absDelta > windowMiss) return;

        if (!closest.isLongNote && absDelta > window50)
        {
            ScoreManager.instance.RegisterHit(Judgement.Miss);
            closest.isMissed = true;
            return;
        }

        if (closest.isLongNote && absDelta > window50) return;

        Judgement headJudgement = GetJudgement(absDelta);
        ScoreManager.instance.RegisterHit(headJudgement);
        BossHealthBar.Instance?.RegisterJudgement(headJudgement);

        if (closest.isLongNote)
        {
            closest.isBeingHeld = true;
            heldNote = closest;
        }
        else
        {
            Destroy(closest.gameObject);
        }
    }

    void HandleKeyUp()
    {
        if (idleRenderer != null) idleRenderer.enabled = true;
        if (pressedRenderer != null) pressedRenderer.enabled = false;

        if (heldNote == null) return;

        float songPos = Conductor.instance.songPosition;
        Judgement tailJudgement = heldNote.GetTailJudgement(songPos, window300, window100, window50);

        if (tailJudgement == Judgement.Miss)
        {
            ScoreManager.instance.RegisterHit(Judgement.Miss);
            heldNote.SetMissed();
            heldNote.isBeingHeld = false;
        }
        else
        {
            ScoreManager.instance.RegisterHit(tailJudgement);
            BossHealthBar.Instance?.RegisterJudgement(tailJudgement);
            Destroy(heldNote.gameObject);
        }

        heldNote = null;
    }

    Judgement GetJudgement(float delta)
    {
        if (delta <= window300) return Judgement.Hit300;
        if (delta <= window100) return Judgement.Hit100;
        return Judgement.Hit50;
    }

    NoteObject FindClosestNote()
    {
        NoteObject closest = null;
        float minDelta = float.MaxValue;

        foreach (var note in NoteObject.All)
        {
            if (note.isBeingHeld) continue;
            if (note.isMissed) continue;
            if (Mathf.Abs(note.transform.position.x - laneX) > 0.5f) continue;

            float delta = Mathf.Abs(Conductor.instance.songPosition - note.beatTime);
            if (delta < minDelta)
            {
                minDelta = delta;
                closest = note;
            }
        }
        return closest;
    }
}