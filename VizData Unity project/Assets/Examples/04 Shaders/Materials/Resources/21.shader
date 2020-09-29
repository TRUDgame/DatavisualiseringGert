Shader "Custom/21"
{

	Properties{
		_NoiseFrequency("Noise Frequency", Float) = 1.0

	}

	SubShader
	{
		Pass
		{
			CGPROGRAM

			#pragma  target 2.0

			#include "SimplexNoise.cginc"

			#pragma vertex Vert
			#pragma fragment Frag

			
				half hash12(half2 p)
			{
				half3 p3 = frac(half3(p.xyx) * .1031);
				p3 += dot(p3, p3.yzx + 33.33);
				return frac((p3.x + p3.y) * p3.z);
			}
			struct ToVert
			{

				float4 vertex : POSITION;
				float2 uv : TEXCOORD0; //Receive UV set 0. 
				float3 normal : NORMAL; 
			};

			struct ToFrag
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			half _NoiseFrequency; 
			int _StepCount; 

			ToFrag Vert(ToVert v)
			{
				v.vertex.xyz += v.normal * 0.2 * SimplexNoise(half4(v.vertex.xyz *_NoiseFrequency, _Time.y));

				ToFrag o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv; //Copy uv to output that will be forwarded to Frag function. 
				return o;
			}

			half4 Frag(ToFrag i) : SV_Target
			{

				int stepCount = 10;

				i.uv.x += floor(_Time.y * 10);
				half2 pos = max(abs((i.uv.x - 0.5)), abs((i.uv.y - 0.5)) * stepCount);
				half result = hash12(pos);


				return half4(result.xxx, 1);


			}


			ENDCG
		}
	}
}

