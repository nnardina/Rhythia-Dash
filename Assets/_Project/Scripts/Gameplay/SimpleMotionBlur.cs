using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SimpleMotionBlur : MonoBehaviour
{
    [Range(0f, 0.99f)]
    public float blurAmount = 0.99f;
    
    [Header("Directional Blur")]
    [Range(0f, 0.2f)]
    public float blurSize = 0.05f;
    
    [Range(4, 32)]
    public int blurSamples = 16;

    private RenderTexture accumBuffer;
    private Material blurMat;
    private bool firstFrame = true;

    private void OnEnable()
    {
        Shader s = Shader.Find("Custom/MotionBlur");
        if (s == null)
        {
            Debug.LogError("[MotionBlur] Шейдер Custom/MotionBlur не найден!");
            enabled = false;
            return;
        }

        blurMat = new Material(s);
        blurMat.hideFlags = HideFlags.HideAndDontSave;
        firstFrame = true;

        Debug.Log("[MotionBlur] Enabled");
    }

    private void OnDisable()
    {
        ReleaseBuffer();

        if (blurMat != null)
        {
            DestroyImmediate(blurMat);
            blurMat = null;
        }

        Debug.Log("[MotionBlur] Disabled");
    }

    private void ReleaseBuffer()
    {
        if (accumBuffer != null)
        {
            accumBuffer.Release();
            DestroyImmediate(accumBuffer);
            accumBuffer = null;
        }
    }

    private void EnsureBuffer(RenderTexture src)
    {
        if (accumBuffer != null
            && accumBuffer.width == src.width
            && accumBuffer.height == src.height)
            return;

        ReleaseBuffer();

        accumBuffer = new RenderTexture(src.width, src.height, 0, src.format);
        accumBuffer.hideFlags = HideFlags.HideAndDontSave;
        accumBuffer.Create();
        firstFrame = true;
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (blurMat == null)
        {
            Graphics.Blit(src, dst);
            return;
        }

        EnsureBuffer(src);
        
        // Применяем directional blur
        RenderTexture blurred = RenderTexture.GetTemporary(
            src.width, src.height, 0, src.format);
        
        blurMat.SetFloat("_BlurSize", blurSize);
        blurMat.SetInt("_BlurSamples", blurSamples);
        Graphics.Blit(src, blurred, blurMat, 0);

        if (firstFrame)
        {
            Graphics.Blit(blurred, accumBuffer);
            Graphics.Blit(accumBuffer, dst);
            firstFrame = false;
            RenderTexture.ReleaseTemporary(blurred);
            return;
        }

        RenderTexture temp = RenderTexture.GetTemporary(
            src.width, src.height, 0, src.format);

        blurMat.SetTexture("_CurrTex", blurred);
        blurMat.SetFloat("_BlurAmount", blurAmount);
        Graphics.Blit(accumBuffer, temp, blurMat, 1);
        Graphics.Blit(temp, accumBuffer);
        Graphics.Blit(accumBuffer, dst);

        RenderTexture.ReleaseTemporary(temp);
        RenderTexture.ReleaseTemporary(blurred);
    }
}