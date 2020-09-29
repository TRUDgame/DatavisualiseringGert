﻿/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

Shader "Hidden/Draw/Pie"
{
	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM

			#pragma vertex PieVert
			#pragma fragment Frag

			#pragma multi_compile_fog 				// Support fog.
			#pragma multi_compile_instancing		// Support instancing

			#include "UnityCG.cginc"
			#include "SDFShapeBase.cginc"

			struct PieToFrag
			{
				float4 vertex : SV_POSITION;
				float2 pos : TEXCOORD0;
				nointerpolation float2 c : TEXCOORD1;	// Sincos to angle extents.
				UNITY_FOG_COORDS( 2 ) 					// Support fog.
					UNITY_VERTEX_INPUT_INSTANCE_ID 			// Support instanced properties in fragment Shader.
			};


			UNITY_INSTANCING_BUFFER_START( Props )
				UNITY_DEFINE_INSTANCED_PROP( fixed4, _FillColor )
				UNITY_DEFINE_INSTANCED_PROP( fixed4, _StrokeColor )
				UNITY_DEFINE_INSTANCED_PROP( half, _StrokeMin )
				UNITY_DEFINE_INSTANCED_PROP( half, _StrokeThickness )
				UNITY_DEFINE_INSTANCED_PROP( half, _FillExtents )
				UNITY_DEFINE_INSTANCED_PROP( half, _AngleExtents )
			UNITY_INSTANCING_BUFFER_END( Props )


			PieToFrag PieVert( ToVert v )
			{
				PieToFrag o;

				UNITY_SETUP_INSTANCE_ID( v );			// Support instancing
				UNITY_TRANSFER_INSTANCE_ID( v, o );		// Support instanced properties in fragment Shader.

				o.vertex = UnityObjectToClipPos( v.vertex );
				o.pos = GetModelScale2D() * v.vertex.xy;

				half angleExtents = UNITY_ACCESS_INSTANCED_PROP( Props, _AngleExtents );
				half2 c;
				sincos( angleExtents, c.x, c.y );
				o.c = c;

				UNITY_TRANSFER_FOG( o, o.vertex ); 		// Support fog.

				return o;
			}

			// From IQ: https://www.iquilezles.org/www/articles/distfunctions2d/distfunctions2d.htm
			half SdPie( half2 p, half2 c, half r )
			{
				p.x = abs( p.x );
				float l = length( p ) - r;
				float m = length( p - c * clamp( dot( p, c ), 0.0, r ) ); // c = sin/cos of the aperture
				return max( l, m * sign( c.y * p.x - c.x * p.y ) );
			}


			fixed4 Frag( PieToFrag i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( i ); // Support instanced properties in fragment Shader.
				
				half fillExtents = UNITY_ACCESS_INSTANCED_PROP( Props, _FillExtents );
				half strokeMin = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeMin );
				half strokeThickness = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeThickness );
				
				half d = SdPie( i.pos, i.c, fillExtents );
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

				// Support fog.
				UNITY_APPLY_FOG( i.fogCoord, col );

				return col;
			}

			ENDCG
		}
	}
}