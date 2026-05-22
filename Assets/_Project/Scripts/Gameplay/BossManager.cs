using UnityEngine;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance { get; private set; }

    [System.Serializable]
    public class BossData
    {
        public Sprite bossSprite;
        public string bossName;
    }

    [Header("Boss Types (0-4)")]
    public BossData[] bossTypes = new BossData[5];

    private int currentBossType = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetBossType(int type)
    {
        currentBossType = Mathf.Clamp(type, 0, bossTypes.Length - 1);
        Debug.Log($"[BossManager] Boss type set to: {currentBossType}");
    }

    public void ApplyBossToBossHealthBar()
    {
        if (BossHealthBar.Instance == null || BossHealthBar.Instance.bossImage == null)
        {
            Debug.LogWarning("[BossManager] BossHealthBar or bossImage not found");
            return;
        }

        if (currentBossType < 0 || currentBossType >= bossTypes.Length)
        {
            Debug.LogWarning($"[BossManager] Invalid boss type: {currentBossType}");
            return;
        }

        var bossData = bossTypes[currentBossType];
        if (bossData.bossSprite != null)
        {
            BossHealthBar.Instance.bossImage.sprite = bossData.bossSprite;
            Debug.Log($"[BossManager] Applied boss sprite: {bossData.bossName}");
        }
        else
        {
            Debug.LogWarning($"[BossManager] Boss sprite for type {currentBossType} is null");
        }
    }

    public int GetCurrentBossType() => currentBossType;
}
