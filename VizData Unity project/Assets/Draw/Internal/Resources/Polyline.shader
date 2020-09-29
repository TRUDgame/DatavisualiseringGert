/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

Shader "Hidden/Draw/Polyline"
{
	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM

			#pragma vertex VertPolyline
			#pragma fragment FragPolyline

			#pragma multi_compile_fog 				// Support fog.
			#pragma multi_compile_instancing		// Support instancing

			#include "UnityCG.cginc"
			#include "SDFShapeBase.cginc"

			// From IQ: https://www.iquilezles.org/www/articles/distfunctions2d/distfunctions2d.htm
			float sdSegment( float2 p, float2 a, float2 b )
			{
				float2 pa = p - a, ba = b - a;
				float h = clamp( dot( pa, ba ) / dot( ba, ba ), 0.0, 1.0 );
				return length( pa - ba * h );
			}

			struct ToVertPolyline
			{
				float4 vertex : POSITION;
				float4 points : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct ToFragPolyline
			{
				float4 vertex : SV_POSITION;
				float2 pos : TEXCOORD0;
				nointerpolation float4 points : TEXCOORD1;
				UNITY_FOG_COORDS( 1 ) 					// Support fog.
				UNITY_VERTEX_INPUT_INSTANCE_ID 			// Support instanced properties in fragment Shader.
			};

			UNITY_INSTANCING_BUFFER_START( Props )
				UNITY_DEFINE_INSTANCED_PROP( fixed4, _StrokeColor )
				UNITY_DEFINE_INSTANCED_PROP( half, _HalfStrokeThickness )
			UNITY_INSTANCING_BUFFER_END( Props )
			

			ToFragPolyline VertPolyline( ToVertPolyline v )
			{
				ToFragPolyline o;

				UNITY_SETUP_INSTANCE_ID( v );			// Support instancing
				UNITY_TRANSFER_INSTANCE_ID( v, o );		// Support instanced properties in fragment Shader.

				o.vertex = UnityObjectToClipPos( v.vertex );
				o.pos = v.vertex.xy;
				o.points = v.points;

				UNITY_TRANSFER_FOG( o, o.vertex ); 		// Support fog.
				 
				return o;
			}
			

			fixed4 FragPolyline( ToFragPolyline i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( i ); // Support instanced properties in fragment Shader.
				
				half halfStrokeThickness = UNITY_ACCESS_INSTANCED_PROP( Props, _HalfStrokeThickness );

				float d = sdSegment( i.pos, i.points.xy, i.points.zw );
				if( d > halfStrokeThickness ) discard;

				fixed4 col = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeColor );

				// Apply smooth edge.
				half dDelta = fwidth( d );
				half edgeAlpha = smoothstep( halfStrokeThickness, halfStrokeThickness - dDelta * ANTIALIAS_SIZE, d );
				col.a *= edgeAlpha;

				// Support fog.
				UNITY_APPLY_FOG( i.fogCoord, col );

				return col;
			}
			ENDCG
		}
	}
}