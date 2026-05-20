using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
    public GameObject panelRoot;
    public Slider scrollSpeedSlider;
    public TextMeshProUGUI scrollSpeedLabel;
    public Button[] keyBindButtons;
    public TextMeshProUGUI[] keyBindLabels;
    public Button saveButton;
    public Button closeButton;

    private int listeningLane = -1;
    private KeyCode[] tempKeys = new KeyCode[4];
    private float tempScrollSpeed;

    private void Start()
    {
        // Назначаем слушателей в Start, чтобы всё успело прогрузиться
        for (int i = 0; i < keyBindButtons.Length; i++)
        {
            int lane = i;
            keyBindButtons[i].onClick.RemoveAllListeners(); // На всякий случай чистим
            keyBindButtons[i].onClick.AddListener(() => StartListening(lane));
        }

        scrollSpeedSlider.onValueChanged.RemoveAllListeners();
        scrollSpeedSlider.onValueChanged.AddListener(OnSliderChanged);

        saveButton.onClick.RemoveAllListeners();
        saveButton.onClick.AddListener(OnSave);

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(OnClose);
    }

    public void Open()
    {
        if (GameSettings.Instance == null) return;

        // 1. Сначала подгружаем данные
        tempScrollSpeed = GameSettings.Instance.ScrollSpeed;
        for (int i = 0; i < 4; i++)
            tempKeys[i] = GameSettings.Instance.LaneKeys[i];

        // 2. Активируем объект
        panelRoot.SetActive(true);

        // 3. Сразу обновляем визуальную часть
        listeningLane = -1;
        RefreshUI();
    }

    private void RefreshUI()
    {
        scrollSpeedSlider.SetValueWithoutNotify(tempScrollSpeed);
        scrollSpeedLabel.text = $"Scroll Speed: {tempScrollSpeed:F1}x";

        for (int i = 0; i < keyBindLabels.Length; i++)
        {
            keyBindLabels[i].text = tempKeys[i].ToString();
            SetButtonColor(i, new Color(0.1f, 0.15f, 0.3f));
        }
    }

    private void OnSliderChanged(float v)
    {
        tempScrollSpeed = Mathf.Round(v * 10f) / 10f;
        scrollSpeedLabel.text = $"Scroll Speed: {tempScrollSpeed:F1}x";
    }

    private void StartListening(int lane)
    {
        if (listeningLane >= 0) SetButtonColor(listeningLane, new Color(0.1f, 0.15f, 0.3f));
        listeningLane = lane;
        keyBindLabels[lane].text = "???"; // Нагляднее, что ждем нажатия
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
        for (int i = 0; i < 4; i++) GameSettings.Instance.SetLaneKey(i, tempKeys[i]);
        GameSettings.Instance.Save();
        panelRoot.SetActive(false);
    }

    private void OnClose() => panelRoot.SetActive(false);
}