using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("Stats")]
    public int score;
    public int combo;
    public int maxCombo;
    public int count300;
    public int count100;
    public int count50;
    public int missCount;

    private int totalNotes;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void RegisterHit(Judgement judgement)
    {
        totalNotes++;

        switch (judgement)
        {
            case Judgement.Hit300:
                score += 300 * (combo + 1);
                count300++;
                combo++;
                break;
            case Judgement.Hit100:
                score += 100 * (combo + 1);
                count100++;
                combo++;
                break;
            case Judgement.Hit50:
                score += 50 * (combo + 1);
                count50++;
                combo++;
                break;
            case Judgement.Miss:
                missCount++;
                combo = 0;
                break;
        }

        if (combo > maxCombo) maxCombo = combo;
        Debug.Log($"[{judgement}] Score: {score} | Combo: {combo}x | Accuracy: {GetAccuracy():F2}%");
    }

    public float GetAccuracy()
    {
        if (totalNotes == 0) return 100f;
        return (float)(count300 * 300 + count100 * 100 + count50 * 50)
               / (totalNotes * 300) * 100f;
    }
}