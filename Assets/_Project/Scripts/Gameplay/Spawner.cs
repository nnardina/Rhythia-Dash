using UnityEngine;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    public GameObject notePrefab;
    public Conductor conductor;
    public List<float> noteTimes;
    public float[] laneYPositions = { 2f, 0f, -2f };
    public int[] noteLanes;

    private int nextIndex = 0;

    void Update()
    {
        if (nextIndex >= noteTimes.Count) return;

        NoteObject note = notePrefab.GetComponent<NoteObject>();
        if (conductor.songPosition >= noteTimes[nextIndex] - note.AR)
        {
            GameObject newNote = Instantiate(notePrefab);
            newNote.GetComponent<NoteObject>().beatTime = noteTimes[nextIndex];

            int lane = (noteLanes.Length > nextIndex) ? noteLanes[nextIndex] : 0;
            newNote.transform.position = new Vector3(10f, laneYPositions[lane], 0);

            nextIndex++;
        }
    }
}