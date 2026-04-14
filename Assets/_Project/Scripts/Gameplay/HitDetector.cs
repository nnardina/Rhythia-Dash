using UnityEngine;
using UnityEngine.InputSystem;

public enum Judgement { Perfect, Great, Good, Ok, Meh, Miss }

public class HitDetector : MonoBehaviour
{
    public KeyCode laneKey;
    public float laneY;
    public float OD = 8f;

    private float perfectWindow;
    private float greatWindow;
    private float goodWindow;
    private float okWindow;
    private float mehWindow;

    void Start()
    {
        perfectWindow = 16f / 1000f;
        greatWindow = (64f - 3f * OD) / 1000f;
        goodWindow = (97f - 3f * OD) / 1000f;
        okWindow = (127f - 3f * OD) / 1000f;
        mehWindow = (151f - 3f * OD) / 1000f;
    }

    void Update()
    {
        if (!Input.GetKeyDown(laneKey)) return;

        NoteObject closest = FindClosestNote();
        if (closest == null) return;

        float delta = Mathf.Abs(Conductor.instance.songPosition - closest.beatTime);

        if (delta > mehWindow) return;

        Judgement judgement = GetJudgement(delta);
        Destroy(closest.gameObject);
        ScoreManager.instance.RegisterHit(judgement);
    }

    Judgement GetJudgement(float delta)
    {
        if (delta < perfectWindow) return Judgement.Perfect;
        if (delta < greatWindow) return Judgement.Great;
        if (delta < goodWindow) return Judgement.Good;
        if (delta < okWindow) return Judgement.Ok;
        if (delta < mehWindow) return Judgement.Meh;
        return Judgement.Miss;
    }

    NoteObject FindClosestNote()
    {
        NoteObject closest = null;
        float minDelta = float.MaxValue;

        foreach (var note in FindObjectsByType<NoteObject>())
        {
            if (Mathf.Abs(note.transform.position.y - laneY) > 0.5f) continue;
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