Shader "KT/Mobile/HighlightPreview" {
Properties {
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 150
Cull Front

CGPROGRAM
#pragma surface surf Lambert

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	o.Albedo = fixed3(1, 0, 0);
	o.Emission = fixed3(1, 0, 0);
}
ENDCG
}

Fallback "Mobile/VertexLit"
}
