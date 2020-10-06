/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

Shader "Hidden/Draw/Arc"
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

			struct ArcToFrag
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
				UNITY_DEFINE_INSTANCED_PROP( half, _StrokeOffsetMin )
				UNITY_DEFINE_INSTANCED_PROP( half, _StrokeThickness )
				UNITY_DEFINE_INSTANCED_PROP( half2, _FillExtents )
				UNITY_DEFINE_INSTANCED_PROP( half, _AngleExtents )
			UNITY_INSTANCING_BUFFER_END( Props )

			// From IQ: https://www.iquilezles.org/www/articles/distfunctions2d/distfunctions2d.htm
			half SdArc( half2 p, half2 c, half r, half w )
			{
				p.x = abs( p.x );
				half l = length( p );
				p = mul( half2x2( -c.y, c.x, c.x, c.y ), p );
				p = half2( ( p.y > 0.0 ) ? p.x : l * sign( -c.y ), ( p.x > 0.0 ) ? p.y : l );
				p = half2( p.x, abs( p.y - r ) - w );
				return length( max( p, 0.0 ) ) + min( 0.0, max( p.x, p.y ) );
			}


			ArcToFrag Vert( ToVert v )
			{
				ArcToFrag o;

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


			fixed4 Frag( ArcToFrag i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( i ); // Support instanced properties in fragment Shader.
				
				half2 fillExtents = UNITY_ACCESS_INSTANCED_PROP( Props, _FillExtents );
				half strokeOffsetMin = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeOffsetMin );
				half strokeThickness = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeThickness );
				
				half d = SdArc( i.pos, i.c, fillExtents.x, fillExtents.y ) - strokeOffsetMin;
				if( d > strokeThickness ) discard;

				fixed4 fillCol = UNITY_ACCESS_INSTANCED_PROP( Props, _FillColor );
				fixed4 strokeCol = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeColor );

				return EvaluateFillStrokeColor( d, strokeThickness, fillCol, strokeCol );
			}
			
			ENDCG
		}
	}
}