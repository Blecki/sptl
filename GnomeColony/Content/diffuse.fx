float4x4 World;
float4x4 View;
float4x4 Projection;

float4 DiffuseColor;
float Alpha;
float ClipAlpha;

texture Texture;
float4x4 UVTransform;

sampler diffuseSampler = sampler_state
{
    Texture = (Texture);
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

	float4 tTexcoord = float4(input.Texcoord[0], input.Texcoord[1], 1, 1);
	tTexcoord = mul(tTexcoord, UVTransform);

	output.Texcoord = float2(tTexcoord[0], tTexcoord[1]);
    return output;
}

struct PixelShaderOutput
{
    float4 Color : COLOR0;
};

PixelShaderOutput PSTexturedColor(TexturedVertexShaderOutput input) : COLOR0
{
    PixelShaderOutput output;
	output.Color = DiffuseColor * tex2D(diffuseSampler, input.Texcoord);
	output.Color.a *= Alpha;
	clip(output.Color.a < ClipAlpha ? -1:1);

    return output;
}

technique Draw
{
    pass Pass1
    {
		AlphaBlendEnable = false;
		BlendOp = Add;
		SrcBlend = One;
		DestBlend = InvSrcAlpha;
		
        VertexShader = compile vs_4_0 TexturedVertexShaderFunction();
        PixelShader = compile ps_4_0 PSTexturedColor();
    }
}
