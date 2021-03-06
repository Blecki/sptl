float4x4 World;
float4x4 View;
float4x4 Projection;

texture Diffuse;

sampler diffuseSampler = sampler_state
{
	Texture = (Diffuse);
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = POINT;
	AddressU = Wrap;
	AddressV = Wrap;
};

struct TexturedVertexShaderInput
{
	float4 Position : SV_Position0;
	float2 Texcoord : TEXCOORD0;
};

struct TexturedVertexShaderOutput
{
	float4 Position : SV_Position0;
	float2 Texcoord : TEXCOORD0;
};


TexturedVertexShaderOutput TexturedVertexShaderFunction(TexturedVertexShaderInput input)
{
    TexturedVertexShaderOutput output;
    input.Position.w = 1.0f;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	output.Texcoord = input.Texcoord;
    return output;
}

struct PixelShaderOutput
{
    float4 Color : COLOR0;
};

PixelShaderOutput PSTexturedColor(TexturedVertexShaderOutput input) : COLOR0
{
    PixelShaderOutput output;
	output.Color = tex2D(diffuseSampler, float2(input.Position.x / 800, input.Position.y / 600));
    return output;
}

technique Draw
{
    pass Pass1
    {
		AlphaBlendEnable = false;
		
        VertexShader = compile vs_4_0 TexturedVertexShaderFunction();
        PixelShader = compile ps_4_0 PSTexturedColor();
    }
}
