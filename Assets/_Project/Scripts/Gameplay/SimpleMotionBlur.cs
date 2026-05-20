using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SimpleMotionBlur : MonoBehaviour
{
    [Range(0f, 0.95f)]
    public float blurAmount = 0.75f;

    private RenderTexture accumBuffer;
    private Material blurMat;
    private bool firstFrame = true;

    private void OnEnable()
    {
        Shader s = Shader.Find("Hidden/MotionBlur");
        if (s == null)
        {
            Debug.LogError("[MotionBlur] Шейдер Hidden/MotionBlur не найден!");
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

        if (firstFrame)
        {
            Graphics.Blit(src, accumBuffer, blurMat, 0);
            Graphics.Blit(accumBuffer, dst, blurMat, 0);
            firstFrame = false;
            return;
        }

        RenderTexture temp = RenderTexture.GetTemporary(
            src.width, src.height, 0, src.format);

        blurMat.SetTexture("_CurrTex", src);
        blurMat.SetFloat("_BlurAmount", blurAmount);
        Graphics.Blit(accumBuffer, temp, blurMat, 1);
        Graphics.Blit(temp, accumBuffer, blurMat, 0);
        Graphics.Blit(accumBuffer, dst, blurMat, 0);

        RenderTexture.ReleaseTemporary(temp);
    }
}