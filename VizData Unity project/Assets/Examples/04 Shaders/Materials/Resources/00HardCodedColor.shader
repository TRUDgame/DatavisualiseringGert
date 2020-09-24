Shader "Custom/00HardCodedColor"
{
	SubShader
	{
		Pass
		{
			CGPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag
		
			struct ToVert
			{
				float4 vertex : POSITION;
			};
			
			struct ToFrag
			{
				float4 vertex : SV_POSITION;
			};
			
			ToFrag Vert( ToVert v )
			{
				ToFrag o;
				o.vertex = UnityObjectToClipPos( v.vertex );
				return o;
			}
			
			half4 Frag( ToFrag i ) : SV_Target
			{
				return half4( 0.0, 1.0, 0.0, 1.0 );
			}

			ENDCG
		}
	}
}