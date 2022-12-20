RWTexture2D<float4> Result;

[numthreads(8, 8, 1)]
void Main(uint3 id : SV_DispatchThreadID)
{
	int x = id.x & id.y;
	Result[id.xy] = float4(x, x, x, 1);
}
