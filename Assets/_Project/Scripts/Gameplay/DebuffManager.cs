using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum DebuffType { Flash, BarrelRoll, MetalPipe, MotionBlur }

public class DebuffManager : MonoBehaviour
{
    public static DebuffManager Instance { get; private set; }

    [Header("=== FLASH ===")]
    public Image csFlashImage;
    public AudioSource fxAudioSource;
    public AudioClip csFlashSound;
    public float csFlashFadeIn = 0.08f;
    public float csFlashFadeOut = 2.5f;

    [Header("=== BARREL ROLL ===")]
    public Camera gameCamera;
    public float barrelRollDuration = 1.2f;

    [Header("=== METAL PIPE ===")]
    public float metalPipeDuration = 10f;

    [Header("=== MOTION BLUR ===")]
    public SimpleMotionBlur motionBlur;
    public float motionBlurDuration = 8f;
    [Range(0f, 0.99f)]
    public float motionBlurIntensity = 0.99f;

    [Header("=== SHAKE ===")]
    public float shakeAmount = 0.10f;
    public float shakeDuration = 0.08f;

    [Header("=== AFTER BOSS DEATH ===")]
    public float debuffInterval = 20f;

    private bool flashActive = false;
    private bool metalPipeActive = false;
    private bool rollActive = false;
    private bool motionBlurActive = false;

    private Vector3 cameraBasePosition;
    private Quaternion cameraBaseRotation;

    private Coroutine flashCoroutine;
    private Coroutine rollCoroutine;
    private Coroutine metalPipeCoroutine;
    private Coroutine motionBlurCoroutine;
    private Coroutine shakeCoroutine;
    private Coroutine postDeathCoroutine;

    private DebuffType? lastDebuff = null;

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
        if (gameCamera == null)
            gameCamera = Camera.main;

        if (gameCamera != null)
        {
            cameraBasePosition = gameCamera.transform.position;
            cameraBaseRotation = gameCamera.transform.rotation;
        }

        if (csFlashImage != null)
        {
            csFlashImage.color = new Color(1f, 1f, 1f, 0f);
            csFlashImage.gameObject.SetActive(false);
        }

        if (motionBlur != null)
            motionBlur.enabled = false;

