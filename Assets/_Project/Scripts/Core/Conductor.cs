using UnityEngine;

public class Conductor : MonoBehaviour
{
    [Header("Song Settings")]
    public float songBpm;
    public float firstBeatOffset;

    [Header("Tracking")]
    public float songPosition;
    public float songPositionInBeats;
    public float dspSongTime;

    public static Conductor instance;
    private AudioSource audioSource;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        dspSongTime = (float)AudioSettings.dspTime;
        audioSource.Play();
    }

    void Update()
    {
        songPosition = (float)(AudioSettings.dspTime - dspSongTime) - firstBeatOffset;
        if (songPosition < 0) return;
        songPositionInBeats = songPosition / (60f / songBpm);
    }
}