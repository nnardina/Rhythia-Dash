using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum DebuffType { Flash }

public class DebuffManager : MonoBehaviour
{
    public static DebuffManager Instance { get; private set; }

    [Header("Длительность (сек)")]
    public float flashDuration = 8f;

    [Header("Ноты видны ниже этой Y")]
    public float flashVisibleY = -1.0f;

    private bool debuffActive = false;
    private bool flashActive = false;

    private readonly int[] debuffSegments = { 0, 2, 4 };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void OnSegmentDestroyed(int segmentIndex)
    {
        var shouldDebuff = false;
        foreach (int i in debuffSegments)
            if (i == segmentIndex) { shouldDebuff = true; break; }

        if (!shouldDebuff || debuffActive) return;

        StartCoroutine(ApplyFlash());
    }

    private IEnumerator ApplyFlash()
    {
        debuffActive = true;
        flashActive = true;

        yield return new WaitForSeconds(flashDuration);

        flashActive = false;
        debuffActive = false;
    }

    public bool ShouldHideNote(float worldY)
    {
        return flashActive && worldY >= flashVisibleY;
    }

    public bool IsFlashActive() => flashActive;
    public bool IsDebuffActive() => debuffActive;
}