        if (BossHealthBar.Instance != null)
            BossHealthBar.Instance.OnBossDeath += OnBossDied;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("[TEST] Motion blur toggle");
            if (motionBlur != null)
            {
                motionBlur.enabled = !motionBlur.enabled;
                Debug.Log($"MotionBlur enabled: {motionBlur.enabled}");
            }
            else
            {
                Debug.LogError("motionBlur == null! Не назначен в Inspector");
            }
        }
    }

    public void OnSegmentDestroyed(int segmentIndex)
    {
        ApplyRandomDebuff();
    }

    private void OnBossDied()
    {
        if (postDeathCoroutine != null)
            StopCoroutine(postDeathCoroutine);
        postDeathCoroutine = StartCoroutine(PostDeathDebuffLoop());
    }

    public void StopPostDeathDebuffs()
    {
        if (postDeathCoroutine != null)
        {
            StopCoroutine(postDeathCoroutine);
            postDeathCoroutine = null;
        }
    }

    public void OnKeyPressed()
    {
        if (!metalPipeActive) return;
        if (gameCamera == null) return;

        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeCamera());
    }

    private IEnumerator PostDeathDebuffLoop()
    {
        Debug.Log("[DebuffManager] Post-death debuff loop started");
        while (true)
        {
            yield return new WaitForSeconds(debuffInterval);
            ApplyRandomDebuff();
        }
    }

    private void ApplyRandomDebuff()
    {
        DebuffType chosen = GetRandomDebuff();
        lastDebuff = chosen;

        Debug.Log($"[DebuffManager] Random debuff: {chosen}");

        switch (chosen)
        {
            case DebuffType.Flash:
                if (flashCoroutine != null) StopCoroutine(flashCoroutine);
                flashCoroutine = StartCoroutine(ApplyFlash());
                break;

            case DebuffType.BarrelRoll:
                if (rollCoroutine != null) StopCoroutine(rollCoroutine);
                rollCoroutine = StartCoroutine(ApplyBarrelRoll());
                break;

            case DebuffType.MetalPipe:
                if (metalPipeCoroutine != null) StopCoroutine(metalPipeCoroutine);
                metalPipeCoroutine = StartCoroutine(ApplyMetalPipe());
                break;

            case DebuffType.MotionBlur:
                if (motionBlurCoroutine != null) StopCoroutine(motionBlurCoroutine);
                motionBlurCoroutine = StartCoroutine(ApplyMotionBlur());
                break;
        }
    }

    private DebuffType GetRandomDebuff()
    {
        DebuffType[] all =
        {
            DebuffType.Flash,
            DebuffType.BarrelRoll,
            DebuffType.MetalPipe,
            DebuffType.MotionBlur
        };

        if (lastDebuff == null)
            return all[Random.Range(0, all.Length)];

        DebuffType chosen;
        do { chosen = all[Random.Range(0, all.Length)]; }
        while (chosen == lastDebuff.Value);

        return chosen;
    }

 
    private IEnumerator ApplyFlash()
    {
        flashActive = true;

        if (fxAudioSource != null && csFlashSound != null)
            fxAudioSource.PlayOneShot(csFlashSound);

        if (csFlashImage != null)
        {
            csFlashImage.gameObject.SetActive(true);
            float t = 0f;
            while (t < csFlashFadeIn)
            {
                t += Time.deltaTime;
                csFlashImage.color = new Color(1f, 1f, 1f, Mathf.Clamp01(t / csFlashFadeIn));
                yield return null;
            }
            csFlashImage.color = new Color(1f, 1f, 1f, 1f);

            t = 0f;
            while (t < csFlashFadeOut)
            {
                t += Time.deltaTime;
                csFlashImage.color = new Color(1f, 1f, 1f, Mathf.Clamp01(1f - t / csFlashFadeOut));
                yield return null;
            }
            csFlashImage.color = new Color(1f, 1f, 1f, 0f);
            csFlashImage.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(csFlashFadeIn + csFlashFadeOut);
        }

        flashActive = false;
        flashCoroutine = null;
    }

    private IEnumerator ApplyBarrelRoll()
    {
        if (gameCamera == null) yield break;

        rollActive = true;
        float elapsed = 0f;
        float startZ = cameraBaseRotation.eulerAngles.z;

        while (elapsed < barrelRollDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / barrelRollDuration);
            float smooth = t < 0.5f
                ? 4f * t * t * t
                : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;

            gameCamera.transform.rotation = Quaternion.Euler(0f, 0f, startZ + Mathf.Lerp(0f, 360f, smooth));
            yield return null;
        }

        gameCamera.transform.rotation = cameraBaseRotation;
        rollActive = false;
        rollCoroutine = null;
    }

    private IEnumerator ApplyMetalPipe()
    {
        metalPipeActive = true;
        yield return new WaitForSeconds(metalPipeDuration);
        metalPipeActive = false;
        metalPipeCoroutine = null;
    }

    private IEnumerator ApplyMotionBlur()
    {
        if (motionBlur == null) yield break;

        motionBlurActive = true;
        motionBlur.blurAmount = motionBlurIntensity;
        motionBlur.blurSize = 0.1f;
        motionBlur.blurSamples = 24;
        motionBlur.enabled = true;

        yield return new WaitForSeconds(motionBlurDuration);

        motionBlur.enabled = false;
        motionBlurActive = false;
        motionBlurCoroutine = null;
    }

    private IEnumerator ShakeCamera()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float strength = Mathf.Lerp(shakeAmount, 0f, elapsed / shakeDuration);
            gameCamera.transform.position = cameraBasePosition + new Vector3(
                Random.Range(-strength, strength),
                Random.Range(-strength, strength),
                0f);
            yield return null;
        }
        gameCamera.transform.position = cameraBasePosition;
        shakeCoroutine = null;
    }

    public bool IsFlashActive() => flashActive;
    public bool IsMetalPipeActive() => metalPipeActive;
    public bool IsDebuffActive() => flashActive || metalPipeActive || rollActive || motionBlurActive;
    public bool ShouldHideNote(float worldY) => false;
}