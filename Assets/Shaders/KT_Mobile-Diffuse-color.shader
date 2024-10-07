Shader "KT/Mobile/DiffuseColor" {
Properties {
	_Color ("Color", Color) = (1,1,1,1)
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 150

CGPROGRAM
#pragma surface surf Lambert

fixed4 _Color;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	o.Albedo = _Color.rgb;
	o.Alpha = _Color.a;
}
ENDCG
}

Fallback "Mobile/VertexLit"
}
