Shader "Custom/10Checkers"
{
	Properties{
		_TileCount ("Tile Count",  Int) = 10
	}

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
				
				float offset = floor( fmod(i.uv.y * _TileCount, 2.0));
				half gradientValue = fmod(i.uv.x * _TileCount + offset, 2.0); // Repeat,  when we hit 2.0
				half brightness = floor(gradientValue); //"Rund ned". 0.5 bliver 0.0 og 1.5 bliver 1.0

				return half4(brightness, brightness, brightness, 1);

			}


			ENDCG
		}
	}
}

