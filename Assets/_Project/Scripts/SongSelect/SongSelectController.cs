using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class SongData
{
    public string title;
    public string artist;
    public string mapper;
    public string difficulty;
    public int bpm;
    public float lengthSeconds;
    public Sprite coverSprite;
    public string osuFileName;
}

public class SongSelectController : MonoBehaviour
{
    [Header("Song List")]
    public Transform songListContainer;
    public TMP_InputField searchField;

    [Header("Selected Song Info")]
    public TextMeshProUGUI selectedTitleText;
    public TextMeshProUGUI selectedArtistText;
    public TextMeshProUGUI selectedMapperText;
    public TextMeshProUGUI selectedStarsText;
    public TextMeshProUGUI selectedBpmText;
    public Image selectedCoverImage;

    [Header("Navigation")]
    public Button backButton;
    public Button playButton;

    private SongData currentSelected;
    private List<SongData> songs = new List<SongData>();
    private List<GameObject> songItems = new List<GameObject>();

    private void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(OnBack);
        if (playButton != null)
            playButton.onClick.AddListener(OnPlay);
        if (searchField != null)
            searchField.onValueChanged.AddListener(OnSearch);
        ScanStreamingAssets();
    }


    private void ScanStreamingAssets()
    {
        songs.Clear();
        string rootPath = Application.streamingAssetsPath;
        string[] osuFiles = Directory.GetFiles(
            rootPath, "*.osu", SearchOption.AllDirectories);
        foreach (string filePath in osuFiles)
        {
            SongData data = ParseOsuHeader(filePath);
            if (data != null)
                songs.Add(data);
        }
        
        songs.Sort((a, b) => ExtractLevelNumber(a.difficulty).CompareTo(ExtractLevelNumber(b.difficulty)));
        
        PopulateSongList(songs);
        
        if (songs.Count > 0)
        {
            SelectSong(songs[0]);
        }
    }

    private SongData ParseOsuHeader(string fullPath)
    {
        SongData data = new SongData();
        data.osuFileName = fullPath.Replace(
            Application.streamingAssetsPath + Path.DirectorySeparatorChar, "");

        try
        {
            string[] lines = File.ReadAllLines(fullPath);
            bool inMetadata = false;
            bool inDifficulty = false;
            bool inTiming = false;
            bool inHitObjects = false;
            bool inEvents = false;
            
            float lastNoteTime = 0f;
            string backgroundImage = "";

            foreach (string raw in lines)
            {
                string line = raw.Trim();

                if (line == "[Metadata]") { inMetadata = true; inDifficulty = false; inTiming = false; inHitObjects = false; inEvents = false; continue; }
                if (line == "[Difficulty]") { inDifficulty = true; inMetadata = false; inTiming = false; inHitObjects = false; inEvents = false; continue; }
                if (line == "[TimingPoints]") { inTiming = true; inMetadata = false; inDifficulty = false; inHitObjects = false; inEvents = false; continue; }
                if (line == "[HitObjects]") { inHitObjects = true; inMetadata = false; inDifficulty = false; inTiming = false; inEvents = false; continue; }
                if (line == "[Events]") { inEvents = true; inMetadata = false; inDifficulty = false; inTiming = false; inHitObjects = false; continue; }

                if (line.StartsWith("[") && line != "[Metadata]" && line != "[Difficulty]" && line != "[TimingPoints]" && line != "[HitObjects]" && line != "[Events]")
                {
                    inMetadata = inDifficulty = inTiming = inHitObjects = inEvents = false;
                }

                if (inMetadata)
                {
                    if (line.StartsWith("Title:")) data.title = line.Substring(6).Trim();
                    else if (line.StartsWith("Artist:")) data.artist = line.Substring(7).Trim();
                    else if (line.StartsWith("Creator:")) data.mapper = line.Substring(8).Trim();
                    else if (line.StartsWith("Version:")) data.difficulty = line.Substring(8).Trim();
                }

                if (inTiming && !string.IsNullOrEmpty(line) && !line.StartsWith("["))
                {
                    string[] parts = line.Split(',');
                    if (parts.Length >= 2)
                    {
                        if (double.TryParse(parts[1],
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out double beatLength))
                        {
                            if (beatLength > 0)
                            {
                                data.bpm = Mathf.RoundToInt((float)(60000.0 / beatLength));
                                inTiming = false; 
                            }
                        }
                    }
                }
                
                if (inEvents && !string.IsNullOrEmpty(line) && !line.StartsWith("[") && !line.StartsWith("//"))
                {
                    string[] parts = line.Split(',');
                    if (parts.Length >= 3 && parts[0] == "0" && parts[1] == "0")
                    {
                        backgroundImage = parts[2].Trim().Trim('"');
                        Debug.Log($"[SongSelect] Found background in {Path.GetFileName(fullPath)}: {backgroundImage}");
                    }
                }
                
                if (inHitObjects && !string.IsNullOrEmpty(line) && !line.StartsWith("["))
                {
                    string[] parts = line.Split(',');
                    if (parts.Length >= 3)
                    {
                        if (float.TryParse(parts[2],
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out float timeMs))
                        {
                            lastNoteTime = Mathf.Max(lastNoteTime, timeMs / 1000f);
                        }
                    }
                }
            }

            data.lengthSeconds = lastNoteTime;

            if (!string.IsNullOrEmpty(backgroundImage))
            {
                string imagePath = Path.Combine(Path.GetDirectoryName(fullPath), backgroundImage);
                Debug.Log($"[SongSelect] Loading image from: {imagePath}, exists: {File.Exists(imagePath)}");
                data.coverSprite = LoadSpriteFromFile(imagePath);
                if (data.coverSprite != null)
                {
                    Debug.Log($"[SongSelect] Successfully loaded sprite for {data.title}");
                }
            }
            else
            {
                Debug.Log($"[SongSelect] No background image found for {Path.GetFileName(fullPath)}");
            }

            if (string.IsNullOrEmpty(data.title))
                data.title = Path.GetFileNameWithoutExtension(fullPath);

            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SongSelect] Ошибка парсинга {fullPath}: {e.Message}");
            return null;
        }
    }

    private Sprite LoadSpriteFromFile(string path)
    {
        if (!File.Exists(path))
            return null;

        try
        {
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(fileData))
            {
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SongSelect] Не удалось загрузить изображение {path}: {e.Message}");
        }

        return null;
    }

    private void PopulateSongList(List<SongData> data)
    {
        foreach (var item in songItems) Destroy(item);
        songItems.Clear();
        if (songListContainer == null)
        {
            Debug.LogWarning("[SongSelect] songListContainer не назначен!");
            return;
        }

        foreach (var song in data)
        {
            GameObject item = CreateSongItem(song);
            songItems.Add(item);
        }
    }

    private GameObject CreateSongItem(SongData song)
    {
        GameObject item = new GameObject($"SongItem_{song.title}");
        item.transform.SetParent(songListContainer, false);
        Image bg = item.AddComponent<Image>();
        bg.color = new Color(0.12f, 0.18f, 0.30f, 1f);
        LayoutElement le = item.AddComponent<LayoutElement>();
        le.preferredHeight = 80f;
        le.minHeight = 80f;
        le.flexibleWidth = 1f;
        Button btn = item.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.12f, 0.18f, 0.30f, 1f);
        cb.highlightedColor = new Color(0.20f, 0.35f, 0.55f, 1f);
        cb.pressedColor = new Color(0.08f, 0.12f, 0.22f, 1f);
        cb.fadeDuration = 0.1f;
        btn.colors = cb;
   
        SongData capturedSong = song;
        btn.onClick.AddListener(() => SelectSong(capturedSong));

        GameObject stripe = new GameObject("Stripe");
        stripe.transform.SetParent(item.transform, false);
        Image stripeImg = stripe.AddComponent<Image>();
        stripeImg.color = GetDifficultyColor(song.difficulty);
        RectTransform stripeRect = stripe.GetComponent<RectTransform>();
        stripeRect.anchorMin = new Vector2(0f, 0f);
        stripeRect.anchorMax = new Vector2(0.008f, 1f);
        stripeRect.offsetMin = Vector2.zero;
        stripeRect.offsetMax = Vector2.zero;

        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(item.transform, false);
        TextMeshProUGUI titleTmp = titleGO.AddComponent<TextMeshProUGUI>();
        titleTmp.text = string.IsNullOrEmpty(song.title) ? "Unknown" : song.title;
        titleTmp.fontSize = 22;
        titleTmp.color = new Color(1f, 1f, 1f, 1f);
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.overflowMode = TextOverflowModes.Ellipsis;
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.015f, 0.5f);
        titleRect.anchorMax = new Vector2(0.72f, 1f);
        titleRect.offsetMin = new Vector2(8f, 2f);
        titleRect.offsetMax = new Vector2(0f, -2f);

        GameObject artistGO = new GameObject("Artist");
        artistGO.transform.SetParent(item.transform, false);
        TextMeshProUGUI artistTmp = artistGO.AddComponent<TextMeshProUGUI>();
        artistTmp.text = string.IsNullOrEmpty(song.artist) ? "Unknown Artist" : song.artist;
        artistTmp.fontSize = 17;
        artistTmp.color = new Color(0.65f, 0.78f, 1f, 1f);
        artistTmp.overflowMode = TextOverflowModes.Ellipsis;
        RectTransform artistRect = artistGO.GetComponent<RectTransform>();
        artistRect.anchorMin = new Vector2(0.015f, 0f);
        artistRect.anchorMax = new Vector2(0.72f, 0.5f);
        artistRect.offsetMin = new Vector2(8f, 2f);
        artistRect.offsetMax = new Vector2(0f, -2f);

        Color dc = GetDifficultyColor(song.difficulty);
        GameObject badge = new GameObject("Badge");
        badge.transform.SetParent(item.transform, false);
        Image badgeBg = badge.AddComponent<Image>();
        badgeBg.color = new Color(dc.r * 0.3f, dc.g * 0.3f, dc.b * 0.3f, 1f);
        RectTransform badgeRect = badge.GetComponent<RectTransform>();
        badgeRect.anchorMin = new Vector2(0.73f, 0.15f);
        badgeRect.anchorMax = new Vector2(0.99f, 0.85f);
        badgeRect.offsetMin = Vector2.zero;
        badgeRect.offsetMax = Vector2.zero;

        GameObject badgeTxtGO = new GameObject("BadgeText");
        badgeTxtGO.transform.SetParent(badge.transform, false);
        TextMeshProUGUI badgeTmp = badgeTxtGO.AddComponent<TextMeshProUGUI>();
        badgeTmp.text = string.IsNullOrEmpty(song.difficulty) ? "?" : song.difficulty;
        badgeTmp.fontSize = 16;
        badgeTmp.color = dc;
        badgeTmp.fontStyle = FontStyles.Bold;
        badgeTmp.alignment = TextAlignmentOptions.Center;
        RectTransform badgeTxtRect = badgeTxtGO.GetComponent<RectTransform>();
        badgeTxtRect.anchorMin = Vector2.zero;
        badgeTxtRect.anchorMax = Vector2.one;
        badgeTxtRect.offsetMin = new Vector2(4f, 0f);
        badgeTxtRect.offsetMax = new Vector2(-4f, 0f);

        GameObject divider = new GameObject("Divider");
        divider.transform.SetParent(item.transform, false);
        Image divImg = divider.AddComponent<Image>();
        divImg.color = new Color(0.2f, 0.3f, 0.5f, 0.5f);
        RectTransform divRect = divider.GetComponent<RectTransform>();
        divRect.anchorMin = new Vector2(0f, 0f);
        divRect.anchorMax = new Vector2(1f, 0.02f);
        divRect.offsetMin = Vector2.zero;
        divRect.offsetMax = Vector2.zero;
        return item;
    }

    public void SelectSong(SongData song)
    {
        currentSelected = song;

        if (selectedTitleText) selectedTitleText.text = song.title;
        if (selectedArtistText) selectedArtistText.text = song.artist;
        if (selectedMapperText) selectedMapperText.text = $"Map by {song.mapper}  •  {song.difficulty}";

        if (selectedBpmText) selectedBpmText.text = $"BPM: {song.bpm}";
        
        if (selectedStarsText)
        {
            int minutes = Mathf.FloorToInt(song.lengthSeconds / 60f);
            int seconds = Mathf.FloorToInt(song.lengthSeconds % 60f);
            selectedStarsText.text = $"{minutes:D2}:{seconds:D2}";
        }

        if (selectedCoverImage)
        {
            if (song.coverSprite != null)
            {
                selectedCoverImage.sprite = song.coverSprite;
                selectedCoverImage.enabled = true;
                
                Transform parent = selectedCoverImage.transform.parent;
                if (parent != null)
                {
                    UnityEngine.UI.RectMask2D rectMask = parent.GetComponent<UnityEngine.UI.RectMask2D>();
                    if (rectMask == null)
                    {
                        rectMask = parent.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
                    }
                }
                
                RectTransform imageRect = selectedCoverImage.GetComponent<RectTransform>();
                if (imageRect != null)
                {
                    Texture2D texture = song.coverSprite.texture;
                    float imageWidth = texture.width;
                    float imageHeight = texture.height;
                    
                    float containerWidth = 500f;
                    float containerHeight = 500f;
                    
                    float scaleX = containerWidth / imageWidth;
                    float scaleY = containerHeight / imageHeight;
                    
                    float scale = Mathf.Max(scaleX, scaleY);
                    
                    float finalWidth = imageWidth * scale;
                    float finalHeight = imageHeight * scale;
                    
                    imageRect.sizeDelta = new Vector2(finalWidth, finalHeight);
                }
            }
            else
            {
                selectedCoverImage.sprite = null;
                selectedCoverImage.enabled = false;
            }
        }
    }

    private void OnSearch(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            PopulateSongList(songs);
            return;
        }

        string lower = query.ToLower();
        var filtered = songs.FindAll(s =>
            s.title.ToLower().Contains(lower) ||
            s.artist.ToLower().Contains(lower) ||
            s.difficulty.ToLower().Contains(lower));

        PopulateSongList(filtered);
    }

    private void OnBack()
    {
        if (SceneFader.Instance != null)
            SceneFader.Instance.LoadSceneWithFade("Main_Menu");
        else
            SceneManager.LoadScene("Main_Menu");
    }

    private void OnPlay()
    {
        if (currentSelected == null)
        {
            Debug.LogWarning("Карта не выбрана!");
            return;
        }

        PlayerPrefs.SetString("SelectedOsuFile", currentSelected.osuFileName);
        PlayerPrefs.Save();

        if (SceneFader.Instance != null)
            SceneFader.Instance.LoadSceneWithFade("Game");
        else
            SceneManager.LoadScene("Game");
    }

    private Color GetDifficultyColor(string diff)
    {
        if (string.IsNullOrEmpty(diff)) return new Color(0.3f, 0.5f, 1.0f);

        string lower = diff.ToLower();

        if (lower.Contains("easy")) return new Color(0.4f, 0.85f, 0.4f);
        if (lower.Contains("normal")) return new Color(0.3f, 0.5f, 1.0f);
        if (lower.Contains("hard")) return new Color(1.0f, 0.6f, 0.1f);
        if (lower.Contains("insane")) return new Color(0.9f, 0.2f, 0.5f);
        if (lower.Contains("expert")) return new Color(0.55f, 0.1f, 0.9f);

        return new Color(0.3f, 0.5f, 1.0f);
    }

    private int ExtractLevelNumber(string difficulty)
    {
        if (string.IsNullOrEmpty(difficulty))
            return int.MaxValue;

        string lower = difficulty.ToLower();
        
        if (lower.Contains("tutorial"))
            return 0;

        System.Text.RegularExpressions.Match match = 
            System.Text.RegularExpressions.Regex.Match(difficulty, @"level\s*(\d+)", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        if (match.Success && int.TryParse(match.Groups[1].Value, out int level))
            return level;

        return int.MaxValue;
    }
}