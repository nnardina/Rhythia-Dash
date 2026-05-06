using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class OsuBeatmap
{
    public string audioFilename      = "";
    public float  approachRate       = 8f;
    public float  overallDifficulty  = 8f;
    public int    columnCount        = 4;
    public float  bpm                = 120f;
    public float  firstBeatOffset    = 0f;
    public List<NoteData> notes      = new List<NoteData>();
}

public struct NoteData
{
    public float timeSeconds;
    public float endTimeSeconds;
    public int   lane;
    public bool  isLongNote;
}

public static class OsuParser
{
    public static OsuBeatmap Parse(string filePath)
    {
        var beatmap = new OsuBeatmap();

        if (!File.Exists(filePath))
        {
            Debug.LogError($"[OsuParser] File not found: {filePath}");
            return beatmap;
        }

        string[] lines   = File.ReadAllLines(filePath);
        string   section = "";
        bool     firstTimingPointFound = false;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                section = line;
                continue;
            }

            if (string.IsNullOrEmpty(line) || line.StartsWith("//"))
                continue;

            switch (section)
            {
                case "[General]":
                    if (line.StartsWith("AudioFilename:"))
                        beatmap.audioFilename = Split(line);
                    else if (line.StartsWith("Mode:"))
                    {
                        int mode = int.Parse(Split(line));
                        if (mode != 3)
                            Debug.LogWarning($"[OsuParser] Ожидался Mode:3 (mania), получен Mode:{mode}");
                    }
                    break;

                case "[Difficulty]":
                    if      (line.StartsWith("ApproachRate:"))
                        beatmap.approachRate = ParseFloat(Split(line));
                    else if (line.StartsWith("OverallDifficulty:"))
                        beatmap.overallDifficulty = ParseFloat(Split(line));
                    else if (line.StartsWith("CircleSize:"))
                        beatmap.columnCount = (int)ParseFloat(Split(line));
                    break;

                case "[TimingPoints]":
                    if (!firstTimingPointFound)
                        ParseFirstTimingPoint(line, beatmap, ref firstTimingPointFound);
                    break;

                case "[HitObjects]":
                    ParseHitObject(line, beatmap);
                    break;
            }
        }

        beatmap.notes.Sort((a, b) => a.timeSeconds.CompareTo(b.timeSeconds));

        int lnCount = beatmap.notes.FindAll(n => n.isLongNote).Count;
        Debug.Log($"[OsuParser] Загружено {beatmap.notes.Count} нот ({lnCount} LN) | " +
                  $"Columns={beatmap.columnCount} | BPM={beatmap.bpm:F1} | " +
                  $"AR={beatmap.approachRate} | OD={beatmap.overallDifficulty}");

        return beatmap;
    }

    static void ParseFirstTimingPoint(string line, OsuBeatmap beatmap, ref bool found)
    {
        string[] parts = line.Split(',');
        if (parts.Length < 8) return;
        if (parts[6].Trim() != "1") return;

        if (!float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float offsetMs)) return;
        if (!float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float msPerBeat)) return;

        beatmap.firstBeatOffset = offsetMs / 1000f;
        beatmap.bpm             = 60000f / msPerBeat;
        found = true;
    }

    static void ParseHitObject(string line, OsuBeatmap beatmap)
    {
        string[] parts = line.Split(',');
        if (parts.Length < 5) return;

        if (!int.TryParse(parts[0].Trim(), out int x))     return;
        if (!float.TryParse(parts[2].Trim(),
                NumberStyles.Float, CultureInfo.InvariantCulture,
                out float timeMs)) return;
        if (!int.TryParse(parts[3].Trim(), out int type))  return;

        bool isLongNote = (type & 128) != 0;

        float endTimeMs = 0f;
        if (isLongNote && parts.Length >= 6)
        {
            string endPart = parts[5].Trim().Split(':')[0];
            float.TryParse(endPart, NumberStyles.Float, CultureInfo.InvariantCulture, out endTimeMs);
        }

        int lane = Mathf.Clamp(
            Mathf.FloorToInt(x * beatmap.columnCount / 512f),
            0, beatmap.columnCount - 1);

        beatmap.notes.Add(new NoteData
        {
            timeSeconds    = timeMs    / 1000f,
            endTimeSeconds = endTimeMs / 1000f,
            lane           = lane,
            isLongNote     = isLongNote
        });
    }

    static string Split(string line) =>
        line.Substring(line.IndexOf(':') + 1).Trim();

    static float ParseFloat(string s) =>
        float.Parse(s, CultureInfo.InvariantCulture);
}