using UnityEngine;
using System.Collections.Generic;

public class HitDetector : MonoBehaviour
{
    public KeyCode laneKey;
    public float laneY;

    void Update()
    {
        if (Input.GetKeyDown(laneKey))
        {
            NoteObject closest = FindClosestNote();
            if (closest == null)
            {
                Debug.Log(laneKey + " - Miss");
                return;
            }

            float delta = Mathf.Abs(Conductor.instance.songPosition - closest.beatTime);

            if (delta < 0.05f)
                Debug.Log(laneKey + " - PERFECT! (" + delta + ")");
            else if (delta < 0.1f)
                Debug.Log(laneKey + " - Good (" + delta + ")");
            else if (delta < 0.15f)
                Debug.Log(laneKey + " - OK (" + delta + ")");
            else
                Debug.Log(laneKey + " - Miss (" + delta + ")");

            if (delta < 0.15f) Destroy(closest.gameObject);
        }
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