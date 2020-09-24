Shader "Custom/02ModelPositionAsColor"
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
				float4 modelSpacePosition : TEXCOORD0; //Send on first uv coordinate channel. 
			};

			ToFrag Vert(ToVert v)
			{
				ToFrag o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.modelSpacePosition = v.vertex; 
				return o;
			}

			half4 Frag(ToFrag i) : SV_Target
			{
				return i.modelSpacePosition;
			}

			ENDCG
		}
	}
}
