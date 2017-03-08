//-----------------------------------------------------------------------------
// SpriteEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#include "Macros.fxh"

DECLARE_TEXTURE(Texture, 0);

BEGIN_CONSTANTS
MATRIX_CONSTANTS
	float4x4 MatrixTransform    _vs(c0) _cb(c0);
END_CONSTANTS

float2 TextureSize;

struct VertexShaderInput
{
	float4 color : COLOR0;
	float4 texCoord : TEXCOORD0;
	float4 position : SV_Position;
};

struct VertexShaderOutput
{
	float4 color : COLOR0;
	float4 position : SV_Position;
	float4 texCoord0 : TEXCOORD0;
	float4 texCoord1 : TEXCOORD1;
	float4 texCoord2 : TEXCOORD2;
	float4 texCoord3 : TEXCOORD3;
	float4 texCoord4 : TEXCOORD4;
};

VertexShaderOutput SpriteVertexShader(VertexShaderInput input)
{
	VertexShaderOutput output;

	float x = 0.5 * (1.0 / TextureSize.x);
	float y = 0.5 * (1.0 / TextureSize.y);
	float2 dg1 = float2( x, y);
	float2 dg2 = float2(-x, y);
	float2 dx = float2(x, 0.0);
	float2 dy = float2(0.0, y);

	output.color = input.color;
	output.position = mul(input.position, MatrixTransform);
	output.texCoord0 = input.texCoord;
	output.texCoord1.xy = input.texCoord.xy - dg1;
	output.texCoord1.zw = input.texCoord.xy - dy;
	output.texCoord2.xy = input.texCoord.xy - dg2;
	output.texCoord2.zw = input.texCoord.xy + dx;
	output.texCoord3.xy = input.texCoord.xy + dg1;
	output.texCoord3.zw = input.texCoord.xy + dy;
	output.texCoord4.xy = input.texCoord.xy + dg2;
	output.texCoord4.zw = input.texCoord.xy - dx;

	return output;
}

const float mx = 0.325;      // start smoothing wt.
const float k = -0.250;      // wt. decrease factor
const float max_w = 0.25;    // max filter weigth
const float min_w =-0.05;    // min filter weigth
const float lum_add = 0.25;  // effects smoothing

float4 SpritePixelShader(VertexShaderOutput input) : SV_Target0
{
	float4 c00 = SAMPLE_TEXTURE(Texture, input.texCoord1.xy); 
	float4 c10 = SAMPLE_TEXTURE(Texture, input.texCoord1.zw); 
	float4 c20 = SAMPLE_TEXTURE(Texture, input.texCoord2.xy); 
	float4 c01 = SAMPLE_TEXTURE(Texture, input.texCoord4.zw); 
	float4 c11 = SAMPLE_TEXTURE(Texture, input.texCoord0.xy); 
	float4 c21 = SAMPLE_TEXTURE(Texture, input.texCoord2.zw); 
	float4 c02 = SAMPLE_TEXTURE(Texture, input.texCoord4.xy); 
	float4 c12 = SAMPLE_TEXTURE(Texture, input.texCoord3.zw); 
	float4 c22 = SAMPLE_TEXTURE(Texture, input.texCoord3.xy); 
	float4 dt = float4(1.0, 1.0, 1.0, 1.0);
	
	float md1 = dot(abs(c00 - c22), dt);
	float md2 = dot(abs(c02 - c20), dt);
	
	float w1 = dot(abs(c22 - c11), dt) * md2;
	float w2 = dot(abs(c02 - c11), dt) * md1;
	float w3 = dot(abs(c00 - c11), dt) * md2;
	float w4 = dot(abs(c20 - c11), dt) * md1;
	
	float t1 = w1 + w3;
	float t2 = w2 + w4;
	float ww = max(t1, t2) + 0.0001;
	
	c11 = (w1 * c00 + w2 * c20 + w3 * c22 + w4 * c02 + ww * c11) / (t1 + t2 + ww);
	
	float lc1 = k / (0.12 * dot(c10 + c12 + c11, dt) + lum_add);
	float lc2 = k / (0.12 * dot(c01 + c21 + c11, dt) + lum_add);
	
	w1 = clamp(lc1 * dot(abs(c11 - c10), dt) + mx, min_w, max_w);
	w2 = clamp(lc2 * dot(abs(c11 - c21), dt) + mx, min_w, max_w);
	w3 = clamp(lc1 * dot(abs(c11 - c12), dt) + mx, min_w, max_w);
	w4 = clamp(lc2 * dot(abs(c11 - c01), dt) + mx, min_w, max_w);
	
	return w1 * c10 + w2 * c21 + w3 * c12 + w4 * c01 + (1.0 - w1 - w2 - w3 - w4) * c11;
}

technique SpriteBatch
{
	pass
	{
		VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader  = compile ps_3_0 SpritePixelShader();
	}
}