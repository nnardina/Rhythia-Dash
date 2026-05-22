using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class DefeatMenu : MonoBehaviour
{
    public static DefeatMenu Instance { get; private set; }

    [Header("UI")]
    public GameObject defeatPanel;
    public Button retryButton;
    public Button backToMenuButton;

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
        if (defeatPanel) defeatPanel.SetActive(false);

        if (retryButton)
            retryButton.onClick.AddListener(OnRetry);

        if (backToMenuButton)
            backToMenuButton.onClick.AddListener(OnBackToMenu);
    }

    public void ShowDefeat()
    {
        StartCoroutine(ShowDefeatSequence());
    }

    private IEnumerator ShowDefeatSequence()
    {
        yield return new WaitForSeconds(0.5f);

        if (defeatPanel)
        {
            defeatPanel.SetActive(true);
            yield return StartCoroutine(FadeIn(defeatPanel, 0.4f));
        }
    }

    private void OnRetry()
    {
        Time.timeScale = 1f;
        
        if (SceneFader.Instance != null)
            SceneFader.Instance.LoadSceneWithFade(SceneManager.GetActiveScene().name);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnBackToMenu()
    {
        Time.timeScale = 1f;
        
        if (SceneFader.Instance != null)
            SceneFader.Instance.LoadSceneWithFade("Song_Select");
        else
            SceneManager.LoadScene("Song_Select");
    }

    private IEnumerator FadeIn(GameObject target, float duration)
    {
        CanvasGroup cg = target.GetComponent<CanvasGroup>();
        if (cg == null) cg = target.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }
}