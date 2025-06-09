Shader "Custom/Film" {
	Properties {
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Grain("Grain (RGB)", 2D) = "white" {}
		_Vignette("Vignette", Float) = 0.0
		_Grayscale("Grayscale", Float) = 0.0
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform sampler2D _Grain;
			uniform float _Vignette;
			uniform float _Grayscale;

			// Slightly modified from https://www.shadertoy.com/view/Wdj3zV
			#define SEQUENCE_LENGTH 24.0
			#define FPS 24.

			float4 vignette(float2 uv, float time) 
			{
				uv *=  1.0 - uv.yx;   
				float vig = uv.x*uv.y * 15.0;
				float t = sin(time * 23.) * cos(time * 8. + .5);
				vig = pow(vig, 0.4 + t * .05);
				return float4(vig.xxxx);
			}

			float easeIn(float t0, float t1, float t) 
			{
				return 2.0*smoothstep(t0,2.*t1-t0,t);
			}

			float4 blackAndWhite(float4 color) 
			{
				return float4(dot(color.xyz, float3(.299, .587, .114)).xxx, 1);
			}

			float filmDirt(float2 pp, float time) 
			{
				float aaRad = 0.1;
				float2 nseLookup2 = pp + float2(.5,.9) + time*100.;
				float3 nse2 =
					tex2D(_Grain,.1*nseLookup2.xy).xyz +
					tex2D(_Grain,.01*nseLookup2.xy).xyz +
					tex2D(_Grain,.004*nseLookup2.xy+0.4).xyz;
				float thresh = .6;
				float mul1 = smoothstep(thresh-aaRad,thresh+aaRad,nse2.x);
				float mul2 = smoothstep(thresh-aaRad,thresh+aaRad,nse2.y);
				float mul3 = smoothstep(thresh-aaRad,thresh+aaRad,nse2.z);
	
				float seed = tex2D(_Grain,float2(time*.35,time)).x;
	
				float result = clamp(0.,1.,seed+.7) + .3*smoothstep(0.,SEQUENCE_LENGTH,time);
	
				result += .06*easeIn(19.2,19.4,time);

				float band = .05;
				if( 0.3 < seed && .3+band > seed )
					return mul1 * result;
				if( 0.6 < seed && .6+band > seed )
					return mul2 * result;
				if( 0.9 < seed && .9+band > seed )
					return mul3 * result;
				return result;
			}

			float4 jumpCut(float seqTime) 
			{
				float toffset = 0.;
				float3 camoffset = float3(0, 0, 0);
	
				float jct = seqTime;
				float jct1 = 7.7;
				float jct2 = 8.2;
				float jc1 = step( jct1, jct );
				float jc2 = step( jct2, jct );
	
				camoffset += float3(.8,.0,.0) * jc1;
				camoffset += float3(-.8,0.,.0) * jc2;
	
				toffset += 0.8 * jc1;
				toffset -= (jc2-jc1)*(jct-jct1);
				toffset -= 0.9 * jc2;
	
				return float4(camoffset, toffset);
			}

			float limitFPS(float time, float fps) 
			{
				time = fmod(time * fps, SEQUENCE_LENGTH);
				return float(int(time * fps)) / fps;
			}

			float2 moveImage(float2 uv, float time) 
			{
				time *= 100;
				uv.x += .002 * (cos(time * 3.) * sin(time * 12. + .25));
				uv.y += .002 * (sin(time * 1. + .5) * cos(time * 15. + .25));
				return uv;
			}

			float4 frag(v2f_img i) : COLOR
			{
				float2 uv = i.uv.xy * 3;
				float2 qq = -1.0 + 2.0*uv;
				qq.x *= _ScreenParams.x / _ScreenParams.y;
    
				float time = limitFPS(_Time, FPS);

				float4 jumpCutData = jumpCut(time);
				float4 dirt = float4(filmDirt(qq, time + jumpCutData.w).xxxx);     
				float4 image = tex2D(_MainTex, moveImage(i.uv, time));   
				float4 vig = lerp(1, vignette(i.uv, time), _Vignette / 1.6);
    
				float4 fragColor = image * dirt * vig;
				return lerp(fragColor, blackAndWhite(fragColor), _Grayscale / 0.35);
			}
			ENDCG
		}
	}
	FallBack "Unlit"
}
