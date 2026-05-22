using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    [Header("UI")]
    public GameObject pausePanel;
    public Button retryButton;
    public Button menuButton;
    public Button resumeButton;

    [Header("Settings")]
    public KeyCode pauseKey = KeyCode.Escape;

    private bool isPaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (pausePanel) pausePanel.SetActive(false);
        if (retryButton) retryButton.onClick.AddListener(OnRetry);
        if (menuButton) menuButton.onClick.AddListener(OnMenu);
        if (resumeButton) resumeButton.onClick.AddListener(OnResume);
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused) OnResume();
            else OnPause();
        }
    }


    public void OnPause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (Conductor.instance != null)
            Conductor.instance.Pause();

        if (pausePanel) pausePanel.SetActive(true);
    }

    public void OnResume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (Conductor.instance != null)
            Conductor.instance.Resume();

        if (pausePanel) pausePanel.SetActive(false);
    }

    public void OnRetry()
    {
        Time.timeScale = 1f;
        var current = SceneManager.GetActiveScene().name;
        
        if (SceneFader.Instance != null)
            SceneFader.Instance.LoadSceneWithFade(current);
        else
            SceneManager.LoadScene(current);
    }

    public void OnMenu()
    {
        Time.timeScale = 1f;
        
        if (SceneFader.Instance != null)
            SceneFader.Instance.LoadSceneWithFade("Song_Select");
        else
            SceneManager.LoadScene("Song_Select");
    }

    public bool IsPaused() => isPaused;
}