Shader "Custom/07HorizontalRepeatedGradient"
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

			ToFrag Vert(ToVert v)
			{
				ToFrag o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv; //Copy uv to output that will be forwarded to Frag function. 
				return o;
			}

			half4 Frag(ToFrag i) : SV_Target
			{
				int gradientCount = 10; 
				half brightness = frac(i.uv.x * gradientCount); // Scale to 0.0-10.0
			//half brightness = fmod(i.uv.x * 10, 1); //Same 
				return half4(brightness, brightness, brightness, 1); 
				//return half4(brightness.xxx,1); //Alternative
			}


			ENDCG
		}
	}
}
