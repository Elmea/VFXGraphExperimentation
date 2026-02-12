using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

[VFXBinder("Collision/Structured")]
public class StructuredBufferPropertyBinder : VFXBinderBase
{
    [VFXPropertyBinding("UnityEngine.GraphicsBuffer")]
    public ExposedProperty BufferProperty = "Buffer";
    public int Count = 64;
    public int Stride = 4 * sizeof(float);

    private GraphicsBuffer buffer;

    private void CreateBuffer()
    {
        if(buffer != null)
        {
            buffer.Release();
        }
        buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, Count, Stride);
        byte[] tempBuffer = new byte[Count * Stride];
        buffer.SetData(tempBuffer);
    }

    protected override void OnEnable()
    {
        CreateBuffer();
    }

    protected override void OnDisable()
    {
        buffer.Release();
    }

    public override bool IsValid(VisualEffect component)
    {
        return component.HasGraphicsBuffer(BufferProperty);
    }


    public override void UpdateBinding(VisualEffect component)
    {
        if(buffer.count != Count || buffer.stride != Stride)
        {
            CreateBuffer();
        }

        component.SetGraphicsBuffer(BufferProperty, buffer);
    }
}
