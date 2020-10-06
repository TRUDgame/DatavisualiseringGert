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

			#pragma vertex Vert
			#pragma fragment Frag

			#pragma multi_compile_fog 				// Support fog.
			#pragma multi_compile_instancing		// Support instancing
			#pragma multi_compile_local __ _ANTIALIAS

			#include "UnityCG.cginc"
			#include "SDFShapeBase.cginc"

			// From IQ: https://www.iquilezles.org/www/articles/distfunctions2d/distfunctions2d.htm
			float sdSegment( float2 p, float2 a, float2 b, float2 r )
			{
				float2 pa = p - a, ba = b - a;
				float sqLength = dot( ba, ba );
				float h = dot( pa, ba ) / sqLength; // Compute normalized position along the segment. Dot( ba, ba ) is square length.
				if( h <= 0 && r.x < 1 ) return max( length( pa - ba * h ), -h * sqrt( sqLength ) );
				if( h >= 1 && r.y < 1 ) return max( length( pa - ba * h ), -(1-h) * sqrt( sqLength ) );
				return length( pa - ba * saturate( h ) );
			}

			struct ToVertPolyline
			{
				float4 vertex : POSITION;
				float4 points : TEXCOORD0;
				float2 roundingFlags : TEXCOORD1; // (1,1) means rounded begin and end caps.
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct ToFragPolyline
			{
				float4 vertex : SV_POSITION;
				float2 pos : TEXCOORD0;
				nointerpolation float4 points : TEXCOORD1;
				nointerpolation float2 roundingFlags : TEXCOORD2;
				UNITY_FOG_COORDS( 3 ) 					// Support fog.
				UNITY_VERTEX_INPUT_INSTANCE_ID 			// Support instanced properties in fragment Shader.
			};

			UNITY_INSTANCING_BUFFER_START( Props )
				UNITY_DEFINE_INSTANCED_PROP( fixed4, _StrokeColor )
				UNITY_DEFINE_INSTANCED_PROP( half, _HalfStrokeThickness )
			UNITY_INSTANCING_BUFFER_END( Props )
			

			ToFragPolyline Vert( ToVertPolyline v )
			{
				ToFragPolyline o;

				UNITY_SETUP_INSTANCE_ID( v );			// Support instancing
				UNITY_TRANSFER_INSTANCE_ID( v, o );		// Support instanced properties in fragment Shader.

				o.vertex = UnityObjectToClipPos( v.vertex );
				o.pos = v.vertex.xy;
				o.points = v.points;
				o.roundingFlags = v.roundingFlags;

				UNITY_TRANSFER_FOG( o, o.vertex ); 		// Support fog.
				 
				return o;
			}
			

			fixed4 Frag( ToFragPolyline i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( i ); // Support instanced properties in fragment Shader.
				
				half halfStrokeThickness = UNITY_ACCESS_INSTANCED_PROP( Props, _HalfStrokeThickness );

				float d = sdSegment( i.pos, i.points.xy, i.points.zw, i.roundingFlags ) - halfStrokeThickness;
				if( d > 0 ) discard;

				fixed4 strokeCol = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeColor );

				return EvaluateStrokeColor( d, strokeCol );
			}
			ENDCG
		}
	}
}