using UnityEngine;
using UnityEngine.UI;

public class BossHexSegment : MonoBehaviour
{
    [Header("Image")]
    public Image hexFill; 

    [Header("Colors")]
    public Color activeColor = Color.white; 
    public Color inactiveColor = new Color(0.15f, 0.18f, 0.25f, 1.0f);
    public Color criticalColor = new Color(1.0f, 0.25f, 0.1f, 1.0f);

    private float currentFill = 1f;
    private float targetFill = 1f;
    private bool isPulsing = false;
    private bool flashActive = false;

    private Color savedActiveColor;

    private void Start()
    {
        if (hexFill != null)
            savedActiveColor = hexFill.color;
        else
            savedActiveColor = activeColor;

        activeColor = savedActiveColor;
    }

    private void Update()
    {
        if (Mathf.Approximately(currentFill, targetFill)) return;

        currentFill = Mathf.Lerp(currentFill, targetFill,
                                   Time.deltaTime * 12f);

        if (Mathf.Abs(currentFill - targetFill) < 0.001f)
            currentFill = targetFill;

        if (!flashActive && !isPulsing)
            ApplyFill();
    }

    public void SetFill(float fill, bool instant = false)
    {
        targetFill = Mathf.Clamp01(fill);
        if (instant)
        {
            currentFill = targetFill;
            ApplyFill();
        }
    }

    private void ApplyFill()
    {
        if (hexFill == null) return;
        hexFill.fillAmount = currentFill;
        hexFill.color = Color.Lerp(inactiveColor, activeColor, currentFill);
    }

    public void FlashDamage()
    {
        StopCoroutine("DamageFlash");
        StartCoroutine("DamageFlash");
    }

    private System.Collections.IEnumerator DamageFlash()
    {
        flashActive = true;
        if (hexFill) hexFill.color = Color.white;
        yield return new WaitForSeconds(0.06f);
        flashActive = false;
        ApplyFill();
    }

    public void StartCriticalPulse()
    {
        if (isPulsing) return;
        isPulsing = true;
        StartCoroutine("CriticalPulse");
    }

    public void StopCriticalPulse()
    {
        if (!isPulsing) return;
        isPulsing = false;
        StopCoroutine("CriticalPulse");
        ApplyFill();
    }

    private System.Collections.IEnumerator CriticalPulse()
    {
        while (isPulsing)
        {
            var t = Mathf.PingPong(Time.time * 3f, 1f);
            if (hexFill && !flashActive)
                hexFill.color = Color.Lerp(activeColor, criticalColor, t);
            yield return null;
        }
    }

    public float GetFill() => currentFill;
}