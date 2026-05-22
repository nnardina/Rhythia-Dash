using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TutorialMessage : MonoBehaviour
{
    public static TutorialMessage Instance { get; private set; }

    [Header("Message Sprite")]
    public Sprite messageSprite;

    [Header("Position Settings")]
    public Vector2 messagePosition = new Vector2(0, 200);
    public Vector2 messageSize = new Vector2(600, 300);

    [Header("Typewriter Settings")]
    [Range(0.01f, 0.2f)]
    public float characterDelay = 0.05f;
    public float delayBeforeStart = 1f;
    public float delayBetweenMessages = 2f;
    public float delayBeforeFadeOut = 3f;
    public float fadeOutDuration = 1f;

    [Header("Text Settings")]
    public TMP_FontAsset customFont;
    public int fontSize = 24;
    public Color textColor = Color.white;
    public Vector2 textPadding = new Vector2(40, 40);
    public Vector2 textOffset = new Vector2(0, 0);

    [Header("Tutorial Text Blocks")]
    public List<string> tutorialTextBlocks = new List<string>
    {
        "67",
        "42",
        "38"
    };

    private GameObject messagePanel;
    private Image panelImage;
    private TextMeshProUGUI messageText;
    private bool isTyping = false;
    private int currentBlockIndex = 0;

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
        CreateMessageUI();
    }

    private void CreateMessageUI()
    {
        if (messageSprite == null)
        {
            Debug.LogWarning("[TutorialMessage] Message sprite not assigned");
            return;
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[TutorialMessage] No Canvas found in scene");
            return;
        }

        messagePanel = new GameObject("TutorialMessagePanel");
        messagePanel.transform.SetParent(canvas.transform, false);

        panelImage = messagePanel.AddComponent<Image>();
        panelImage.sprite = messageSprite;
        panelImage.type = Image.Type.Sliced;
        panelImage.raycastTarget = false;

        RectTransform panelRect = messagePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = messagePosition;
        panelRect.sizeDelta = messageSize;

        GameObject textObj = new GameObject("MessageText");
        textObj.transform.SetParent(messagePanel.transform, false);

        messageText = textObj.AddComponent<TextMeshProUGUI>();
        
        if (customFont != null)
        {
            messageText.font = customFont;
        }
        
        messageText.fontSize = fontSize;
        messageText.color = textColor;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.enableWordWrapping = true;
        messageText.text = "";

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.offsetMin = textPadding;
        textRect.offsetMax = -textPadding;
        textRect.anchoredPosition = textOffset;

        messagePanel.SetActive(false);
    }

    public void ShowTutorialMessage()
    {
        if (messagePanel == null || messageText == null)
        {
            Debug.LogWarning("[TutorialMessage] UI not created");
            return;
        }

        if (isTyping)
            return;

        currentBlockIndex = 0;
        StartCoroutine(ShowAllTextBlocks());
    }

    private IEnumerator ShowAllTextBlocks()
    {
        isTyping = true;

        yield return new WaitForSeconds(delayBeforeStart);

        messagePanel.SetActive(true);

        for (int i = 0; i < tutorialTextBlocks.Count; i++)
        {
            currentBlockIndex = i;
            messageText.text = "";
            yield return StartCoroutine(TypewriterEffect(tutorialTextBlocks[i]));
            
            if (i < tutorialTextBlocks.Count - 1)
            {
                yield return new WaitForSeconds(delayBetweenMessages);
            }
        }

        yield return new WaitForSeconds(delayBeforeFadeOut);
        yield return StartCoroutine(FadeOutMessage());

        isTyping = false;
    }

    private IEnumerator TypewriterEffect(string text)
    {
        foreach (char c in text)
        {
            messageText.text += c;
            yield return new WaitForSeconds(characterDelay);
        }
    }

    private IEnumerator FadeOutMessage()
    {
        float elapsedTime = 0f;
        Color startPanelColor = panelImage.color;
        Color startTextColor = messageText.color;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);

            panelImage.color = new Color(startPanelColor.r, startPanelColor.g, startPanelColor.b, alpha);
            messageText.color = new Color(startTextColor.r, startTextColor.g, startTextColor.b, alpha);

            yield return null;
        }

        panelImage.color = new Color(startPanelColor.r, startPanelColor.g, startPanelColor.b, 0f);
        messageText.color = new Color(startTextColor.r, startTextColor.g, startTextColor.b, 0f);

        messagePanel.SetActive(false);

        panelImage.color = startPanelColor;
        messageText.color = startTextColor;

        if (BossHealthBar.Instance != null)
            BossHealthBar.Instance.HideTutorialBoss();
    }

    public void HideMessage()
    {
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
            
            if (panelImage != null)
            {
                Color panelColor = panelImage.color;
                panelImage.color = new Color(panelColor.r, panelColor.g, panelColor.b, 1f);
            }
        }

        if (messageText != null)
        {
            messageText.text = "";
            
            Color textColorFull = textColor;
            messageText.color = new Color(textColorFull.r, textColorFull.g, textColorFull.b, 1f);
        }

        StopAllCoroutines();
        isTyping = false;
        currentBlockIndex = 0;
    }

    public bool IsTyping() => isTyping;
    
    public int GetCurrentBlockIndex() => currentBlockIndex;
    
    public int GetTotalBlocks() => tutorialTextBlocks.Count;
}
