Shader "DataVisWorkshop/GLShader"
{
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			#include "UnityCG.cginc"

			struct ToVert
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct ToFrag
			{
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			ToFrag Vert( ToVert v )
			{
				ToFrag o;
				o.vertex = UnityObjectToClipPos( v.vertex );
				o.color = v.color;
				return o;
			}

			fixed4 Frag( ToFrag i ) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
	}
}
