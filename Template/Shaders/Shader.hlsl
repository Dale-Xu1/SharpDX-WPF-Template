struct PSInput
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
};

float4x4 Transform;

PSInput VSMain(float3 position : POSITION, float4 color : COLOR)
{
	PSInput result;

	result.position = mul(Transform, float4(position, 1));
	result.color = color;

	return result;
}

float4 PSMain(PSInput input) : SV_TARGET
{
	return input.color;
}
