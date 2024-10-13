Shader "Custom/ScreenShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_ColorB ("Color 2", Color) = (1,1,1,1)
		_MainTex ("Unused", 2D) = "White" {}
		_Speed ("Speed", float) = 1
	}
	SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 150

CGPROGRAM
#pragma surface surf Lambert

fixed4 _Color, _ColorB;
float _Speed;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	o.Albedo = step(.8, sin((IN.uv_MainTex.y + _Speed * _Time.x) * 400)) ? _Color.rgb : _ColorB.rgb;
}
ENDCG
}

Fallback "KT/Mobile/DiffuseColor"
}
