Shader "Custom/16RadialGradient"
{


	SubShader
	{
		Pass
		{
			CGPROGRAM

#pragma  target 2.0

			#pragma vertex Vert
			#pragma fragment Frag

			struct ToVert
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0; //Receive UV set 0. 
			};

			struct ToFrag
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			int _TileCount; 

			ToFrag Vert(ToVert v)
			{
				ToFrag o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv; //Copy uv to output that will be forwarded to Frag function. 
				return o;
			}

			half4 Frag(ToFrag i) : SV_Target
			{
				//float pos = (i.uv * 2) - 1; 
				half brightness = length(i.uv * 2); 
			// half brightness = distance(half2(0, 0), pos); 

				return half4(brightness, brightness, brightness, 1);


			}


			ENDCG
		}
	}
}

