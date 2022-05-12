struct vertexData
{
	float4 position : POSITION;
	float4 normalCoord : NORMAL;
	float4 color : COLOR;
	float2 texCoord : TEXCOORD0;
};

struct pixelData
{
	float4 positionScreenSpace : SV_POSITION;
	float4 positionWorldSpace : POSITION0;
	float4 normalWorldSpace : POSITION1;
	float4 color : COLOR;
	float2 texCoord : TEXCOORD0;
};

cbuffer perFrameData : register(b0) {
	float time;
	float3 _padding0;
}

cbuffer perObjectData : register(b1)
{
	float4x4	worldViewProjectionMatrix;
	float4x4    worldMatrix;
	float4x4	inverseTransposeWorldMatrix;
	int			timeScaling;
	float3      _padding1;
}

pixelData vertexShader(vertexData input) {
	pixelData output = (pixelData)0;
	float4 position = input.position;

	output.positionScreenSpace = mul(position, worldViewProjectionMatrix);
	output.positionWorldSpace = mul(position, worldMatrix);
	output.normalWorldSpace = float4(mul(input.normalCoord.xyz, (float3x3) inverseTransposeWorldMatrix), 1.0f);
	output.color = input.color;
	output.texCoord = input.texCoord;

	return output;
}