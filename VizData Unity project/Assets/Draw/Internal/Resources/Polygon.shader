/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

Shader "Hidden/Draw/Polygon"
{
	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag

			#pragma multi_compile_fog 				// Support fog.
			#pragma multi_compile_instancing		// Support instancing

			#include "UnityCG.cginc"
			#include "SDFShapeBase.cginc"

			UNITY_INSTANCING_BUFFER_START( Props )
				UNITY_DEFINE_INSTANCED_PROP( fixed4, _FillColor )
			UNITY_INSTANCING_BUFFER_END( Props )
			

			ToFrag Vert( ToVert v )
			{
				ToFrag o;

				UNITY_SETUP_INSTANCE_ID( v );			// Support instancing
				UNITY_TRANSFER_INSTANCE_ID( v, o );		// Support instanced properties in fragment Shader.

				float2 scale = GetModelScale2D();
				o.pos = v.vertex.xy * scale;

				o.vertex = UnityObjectToClipPos( v.vertex );

				UNITY_TRANSFER_FOG( o, o.vertex ); 		// Support fog.

				return o;
			}

			
			fixed4 Frag( ToFrag i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( i ); // Support instanced properties in fragment Shader.

				// Get instanced properties.
				fixed4 col = UNITY_ACCESS_INSTANCED_PROP( Props, _FillColor );

				// Support fog.
				UNITY_APPLY_FOG( i.fogCoord, col );

				return col;
			}
			ENDCG
		}
	}
}