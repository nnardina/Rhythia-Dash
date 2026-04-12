using UnityEngine;

public class NoteObject : MonoBehaviour
{
    public float beatTime;
    public float AR = 1.0f;

    private float startX = 10f;
    private float targetX = -7f;

    void Update()
    {
        float songPos = Conductor.instance.songPosition;
        float spawnTime = beatTime - AR;
        float progress = (songPos - spawnTime) / AR;

        transform.position = new Vector3(
            Mathf.Lerp(startX, targetX, progress),
            transform.position.y,
            0
        );

        if (progress > 1.5f) Destroy(gameObject);
    }
}