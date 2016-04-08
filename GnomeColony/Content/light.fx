float4x4 World;
float4x4 View;
float4x4 Projection;

texture Diffuse;
texture Shadow;
texture Light;

float4 Color;

sampler diffuseSampler = sampler_state
{
	Texture = (Diffuse);
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = POINT;
	AddressU = Wrap;
	AddressV = Wrap;
};

sampler shadowSampler = sampler_state
{
	Texture = (Shadow);
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = POINT;
	AddressU = Wrap;
	AddressV = Wrap;
};

sampler lightSampler = sampler_state
{
	Texture = (Light);
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

	float4 diffuse = tex2D(diffuseSampler, float2(input.Position.x / 800, input.Position.y / 600));
		float4 shadow = tex2D(shadowSampler, float2(input.Position.x / 800, input.Position.y / 600));
		float4 light = tex2D(lightSampler, input.Texcoord);

		output.Color = Color * diffuse * (1.0f - shadow.r);
	output.Color.a = light.r;

    return output;
}

technique Draw
{
    pass Pass1
    {
		AlphaBlendEnable = true;
		BlendOp = Add;
		SrcBlend = SrcAlpha;
		DestBlend = One;
		
        VertexShader = compile vs_4_0 TexturedVertexShaderFunction();
        PixelShader = compile ps_4_0 PSTexturedColor();
    }
}
