/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

Shader "Hidden/Draw/Circle"
{
	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM

			#pragma vertex VertNoScale
			#pragma fragment Frag

			#pragma multi_compile_fog 				// Support fog.
			#pragma multi_compile_instancing		// Support instancing
			#pragma multi_compile_local __ _ANTIALIAS

			#include "UnityCG.cginc"
			#include "SDFShapeBase.cginc"

			UNITY_INSTANCING_BUFFER_START( Props )
				UNITY_DEFINE_INSTANCED_PROP( fixed4, _FillColor )
				UNITY_DEFINE_INSTANCED_PROP( fixed4, _StrokeColor )
				UNITY_DEFINE_INSTANCED_PROP( half, _InnerRadiusRel )
			UNITY_INSTANCING_BUFFER_END( Props )
			
			
			fixed4 Frag( ToFrag i ) : SV_Target
			{
				// Compute SDF.
				half d = length( i.pos );
				if( d > 1 ) discard;

				UNITY_SETUP_INSTANCE_ID( i ); // Support instanced properties in fragment Shader.

				// Grow circle to inner radius.
				half innerRadiusRel = UNITY_ACCESS_INSTANCED_PROP( Props, _InnerRadiusRel );
				d -= innerRadiusRel;

				// Get instanced properties.
				fixed4 fillCol = UNITY_ACCESS_INSTANCED_PROP( Props, _FillColor );
				fixed4 strokeCol = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeColor );

				#ifdef _ANTIALIAS
					// Compute the absolute difference between 'd' at this fragment and 'd' at the neighboring fragments.
					// The actual values of 'd' are picked up from neightbor threads, which are executing in same group
					// on modern graphics cards - so it should be cheap.
					// https://computergraphics.stackexchange.com/questions/61/what-is-fwidth-and-how-does-it-work/64
					half dDelta = fwidth( d );

					// Interpolate fill and line colors.
					half innerT = smoothstep( 0.0, -dDelta, d );
					fixed4 col = lerp( strokeCol, fillCol, innerT );

					// Apply smooth edge.
					half edgePos = 1 - innerRadiusRel;
					col.a *= smoothstep( edgePos, edgePos - dDelta * ANTIALIAS_SIZE, d );
				#else
					fixed4 col = lerp( fillCol, strokeCol, ceil( d ) );
				#endif

				// Support fog.
				UNITY_APPLY_FOG( i.fogCoord, col );

				return col;
			}
			ENDCG
		}
	}
}