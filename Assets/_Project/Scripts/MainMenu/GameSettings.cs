using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    public float ScrollSpeed { get; private set; } = 1f;
    public float DamageDecrease { get; private set; } = 1f;
    public KeyCode[] LaneKeys { get; private set; } =
    {
        KeyCode.A,
        KeyCode.S,
        KeyCode.D,
        KeyCode.F
    };

    private const string KEY_SCROLL_SPEED = "ScrollSpeed";
    private const string KEY_DAMAGE_DECREASE = "DamageDecrease";
    private const string KEY_LANE_0 = "LaneKey0";
    private const string KEY_LANE_1 = "LaneKey1";
    private const string KEY_LANE_2 = "LaneKey2";
    private const string KEY_LANE_3 = "LaneKey3";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public void SetScrollSpeed(float value)
    {
        ScrollSpeed = Mathf.Clamp(value, 0.5f, 3f);
    }

    public void SetDamageDeacrease(float value)
    {
        DamageDecrease = Mathf.Clamp(value, 0.5f, 3f);
    }

    public void SetLaneKey(int lane, KeyCode key)
    {
        if (lane < 0 || lane >= LaneKeys.Length) return;
        LaneKeys[lane] = key;
    }

    public void Save()
    {
        PlayerPrefs.SetFloat(KEY_SCROLL_SPEED, ScrollSpeed);
        PlayerPrefs.SetFloat(KEY_DAMAGE_DECREASE, DamageDecrease);
        PlayerPrefs.SetInt(KEY_LANE_0, (int)LaneKeys[0]);
        PlayerPrefs.SetInt(KEY_LANE_1, (int)LaneKeys[1]);
        PlayerPrefs.SetInt(KEY_LANE_2, (int)LaneKeys[2]);
        PlayerPrefs.SetInt(KEY_LANE_3, (int)LaneKeys[3]);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        ScrollSpeed = PlayerPrefs.GetFloat(KEY_SCROLL_SPEED, 1f);
        DamageDecrease = PlayerPrefs.GetFloat(KEY_DAMAGE_DECREASE, 1f);
        LaneKeys[0] = (KeyCode)PlayerPrefs.GetInt(KEY_LANE_0, (int)KeyCode.A);
        LaneKeys[1] = (KeyCode)PlayerPrefs.GetInt(KEY_LANE_1, (int)KeyCode.S);
        LaneKeys[2] = (KeyCode)PlayerPrefs.GetInt(KEY_LANE_2, (int)KeyCode.D);
        LaneKeys[3] = (KeyCode)PlayerPrefs.GetInt(KEY_LANE_3, (int)KeyCode.F);
    }
}