using UnityEngine;

public class Simulation : MonoBehaviour
{
    public enum Resolution
    {
        _32  = 32,
        _64  = 64,
        _128 = 128,
    }

    public Transform PlayerTransform;
    public float StepSize = 0.5f;
    public Resolution SimulationResolution = Resolution._32;
    public Vector3 Offset = new Vector3(0.0f, 0.1f, 0.0f);

    public Transform QuadTransform;
    public MeshRenderer QuadMeshRenderer;

    public ComputeShader SimulationComputeShader;

    private Vector3 lastPos;

    private RenderTexture buffer;
    private RenderTexture buffer2;

    private void OnEnable()
    {
        lastPos = SnapPosition(PlayerTransform.position);
        buffer = CreateBuffer("buffer");
        buffer2 = CreateBuffer("buffer2");
    }

    RenderTexture CreateBuffer(string name)
    {
        RenderTexture tempBuffer = new RenderTexture((int)SimulationResolution, (int)SimulationResolution, 0, RenderTextureFormat.RGHalf, RenderTextureReadWrite.Linear);
        tempBuffer.enableRandomWrite = true;
        tempBuffer.name = name;
        tempBuffer.Create();
        return tempBuffer;
    }

    private void OnDisable()
    {
        buffer.Release();
        buffer2.Release();
    }

    Vector3 SnapPosition(Vector3 pos)
    {
        return new Vector3(pos.x - (pos.x % StepSize), pos.y, pos.z - (pos.z % StepSize));
    }

    Vector2 WorldToSim(Vector3 pos, Vector3 simCenter, float simSize)
    {
        pos = (pos - simCenter) / simSize;
        return new Vector2(pos.x, pos.z);
    }

    private void LateUpdate()
    {
        float scale = (int)SimulationResolution * StepSize;
        Vector3 pos = PlayerTransform.position;
        Vector3 snappedPos = SnapPosition(pos);
        QuadTransform.position = snappedPos + Offset;
        QuadTransform.localScale = new Vector3(scale, scale, scale);


        Vector2 texelOffset = new Vector2((snappedPos.x - lastPos.x) / StepSize, (snappedPos.z - lastPos.z) / StepSize);

        int numX = (int)SimulationResolution / 8;

        SimulationComputeShader.SetInt("resolution", (int)SimulationResolution);
        Vector2 simPos = WorldToSim(pos, snappedPos, scale);

        SimulationComputeShader.SetFloats("playerPosition", simPos.x, simPos.y);
        SimulationComputeShader.SetTexture(0, "Previous", buffer);
        SimulationComputeShader.SetTexture(0, "Result", buffer2);
        SimulationComputeShader.Dispatch(0, numX, numX, 1);

        SimulationComputeShader.SetFloats("offset", texelOffset.x, texelOffset.y);
        SimulationComputeShader.SetTexture(1, "Previous", buffer2);
        SimulationComputeShader.SetTexture(1, "Result", buffer);

        SimulationComputeShader.Dispatch(1, numX, numX, 1);

        QuadMeshRenderer.sharedMaterial.SetTexture("_BaseMap", buffer );

        lastPos = snappedPos;
    }
}
