using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

[VFXBinder("Collision/MultiSphere")]
public class MultiSpherePropertyBinder : VFXBinderBase
{
    public SphereCollider[] Spheres;
    [VFXPropertyBinding("System.Int32")]
    public ExposedProperty SphereCountProperty = "SphereCount";
    [VFXPropertyBinding("UnityEngine.GraphicsBuffer")]
    public ExposedProperty SphereBufferProperty = "SphereBuffer";

    private GraphicsBuffer buffer;
    private float[] bufferData;

    const int MAX_COUNT = 64;

    protected override void OnEnable()
    {
        buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, MAX_COUNT, 4 * sizeof(float));
        bufferData = new float[MAX_COUNT * 4];

    }

    protected override void OnDisable()
    {
        buffer.Release();
    }

    private bool IsValidSphereList()
    {
        if(Spheres == null || Spheres.Length < 1 || Spheres.Length > MAX_COUNT)
            return false;

        bool isValid = true;
        foreach(SphereCollider sphere in Spheres)
        {
            if (sphere == null)
            {
                isValid = false;
                break;
            }
        }
        return isValid;
    }

    public override bool IsValid(VisualEffect component)
    {
        return component.HasInt(SphereCountProperty) && component.HasGraphicsBuffer(SphereBufferProperty) && IsValidSphereList();
    }


    public override void UpdateBinding(VisualEffect component)
    {
        int i = 0;
        foreach(SphereCollider sphere in Spheres)
        {
            Transform trans = sphere.transform;
            Vector3 pos = sphere.transform.position;
            float radius = sphere.radius * trans.localScale.x;
            bufferData[i * 4] = pos.x;
            bufferData[i * 4 + 1] = pos.y;
            bufferData[i * 4 + 2] = pos.z;
            bufferData[i * 4 + 3] = radius;
            ++i;
        }

        buffer.SetData(bufferData);

        component.SetInt(SphereCountProperty, Spheres.Length);
        component.SetGraphicsBuffer(SphereBufferProperty, buffer);
    }
}
