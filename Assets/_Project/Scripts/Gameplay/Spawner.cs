using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class Spawner : MonoBehaviour
{
    [Header("References")]
    public GameObject notePrefab;
    public Conductor conductor;

    [Header("Map")]
    [Tooltip("Имя файла .osu.")]
    public string osuFileName = "map.osu";

    [Header("Lanes")]
    public float[] laneYPositions = { 3f, 1f, -1f, -3f };

    private List<NoteData> notes = new List<NoteData>();
    private int nextIndex = 0;
    private float arValue;
    private float preempt;
    private bool ready = false;
    private OsuBeatmap beatmap;

    void Start()
    {
        string osuPath = Path.Combine(Application.streamingAssetsPath, osuFileName);

        beatmap = OsuParser.Parse(osuPath);

        if (beatmap.columnCount != 4)
        {
            Debug.LogError($"[Spawner] Карта должна быть 4K, но найдено {beatmap.columnCount}K. Загрузка отменена.");
            return;
        }

        notes = beatmap.notes;
        arValue = beatmap.approachRate > 0 ? beatmap.approachRate : 8f;
        preempt = NoteObject.ARToPreempt(arValue);

        string audioPath = Path.Combine(
            Path.GetDirectoryName(osuPath),
            beatmap.audioFilename);

        StartCoroutine(LoadAudioAndStart(audioPath));
    }

    IEnumerator LoadAudioAndStart(string audioPath)
    {
        if (!File.Exists(audioPath))
        {
            Debug.LogError($"[Spawner] Аудио не найдено: {audioPath}");
            yield break;
        }

        AudioType audioType = GetAudioType(audioPath);
        string url = "file://" + audioPath;

        using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Spawner] Ошибка загрузки аудио: {req.error}");
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
            conductor.SetClipAndPlay(clip, beatmap.bpm, beatmap.firstBeatOffset);
            ready = true;
        }
    }

    void Update()
    {
        if (!ready) return;

        while (nextIndex < notes.Count &&
               conductor.songPosition >= notes[nextIndex].timeSeconds - preempt)
        {
            SpawnNote(notes[nextIndex]);
            nextIndex++;
        }
    }

    void SpawnNote(NoteData data)
    {
        int lane = Mathf.Clamp(data.lane, 0, laneYPositions.Length - 1);
        float y = laneYPositions[lane];

        GameObject obj = Instantiate(notePrefab);
        NoteObject note = obj.GetComponent<NoteObject>();

        note.beatTime = data.timeSeconds;
        note.endTime = data.endTimeSeconds;
        note.isLongNote = data.isLongNote;
        note.AR = arValue;

        obj.transform.position = new Vector3(10f, y, 0f);
    }

    static AudioType GetAudioType(string path)
    {
        string ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".mp3" => AudioType.MPEG,
            ".ogg" => AudioType.OGGVORBIS,
            ".wav" => AudioType.WAV,
            _ => AudioType.UNKNOWN
        };
    }
}