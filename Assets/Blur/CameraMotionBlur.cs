using UnityEngine;
using UnityEngine.VFX.Utility;

[ExecuteAlways]
public class CameraMotionBlur : MonoBehaviour
{
    public enum KernelType
    {
        MotionBlur,
    }

    public KernelType KernelIndex = KernelType.MotionBlur;

    public MeshRenderer MeshRenderer;
    public RenderTexture CameraTexture;
    public ComputeShader BlurCompute;

    public ExposedProperty MaterialBaseMap = "_BaseMap";

    [Range(0.0f, 1.0f)]
    public float MotionBlurIntensity = 1.0f;

    private RenderTexture blurredTexture;
    private RenderTexture blurredTexture2;

    private bool flip = false;
    private bool init = false;

    void OnEnable()
    {
        MeshRenderer.sharedMaterial = new Material(MeshRenderer.sharedMaterial);
        blurredTexture = CreateRenderTexture(CameraTexture.width, CameraTexture.height);
        blurredTexture2 = CreateRenderTexture(CameraTexture.width, CameraTexture.height);
        flip = false;
        init = false;
    }

    RenderTexture CreateRenderTexture(int width, int height)
    {
        RenderTexture renderTexture = new RenderTexture(CameraTexture.width, CameraTexture.height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
        return renderTexture;
    }

    void OnDisable()
    {
        blurredTexture.Release();
    }

    void LateUpdate()
    {
        if (Time.frameCount < 1) return;

        Debug.Assert(CameraTexture != null);
        Debug.Assert(BlurCompute != null);
        Debug.Assert(MeshRenderer != null);

        if(init == false)
        {
            Graphics.Blit(CameraTexture, blurredTexture);
            Graphics.Blit(CameraTexture, blurredTexture2);
            init = true;
        }

        int numThreadX = CameraTexture.width / 4;
        int numThreadY = CameraTexture.height / 4;

        flip = !flip;

        BlurCompute.SetInt("TextureWidth", CameraTexture.width);
        BlurCompute.SetInt("TextureHeight", CameraTexture.height);
        BlurCompute.SetTexture((int)KernelIndex, "CameraTexture", CameraTexture);
        BlurCompute.SetTexture((int)KernelIndex, "InputBlurred", flip ? blurredTexture2 : blurredTexture);
        BlurCompute.SetTexture((int)KernelIndex, "Result", flip ? blurredTexture : blurredTexture2);
        BlurCompute.SetFloat("MotionBlurIntensity", MotionBlurIntensity);
        

        BlurCompute.Dispatch((int)KernelIndex, numThreadX, numThreadY, 1);

        MeshRenderer.sharedMaterial.SetTexture(MaterialBaseMap, flip ? blurredTexture : blurredTexture2);
    }
}
