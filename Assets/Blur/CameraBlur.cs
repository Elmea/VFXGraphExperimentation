using UnityEngine;
using UnityEngine.VFX.Utility;

[ExecuteAlways]
public class CameraBlur : MonoBehaviour
{
    public enum KernelType
    {
        NaiveBlur,
        OptimizedBlur,
    }

    public KernelType KernelIndex = KernelType.NaiveBlur;

    public MeshRenderer MeshRenderer;
    public RenderTexture CameraTexture;
    public ComputeShader BlurCompute;

    public ExposedProperty MaterialBaseMap = "_BaseMap";

    private RenderTexture blurredTexture;

    void OnEnable()
    {
        blurredTexture = new RenderTexture(CameraTexture.width, CameraTexture.height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
        blurredTexture.enableRandomWrite = true;
        blurredTexture.Create();
    }

    void OnDisable()
    {
        blurredTexture.Release();
    }

    void LateUpdate()
    {
        Debug.Assert(CameraTexture != null);
        Debug.Assert(BlurCompute != null);
        Debug.Assert(MeshRenderer != null);

        int numThreadX = CameraTexture.width / 4;
        int numThreadY = CameraTexture.height / 4;

        BlurCompute.SetInt("TextureWidth", CameraTexture.width);
        BlurCompute.SetInt("TextureHeight", CameraTexture.height);
        BlurCompute.SetTexture((int)KernelIndex, "CameraTexture", CameraTexture);
        BlurCompute.SetTexture((int)KernelIndex, "Result", blurredTexture);


        BlurCompute.Dispatch((int)KernelIndex, numThreadX, numThreadY, 1);

        MeshRenderer.sharedMaterial.SetTexture(MaterialBaseMap, blurredTexture);
    }
}
