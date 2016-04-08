float4x4 World;
float4x4 View;
float4x4 Projection;
texture Texture;

float4x4 WorldInverseTranspose;

sampler diffuseSampler = sampler_state
{
    Texture = (Texture);
    MAGFILTER = POINT;
    MINFILTER = POINT;
    MIPFILTER = POINT;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

struct VertexShaderInput
{
	float4 Position : SV_Position0;
    float2 TexCoord : TEXCOORD0;
	float4 FGColor : COLOR0;
	float4 BGColor : COLOR1;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
	float4 FGColor : COLOR0;
	float4 BGColor : COLOR1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    input.Position.w = 1.0f;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.TexCoord = input.TexCoord; 
	output.FGColor = input.FGColor;
	output.BGColor = input.BGColor;

    return output;
}

struct PixelShaderOutput
{
    float4 Color : COLOR0;
};

PixelShaderOutput PSColor(VertexShaderOutput input)
{
    PixelShaderOutput output;
	float2 texcoord = input.TexCoord;//(input.TexCoord.x - (0.5 / 128.0), input.TexCoord.y);
	float4 texColor = tex2D(diffuseSampler, texcoord);
	texColor *= input.FGColor;
	output.Color = (texColor * texColor.a) + (input.BGColor * (1.0 - texColor.a));
	output.Color.a = (texColor.a * input.FGColor.a) + input.BGColor.a;
	
	return output;
}

technique DrawColor
{
    pass Pass1
    {
		//AlphaBlendEnable = false;
		//BlendOp = Add;
		//SrcBlend = One;
		//DestBlend = InvSrcAlpha;
		////ZEnable = false;
		////ZWriteEnable = true;
		////ZFunc = LessEqual;
		//CullMode = None;
		
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PSColor();
    }
}