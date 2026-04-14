using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("Stats")]
    public int score;
    public int combo;
    public int maxCombo;
    public int perfectCount;
    public int greatCount;
    public int goodCount;
    public int okCount;
    public int mehCount;
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
            case Judgement.Perfect:
                score += 320 * combo;
                perfectCount++;
                combo++;
                break;
            case Judgement.Great:
                score += 300 * combo;
                greatCount++;
                combo++;
                break;
            case Judgement.Good:
                score += 200 * combo;
                goodCount++;
                combo++;
                break;
            case Judgement.Ok:
                score += 100 * combo;
                okCount++;
                combo++;
                break;
            case Judgement.Meh:
                score += 50 * combo;
                mehCount++;
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
        return (float)(perfectCount * 320 + greatCount * 300 + goodCount * 200 + okCount * 100 + mehCount * 50)
               / (totalNotes * 320) * 100f;
    }
}