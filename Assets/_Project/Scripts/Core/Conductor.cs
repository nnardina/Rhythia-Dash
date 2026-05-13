using UnityEngine;

public class Conductor : MonoBehaviour
{
    [Header("Tracking (read-only)")]
    public float songPosition;
    public float songPositionInBeats;

    public static Conductor instance;

    private AudioSource audioSource;
    private float       dspSongTime;
    private float       firstBeatOffset;
    private float       secPerBeat;
    private float       startOffset;
    private bool        isRunning = false;

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
        isRunning = true;
    }

    public void SetClipAndPlay(AudioClip clip, float bpm, float firstBeatOffset)
    {
        this.firstBeatOffset = firstBeatOffset;
        this.secPerBeat      = 60f / bpm;

        audioSource.clip = clip;
        audioSource.Play();
    }

    void Update()
    {
        if (!isRunning) return;

        songPosition = (float)(AudioSettings.dspTime - dspSongTime) + startOffset;
        songPositionInBeats = (songPosition - firstBeatOffset) / secPerBeat;
    }
}