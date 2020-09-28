Shader "Custom/20"
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

				half2 pos = i.uv * _NoiseFrequency; 
				half result = SimplexNoise(half3(pos, _Time.y)); 
				

				return half4(result.xxx, 1);


			}


			ENDCG
		}
	}
}

