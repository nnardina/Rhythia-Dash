using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
    public GameObject panelRoot;
    public Slider scrollSpeedSlider;
    public Slider damageDecreaseSlider;
    public TextMeshProUGUI scrollSpeedLabel;
    public TextMeshProUGUI damageDecreaseLabel;
    public Button[] keyBindButtons;
    public TextMeshProUGUI[] keyBindLabels;
    public Button saveButton;
    public Button closeButton;

    private int listeningLane = -1;
    private KeyCode[] tempKeys = new KeyCode[4];
    private float tempScrollSpeed;
    private float tempDamageDecrease;

    private void Start()
    {
        for (int i = 0; i < keyBindButtons.Length; i++)
        {
            int lane = i;
            keyBindButtons[i].onClick.RemoveAllListeners();
            keyBindButtons[i].onClick.AddListener(() => StartListening(lane));
        }

        scrollSpeedSlider.onValueChanged.RemoveAllListeners();
        scrollSpeedSlider.onValueChanged.AddListener(OnSpeedSliderChanged);

        damageDecreaseSlider.onValueChanged.RemoveAllListeners();
        damageDecreaseSlider.onValueChanged.AddListener(OnDamageSliderChanged);

        saveButton.onClick.RemoveAllListeners();
        saveButton.onClick.AddListener(OnSave);

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(OnClose);
    }

    public void Open()
    {
        if (GameSettings.Instance == null) return;

        tempScrollSpeed = GameSettings.Instance.ScrollSpeed;
        tempDamageDecrease = GameSettings.Instance.DamageDecrease;
        for (int i = 0; i < 4; i++)
            tempKeys[i] = GameSettings.Instance.LaneKeys[i];

        panelRoot.SetActive(true);


        listeningLane = -1;
        RefreshUI();
    }

    private void RefreshUI()
    {
        scrollSpeedSlider.SetValueWithoutNotify(tempScrollSpeed);
        scrollSpeedLabel.text = $"Scroll Speed: {tempScrollSpeed:F1}x";

        damageDecreaseSlider.SetValueWithoutNotify(tempDamageDecrease);
        damageDecreaseLabel.text = $"Damage Decrease: {tempDamageDecrease:F1}x";

        for (int i = 0; i < keyBindLabels.Length; i++)
        {
            keyBindLabels[i].text = tempKeys[i].ToString();
            SetButtonColor(i, new Color(0.1f, 0.15f, 0.3f));
        }
    }

    private void OnSpeedSliderChanged(float v)
    {
        tempScrollSpeed = Mathf.Round(v * 10f) / 10f;
        scrollSpeedLabel.text = $"Scroll Speed: {tempScrollSpeed:F1}x";
    }

    private void OnDamageSliderChanged(float v)
    {
        tempDamageDecrease = Mathf.Round(v * 10f) / 10f;
        damageDecreaseLabel.text = $"Damage Decrease: {tempDamageDecrease:F1}x";
    }

    private void StartListening(int lane)
    {
        if (listeningLane >= 0) SetButtonColor(listeningLane, new Color(0.1f, 0.15f, 0.3f));
        listeningLane = lane;
        keyBindLabels[lane].text = "???";
        SetButtonColor(lane, new Color(1f, 0.8f, 0f));
    }

    private void Update()
    {
        if (listeningLane < 0 || !panelRoot.activeSelf) return;

        if (Input.anyKeyDown)
        {
            foreach (KeyCode kc in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (kc == KeyCode.None || kc >= KeyCode.JoystickButton0) continue;
                if (kc >= KeyCode.Mouse0 && kc <= KeyCode.Mouse6) continue;

                if (Input.GetKeyDown(kc))
                {
                    tempKeys[listeningLane] = kc;
                    keyBindLabels[listeningLane].text = kc.ToString();
                    SetButtonColor(listeningLane, new Color(0.1f, 0.15f, 0.3f));
                    listeningLane = -1;
                    break;
                }
            }
        }
    }

    private void SetButtonColor(int i, Color c)
    {
        keyBindButtons[i].GetComponent<Image>().color = c;
    }

    private void OnSave()
    {
        GameSettings.Instance.SetScrollSpeed(tempScrollSpeed);
        GameSettings.Instance.SetDamageDeacrease(tempDamageDecrease);
        for (int i = 0; i < 4; i++) GameSettings.Instance.SetLaneKey(i, tempKeys[i]);
        GameSettings.Instance.Save();
        panelRoot.SetActive(false);
    }

    private void OnClose() => panelRoot.SetActive(false);
}