using StableFluids.Marbling;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[DefaultExecutionOrder(-1)]
public class Simulation : MonoBehaviour
{
    public MarblingFluidSimulator MarblingSimulator;

    private static Simulation sInstance;
    public static Simulation Instance => sInstance;

    Dictionary<SphereInfluence, (Vector3, Vector3)> influences = new Dictionary<SphereInfluence, (Vector3, Vector3)>();

    public void Subscribe(SphereInfluence influencer)
    {
        Debug.Assert(!influences.ContainsKey(influencer));
        influences.Add(influencer, (influencer.transform.position, influencer.transform.position));
    }

    public void Unsubscribe(SphereInfluence influencer)
    {
        Debug.Assert(influences.ContainsKey(influencer));
        influences.Remove(influencer);
    }

    public void UpdatePosition(SphereInfluence influencer, Vector3 position)
    {
        Debug.Assert(influences.ContainsKey(influencer));
        var values = influences[influencer];
        values = (position, values.Item1);
        influences[influencer] = values;
    }

    public Transform PlayerTransform;
    public CharacterController CharacterController;
    public float StepSize = 0.5f;
    public Vector3 Offset = new Vector3(0.0f, 0.1f, 0.0f);

    public RenderTexture ForceBuffer;
    public RenderTexture VelocityBuffer;
    public Shader InjectionShader;

    private RenderTexture tempCopy;

    private Material injectionMaterial;

    public ComputeShader OffsetCompute;

    public Transform QuadTransform;
    public MeshRenderer QuadMeshRenderer;

    public VisualEffect VisualEffect;

    private Vector3 lastPos;
    private Vector3 lastSnappedPos;

    public float VelocityMult = 5.0f;
    public float FallOff = 100.0f;

    private void Awake()
    {
        sInstance = this;
    }

    private void OnEnable()
    {
        lastSnappedPos = SnapPosition(PlayerTransform.position);
        injectionMaterial = new Material(InjectionShader);
        injectionMaterial.SetFloat("_Aspect", 1f);

        tempCopy = new RenderTexture(VelocityBuffer);
        tempCopy.enableRandomWrite = true;

        VisualEffect.SetFloat("SimSize", StepSize * VelocityBuffer.width);
        VisualEffect.SetTexture("VelocityBuffer", VelocityBuffer);
    }

    private void OnDisable()
    {
        injectionMaterial = null;
        tempCopy.Release();
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

    Vector2 VelocityWorldToSim(Vector3 pos, Vector3 prevPos, Vector3 simCenter, float simSize)
    {
        pos = WorldToSim(pos, simCenter, simSize);
        prevPos = WorldToSim(prevPos, simCenter, simSize);
        return (pos - prevPos) / Time.deltaTime;
    }

    private void OffsetBuffer(RenderTexture inputBuffer, int numX, Vector2 texelOffset)
    {
        OffsetCompute.SetFloats("offset", texelOffset.x, texelOffset.y);
        OffsetCompute.SetTexture(0, "Previous", inputBuffer);
        OffsetCompute.SetTexture(0, "Result", tempCopy);
        OffsetCompute.Dispatch(0, numX, numX, 1);

        Graphics.Blit(tempCopy, inputBuffer);
    }

    private void LateUpdate()
    {
        int simulationResolution = ForceBuffer.width;

        float scale = simulationResolution * StepSize;
        Vector3 pos = PlayerTransform.position;
        Vector3 snappedPos = SnapPosition(pos);
        QuadTransform.position = snappedPos + Offset;
        QuadTransform.localScale = new Vector3(scale, scale, scale);

        Vector2 texelOffset = new Vector2(Mathf.RoundToInt((snappedPos.x - lastSnappedPos.x) / StepSize), Mathf.RoundToInt((snappedPos.z - lastSnappedPos.z) / StepSize));

        int numX = simulationResolution / 8;

        // Offset
        OffsetBuffer(VelocityBuffer, numX, texelOffset);

        // Injection
        Vector2 simPos = WorldToSim(pos, snappedPos, scale);
        Vector2 simVel = VelocityWorldToSim(pos, lastPos, snappedPos, scale);

        Graphics.Blit(Texture2D.blackTexture, ForceBuffer);

        if(simVel.sqrMagnitude > 0.001f)
        {
            injectionMaterial.SetVector("_Origin", simPos);
            injectionMaterial.SetFloat("_Falloff", FallOff);
            injectionMaterial.SetVector("_Force", simVel * VelocityMult);

            Graphics.Blit(null, ForceBuffer, injectionMaterial, 1);
        }

        foreach(var kvp in influences)
        {
            var influencerPos = kvp.Value.Item1;
            var influencerPrevPos = kvp.Value.Item2;

            var influencerSimPos = WorldToSim(influencerPos, snappedPos, scale);
            var influencerSimVel = VelocityWorldToSim(influencerPos, influencerPrevPos, snappedPos, scale);

            if (influencerSimVel.sqrMagnitude > 0.001f)
            {
                injectionMaterial.SetVector("_Origin", influencerSimPos);
                injectionMaterial.SetFloat("_Falloff", FallOff);
                injectionMaterial.SetVector("_Force", influencerSimVel * VelocityMult);

                Graphics.Blit(null, ForceBuffer, injectionMaterial, 1);
            }
        }

        MarblingSimulator.UpdateSimulation();

        QuadMeshRenderer.sharedMaterial.SetTexture("_BaseMap", VelocityBuffer);

        lastPos = pos;
        lastSnappedPos = snappedPos;
    }
}
