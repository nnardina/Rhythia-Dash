using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class ResultScreenController : MonoBehaviour
{
    [Header("Left Panel — Score")]
    public TextMeshProUGUI txtScore;
    public TextMeshProUGUI txtAcc;
    public TextMeshProUGUI txtCombo;

    [Header("Bars — PERFECT")]
    public Image barFillPerfect;
    public TextMeshProUGUI valuePerfect;

    [Header("Bars — GOOD")]
    public Image barFillGood;
    public TextMeshProUGUI valueGood;

    [Header("Bars — OK")]
    public Image barFillOk;
    public TextMeshProUGUI valueOk;

    [Header("Bars — MISS")]
    public Image barFillMiss;
    public TextMeshProUGUI valueMiss;

    [Header("Right Panel — Track Info")]
    public TextMeshProUGUI txtLine1; 
    public TextMeshProUGUI txtLine2;
    public Image coverImage; 

    [Header("Buttons")]
    public Button btnRetry;
    public Button btnMenu;

    private int score;
    private int count300;
    private int count100;
    private int count50;
    private int miss;
    private int maxCombo;
    private float accuracy;
    private string songFile;

    private void Start()
    {
        LoadResults();
        DisplayResults();

        if (btnRetry) btnRetry.onClick.AddListener(OnRetry);
        if (btnMenu) btnMenu.onClick.AddListener(OnMenu);
    }

    private void LoadResults()
    {
        score = PlayerPrefs.GetInt("Result_Score", 0);
        count300 = PlayerPrefs.GetInt("Result_Count300", 0);
        count100 = PlayerPrefs.GetInt("Result_Count100", 0);
        count50 = PlayerPrefs.GetInt("Result_Count50", 0);
        miss = PlayerPrefs.GetInt("Result_Miss", 0);
        maxCombo = PlayerPrefs.GetInt("Result_MaxCombo", 0);
        accuracy = PlayerPrefs.GetFloat("Result_Accuracy", 0f);
        songFile = PlayerPrefs.GetString("SelectedOsuFile", "");
    }

    private void DisplayResults()
    {
        if (txtScore)
            txtScore.text = score.ToString("N0");

        if (txtAcc)
            txtAcc.text = $"{accuracy:F2}%";

        if (txtCombo)
            txtCombo.text = $"{maxCombo}x";

        var totalHits = count300 + count100 + count50 + miss;
        if (totalHits == 0) totalHits = 1; 

        SetBar(barFillPerfect, valuePerfect, count300, totalHits);
        SetBar(barFillGood, valueGood, count100, totalHits);
        SetBar(barFillOk, valueOk, count50, totalHits);
        SetBar(barFillMiss, valueMiss, miss, totalHits);

        ParseAndDisplaySongInfo(songFile);
    }

    private void SetBar(Image bar, TextMeshProUGUI valueText,
                        int count, int total)
    {
        if (bar)
        {
            var fill = (float)count / total;
            bar.fillAmount = 0f;
            StartCoroutine(AnimateBar(bar, fill, 0.8f));
        }

        if (valueText)
            valueText.text = count.ToString();
    }

    private IEnumerator AnimateBar(Image bar, float targetFill, float duration)
    {
        yield return new WaitForSeconds(0.3f);
        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bar.fillAmount = Mathf.Lerp(0f, targetFill, elapsed / duration);
            yield return null;
        }
        bar.fillAmount = targetFill;
    }

    private void ParseAndDisplaySongInfo(string osuFilePath)
    {
        if (string.IsNullOrEmpty(osuFilePath))
        {
            if (txtLine1) txtLine1.text = "Unknown — Unknown";
            if (txtLine2) txtLine2.text = "Map by Unknown • Unknown";
            return;
        }

        try
        {
            string fullPath = System.IO.Path.Combine(
                Application.streamingAssetsPath, osuFilePath);

            if (!System.IO.File.Exists(fullPath))
            {
                if (txtLine1) txtLine1.text = "Unknown — Unknown";
                if (txtLine2) txtLine2.text = "Map by Unknown • Unknown";
                return;
            }

            var title = "";
            var artist = "";
            var mapper = "";
            var difficulty = "";
            var backgroundImage = "";

            string[] lines = System.IO.File.ReadAllLines(fullPath);
            var inMetadata = false;
            var inEvents = false;

            foreach (var raw in lines)
            {
                var line = raw.Trim();

                if (line == "[Metadata]") { inMetadata = true; inEvents = false; continue; }
                if (line == "[Events]") { inEvents = true; inMetadata = false; continue; }
                if (line.StartsWith("[") && line != "[Metadata]" && line != "[Events]")
                {
                    inMetadata = false;
                    inEvents = false;
                }

                if (inMetadata)
                {
                    if (line.StartsWith("Title:"))
                        title = line.Substring(6).Trim();
                    else if (line.StartsWith("Artist:"))
                        artist = line.Substring(7).Trim();
                    else if (line.StartsWith("Creator:"))
                        mapper = line.Substring(8).Trim();
                    else if (line.StartsWith("Version:"))
                        difficulty = line.Substring(8).Trim();
                }

                if (inEvents && !string.IsNullOrEmpty(line) && !line.StartsWith("[") && !line.StartsWith("//"))
                {
                    string[] parts = line.Split(',');
                    if (parts.Length >= 3 && parts[0] == "0" && parts[1] == "0")
                    {
                        backgroundImage = parts[2].Trim().Trim('"');
                    }
                }
            }

            if (txtLine1)
                txtLine1.text = $"{artist}  —  {title}";

            if (txtLine2)
                txtLine2.text = $"Map by {mapper}  •  {difficulty}";

            if (coverImage && !string.IsNullOrEmpty(backgroundImage))
            {
                string imagePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(fullPath), backgroundImage);
                Sprite sprite = LoadSpriteFromFile(imagePath);
                if (sprite != null)
                {
                    coverImage.sprite = sprite;
                    coverImage.enabled = true;
                    
                    Transform parent = coverImage.transform.parent;
                    if (parent != null)
                    {
                        UnityEngine.UI.RectMask2D rectMask = parent.GetComponent<UnityEngine.UI.RectMask2D>();
                        if (rectMask == null)
                        {
                            rectMask = parent.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ResultScreen] Ошибка парсинга: {e.Message}");
            if (txtLine1) txtLine1.text = "Unknown — Unknown";
            if (txtLine2) txtLine2.text = "Map by Unknown • Unknown";
        }
    }

    private Sprite LoadSpriteFromFile(string path)
    {
        if (!System.IO.File.Exists(path))
            return null;

        try
        {
            byte[] fileData = System.IO.File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(fileData))
            {
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[ResultScreen] Не удалось загрузить изображение {path}: {e.Message}");
        }

        return null;
    }

    private void OnRetry()
    {
        if (SceneFader.Instance != null)
            SceneFader.Instance.LoadSceneWithFade("Game");
        else
            SceneManager.LoadScene("Game");
    }

    private void OnMenu()
    {
        if (SceneFader.Instance != null)
            SceneFader.Instance.LoadSceneWithFade("Song_Select");
        else
            SceneManager.LoadScene("Song_Select");
    }
}