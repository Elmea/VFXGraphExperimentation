void MultiSphereInject(inout VFXAttributes attributes, in RWStructuredBuffer<float4> buffer)
{
	int id = attributes.particleId;
	buffer[id] = float4(attributes.position, attributes.size * 0.5);
}