using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameResultManager : MonoBehaviour
{
    public static GameResultManager Instance { get; private set; }

    [Header("Задержка после последней ноты (сек)")]
    public float endDelay = 2.0f;

    private bool levelEnded = false;
    private bool checking = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (levelEnded || checking) return;
        if (Spawner.instance == null) return;

        var allSpawned = Spawner.instance.AllNotesSpawned();
        var allGone = NoteObject.All.Count == 0;
    
        if (allSpawned && allGone)
        {
            checking = true;
            StartCoroutine(EndLevelSequence());
        }
    }

    private IEnumerator EndLevelSequence()
    {
        yield return new WaitForSeconds(endDelay);
        levelEnded = true;
        DebuffManager.Instance?.StopPostDeathDebuffs();
        var bossAlive = BossHealthBar.Instance != null
                         && !BossHealthBar.Instance.IsDead();
        if (bossAlive)
            DefeatMenu.Instance?.ShowDefeat();
        else
            SaveResultAndGoToScreen();
    }

    private void SaveResultAndGoToScreen()
    {
        if (ScoreManager.instance == null) return;

        PlayerPrefs.SetInt("Result_Score", ScoreManager.instance.score);
        PlayerPrefs.SetInt("Result_Count300", ScoreManager.instance.count300);
        PlayerPrefs.SetInt("Result_Count100", ScoreManager.instance.count100);
        PlayerPrefs.SetInt("Result_Count50", ScoreManager.instance.count50);
        PlayerPrefs.SetInt("Result_Miss", ScoreManager.instance.missCount);
        PlayerPrefs.SetInt("Result_MaxCombo", ScoreManager.instance.maxCombo);
        PlayerPrefs.SetFloat("Result_Accuracy",
            ScoreManager.instance.GetAccuracy());
        PlayerPrefs.SetString("Result_SongName",
            PlayerPrefs.GetString("SelectedOsuFile", "Unknown"));

        PlayerPrefs.Save();

        SceneManager.LoadScene("Result_Screen");
    }

}