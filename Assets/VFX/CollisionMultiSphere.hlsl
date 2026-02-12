void MultiSphereCollision(inout VFXAttributes attributes, in StructuredBuffer<float4> buffer, in int count, in float multiplier)
{
	int id = attributes.particleId;
	int size = attributes.size;

	for (int i = 0; i < count; ++i)
	{
		float4 s = buffer[i];
		float3 pos = attributes.position;
		float3 delta = pos - s.xyz;

		float dist = length(delta) - (s.w + size * 0.5);

		if (dist < 0.0)
		{
			attributes.position -= normalize(delta) * dist;
			attributes.velocity -= normalize(delta) * dist * multiplier;
		}
	}
}

void MultiSphereCollisionExceptSelf(inout VFXAttributes attributes, in StructuredBuffer<float4> buffer, in int count, in float multiplier)
{
	int id = attributes.particleId;
	int size = attributes.size;

	for (int i = 0; i < count; ++i)
	{
		if (i == id)
			continue;

		float4 s = buffer[i];
		float3 pos = attributes.position;
		float3 delta = pos - s.xyz;

		float dist = length(delta) - (s.w + size * 0.5);

		if (dist < 0.0)
		{
			attributes.position -= normalize(delta) * dist;
			attributes.velocity -= normalize(delta) * dist * multiplier;
		}
	}
}