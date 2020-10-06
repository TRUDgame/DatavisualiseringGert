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
			#pragma multi_compile_local __ _ANTIALIAS

			#include "UnityCG.cginc"
			#include "SDFShapeBase.cginc"

			UNITY_INSTANCING_BUFFER_START( Props )
				UNITY_DEFINE_INSTANCED_PROP( fixed4, _FillColor )
				UNITY_DEFINE_INSTANCED_PROP( fixed4, _StrokeColor )
				UNITY_DEFINE_INSTANCED_PROP( half, _StrokeOffsetMin )
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


			ToFrag Vert( ToVert v )
			{
				ToFrag o;

				UNITY_SETUP_INSTANCE_ID( v );			// Support instancing
				UNITY_TRANSFER_INSTANCE_ID( v, o );		// Support instanced properties in fragment Shader.

				o.pos = v.vertex.xy * GetModelScale2D(); // Grab vertex and scale before expanding for antialiasing.

				#ifdef _ANTIALIAS
					// When applying edge antialiasing, we need to expand the quad to compensate for shrinking. Otherwise shapes won't align.
					// This is perhaps more important for rects than other shapes.
					// https://forum.unity.com/threads/need-help-fixed-size-billboard.688054/#post-4604032
					float4 pivotViewPos = mul( UNITY_MATRIX_V, float4( unity_ObjectToWorld._m03_m13_m23, 1.0 ) );
					v.vertex *= 1 - pivotViewPos.z * ANTIALIAS_SIZE * 1.2 / _ScreenParams.y;
				#endif

				o.vertex = UnityObjectToClipPos( v.vertex );
				UNITY_TRANSFER_FOG( o, o.vertex ); // Support fog.

				return o;
			}
			
			
			fixed4 Frag( ToFrag i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( i ); // Support instanced properties in fragment Shader.

				half2 fillExtents = UNITY_ACCESS_INSTANCED_PROP( Props, _FillExtents );
				half strokeThickness = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeThickness );
				half strokeOffsetMin = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeOffsetMin );
				half4 roundedness = UNITY_ACCESS_INSTANCED_PROP( Props, _Roundedness );

				half d = SDRoundedBox( i.pos, fillExtents, roundedness ) - strokeOffsetMin;
				if( d > strokeThickness ) discard;

				fixed4 fillCol = UNITY_ACCESS_INSTANCED_PROP( Props, _FillColor );
				fixed4 strokeCol = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeColor );

				return EvaluateFillStrokeColor( d, strokeThickness, fillCol, strokeCol );
			}

			ENDCG
		}
	}
}