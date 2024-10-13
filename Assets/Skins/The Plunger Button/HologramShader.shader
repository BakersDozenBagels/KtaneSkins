Shader "Custom/HologramShader"
{
	Properties
	{
		_Color ("Color", Color) = (1, 1, 1, 1)
		_t ("T", float) = 0
	}
	SubShader
	{
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			float box(float2 position, float2 halfSize, float cornerRadius)
			{
				position = position - 0.5;
				position = abs(position) - halfSize + cornerRadius;
			    return length(max(position, 0.0)) + min(max(position.x, position.y), 0.0) - cornerRadius;
			}

			fixed4 _Color;
			float _t;

			fixed4 frag (v2f i) : SV_Target
			{
				float alpha = box(i.uv, float2(_t * .5, _t * .5), min(_t * .5, .2)) * -15;
				alpha = clamp(alpha, 0, 1);
				alpha = alpha * (1 - alpha) * 4;
				fixed4 col = _Color;
				col.a = col.a * alpha;
				return col;
			}
			ENDCG
		}
	}
}
