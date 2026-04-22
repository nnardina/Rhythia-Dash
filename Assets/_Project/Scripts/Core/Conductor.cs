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

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    public void SetClipAndPlay(AudioClip clip, float bpm, float firstBeatOffset)
    {
        this.firstBeatOffset = firstBeatOffset;
        this.secPerBeat      = 60f / bpm;

        audioSource.clip = clip;
        dspSongTime      = (float)AudioSettings.dspTime;
        audioSource.Play();
    }

    void Update()
    {
        if (!audioSource.isPlaying) return;

        songPosition        = (float)(AudioSettings.dspTime - dspSongTime);
        songPositionInBeats = (songPosition - firstBeatOffset) / secPerBeat;
    }
}