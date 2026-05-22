using UnityEngine;

public class Conductor : MonoBehaviour
{
    [Header("Tracking (read-only)")]
    public float songPosition;
    public float songPositionInBeats;

    public static Conductor instance;

    private AudioSource audioSource;
    private float dspSongTime;
    private float firstBeatOffset;
    private float secPerBeat;
    private float startOffset;
    private bool isRunning = false;

    private bool isPaused = false;
    private float pausedDspTime = 0f;
    private float totalPausedTime = 0f;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    public void StartWithCountdown(float countdownTime)
    {
        startOffset = -countdownTime;
        dspSongTime = (float)AudioSettings.dspTime;
        totalPausedTime = 0f;
        isRunning = true;
    }

    public void SetClipAndPlay(AudioClip clip, float bpm, float firstBeatOffset)
    {
        this.firstBeatOffset = firstBeatOffset;
        this.secPerBeat = 60f / bpm;

        audioSource.clip = clip;
        audioSource.Play();
    }

    void Update()
    {
        if (!isRunning) return;
        if (isPaused) return;

        songPosition = (float)(AudioSettings.dspTime - dspSongTime)
                       - totalPausedTime
                       + startOffset;

        songPositionInBeats = (songPosition - firstBeatOffset) / secPerBeat;
    }

    public void Pause()
    {
        if (isPaused || !isRunning) return;

        isPaused = true;
        pausedDspTime = (float)AudioSettings.dspTime;

        audioSource.Pause();
    }

    public void Resume()
    {
        if (!isPaused || !isRunning) return;

        totalPausedTime += (float)AudioSettings.dspTime - pausedDspTime;
        isPaused = false;

        audioSource.UnPause();
    }

    public bool IsPaused() => isPaused;
}