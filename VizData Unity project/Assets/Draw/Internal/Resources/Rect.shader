/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk

	Instancing
	https://docs.huihoo.com/unity/5.6/Documentation/Manual/GPUInstancing.html
*/

Shader "Hidden/Draw/Rect"
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
				UNITY_DEFINE_INSTANCED_PROP( fixed4, _StrokeColor )
				UNITY_DEFINE_INSTANCED_PROP( half, _StrokeMin )
				UNITY_DEFINE_INSTANCED_PROP( half, _StrokeThickness )
				UNITY_DEFINE_INSTANCED_PROP( half2, _FillExtents )
				UNITY_DEFINE_INSTANCED_PROP( half4, _Roundedness )
			UNITY_INSTANCING_BUFFER_END( Props )

			// From IQ: https://www.iquilezles.org/www/articles/distfunctions2d/distfunctions2d.htm
			half SDRoundedBox( half2 p, half2 b, half4 r ){
				r.xy = ( p.x < 0.0 ) ? r.xy : r.wz;
				r.x = ( p.y < 0.0 ) ? r.x : r.y;
				half2 q = abs( p ) - b + r.x;
				return min( max( q.x, q.y ), 0.0 ) + length( max( q, 0.0 ) ) - r.x;
			}


			fixed4 Frag( ToFrag i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( i ); // Support instanced properties in fragment Shader.

				half2 fillExtents = UNITY_ACCESS_INSTANCED_PROP( Props, _FillExtents );
				half strokeThickness = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeThickness );
				half strokeMin = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeMin );
				half4 roundedness = UNITY_ACCESS_INSTANCED_PROP( Props, _Roundedness );

				float d = SDRoundedBox( i.pos, fillExtents, roundedness );
				half dEdge = strokeMin + strokeThickness;
				if( d > dEdge ) discard;

				// Compute the absolute difference between 'd' at this fragment and 'd' at the neighboring fragments.
				// The actual values of 'd' are picked up from neightbor threads, which are executing in same group
				// on modern graphics cards - so it should be cheap.
				// https://computergraphics.stackexchange.com/questions/61/what-is-fwidth-and-how-does-it-work/64
				half dDelta = fwidth( d );

				// Get instanced properties.
				fixed4 fillCol = UNITY_ACCESS_INSTANCED_PROP( Props, _FillColor );
				fixed4 strokeCol = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeColor );

				// Interpolate fill and line colors.
				half innerT = smoothstep( strokeMin, strokeMin - dDelta, d );
				fixed4 col = lerp( strokeCol, fillCol, innerT );

				// Apply smooth edge.
				half edgeAlpha = smoothstep( dEdge, dEdge - dDelta * ANTIALIAS_SIZE, d );
				col.a *= edgeAlpha;

				UNITY_APPLY_FOG( i.fogCoord, col ); // Support fog.
				return col;
			}

			ENDCG
		}
	}
}