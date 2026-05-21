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
    public CountdownManager countdownManager;

    [Header("Countdown")]
    public float countdownDuration = 2f;

    [Header("Sprites Outer (lanes 1 & 4)")]
    public Sprite outerHead;
    public Sprite outerBody;
    public Sprite outerTail;

    [Header("Sprites Inner (lanes 2 & 3)")]
    public Sprite innerHead;
    public Sprite innerBody;
    public Sprite innerTail;

    [Header("Map")]
    [Tooltip("��� ����� .osu � ����� StreamingAssets (��������: map.osu)")]
    public string osuFileName = "map.osu";

    [Header("Tutorial Settings")]
    [Tooltip("Задержка перед началом спавна нот (для туториала)")]
    public float noteSpawnDelay = 0f;

    [Header("Lanes")]
    public float[] laneXPositions = { -7f, -5f, -3f, -1f };

    [Header("Debug")]
    [Tooltip("0 = ����� �� �����, ����� ��������������")]
    public float arOverride = 0f;

    private List<NoteData> notes = new List<NoteData>();
    private int nextIndex = 0;
    private float arValue;
    private float preempt;
    private bool ready = false;
    private OsuBeatmap beatmap;

    public static Spawner instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        string selectedFile = PlayerPrefs.GetString("SelectedOsuFile", "");

        if (!string.IsNullOrEmpty(selectedFile))
        {
            osuFileName = selectedFile;
        }
        else
        {

            Debug.Log("Файл пуст");
        }

        string osuPath = Path.Combine(Application.streamingAssetsPath, osuFileName);

        if (!File.Exists(osuPath))
        {
            Debug.LogError($"[Spawner] .osu файл не найден: {osuPath}");
            return;
        }

        beatmap = OsuParser.Parse(osuPath);

        if (beatmap.columnCount != 4)
        {
            Debug.LogError(
                $"[Spawner] Карта должна быть 4K, найдено {beatmap.columnCount}K");
            return;
        }

        notes = beatmap.notes;
        arValue = beatmap.approachRate > 0 ? beatmap.approachRate : 8f;
        preempt = NoteObject.ARToPreempt(arValue);

        HitDetector[] detectors = FindObjectsOfType<HitDetector>();
        foreach (var detector in detectors)
        {
            detector.OD = beatmap.overallDifficulty;
        }

        string audioPath = Path.Combine(
            Path.GetDirectoryName(osuPath),
            beatmap.audioFilename);

        StartCoroutine(LoadAudioAndStart(audioPath));
    }

    IEnumerator LoadAudioAndStart(string audioPath)
    {
        if (!File.Exists(audioPath))
        {
            Debug.LogError($"[Spawner] ����� �� �������: {audioPath}");
            yield break;
        }

        AudioType audioType = GetAudioType(audioPath);
        string url = "file://" + audioPath;

        using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Spawner] ������ �������� �����: {req.error}");
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(req);

            if (BossManager.Instance != null)
            {
                BossManager.Instance.SetBossType(beatmap.bossType);
                BossManager.Instance.ApplyBossToBossHealthBar();
            }

            if (BossHealthBar.Instance != null)
                BossHealthBar.Instance.InitFromNoteCount(notes.Count);

            bool isTutorial = beatmap.bossType == 0;
            if (isTutorial)
            {
                if (BossHealthBar.Instance != null)
                    BossHealthBar.Instance.SetTutorialMode(true);

                if (TutorialMessage.Instance != null)
                {
                    TutorialMessage.Instance.ShowTutorialMessage();
                    noteSpawnDelay = 10f;
                }
            }

            conductor.StartWithCountdown(preempt);

            ready = true;

            if (countdownManager != null)
            {
                StartCoroutine(countdownManager.StartCountdown(arValue));
                yield return new WaitUntil(() => countdownManager.IsCountdownFinished());
            }

            conductor.SetClipAndPlay(clip, beatmap.bpm, beatmap.firstBeatOffset);
        }
    }

    void Update()
    {
        if (!ready) return;

        while (nextIndex < notes.Count &&
               conductor.songPosition >= notes[nextIndex].timeSeconds - preempt)
        {
            if (notes[nextIndex].timeSeconds >= noteSpawnDelay)
            {
                SpawnNote(notes[nextIndex]);
            }
            nextIndex++;
        }
    }

    void SpawnNote(NoteData data)
    {
        int lane = Mathf.Clamp(data.lane, 0, laneXPositions.Length - 1);
        float x = laneXPositions[lane];

        float scrollMult = GameSettings.Instance != null
            ? GameSettings.Instance.ScrollSpeed
            : 1f;
        float startY = 10f * scrollMult;

        GameObject obj = Instantiate(notePrefab);
        NoteObject note = obj.GetComponent<NoteObject>();

        note.beatTime = data.timeSeconds;
        note.endTime = data.endTimeSeconds;
        note.isLongNote = data.isLongNote;
        note.AR = arValue;
        note.OD = beatmap.overallDifficulty;

        bool isOuter = (lane == 0 || lane == 3);

        Sprite head = isOuter ? outerHead : innerHead;
        Sprite body = isOuter ? outerBody : innerBody;
        Sprite tail = isOuter ? outerTail : innerTail;

        var headSr = obj.GetComponent<SpriteRenderer>();
        if (headSr != null && head != null) headSr.sprite = head;

        var bodyT = obj.transform.Find("Body");
        if (bodyT != null && body != null)
        {
            var bodySr = bodyT.GetComponent<SpriteRenderer>();
            if (bodySr != null) bodySr.sprite = body;
        }

        var tailT = obj.transform.Find("Tail");
        if (tailT != null && tail != null)
        {
            var tailSr = tailT.GetComponent<SpriteRenderer>();
            if (tailSr != null)
            {
                tailSr.sprite = tail;
                tailSr.flipY = true;
            }
        }

        obj.transform.position = new Vector3(x, startY, 0f);
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

    public bool AllNotesSpawned()
    {
        return ready && nextIndex >= notes.Count;
    }
}