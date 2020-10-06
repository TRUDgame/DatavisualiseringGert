/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

Shader "Hidden/Draw/CircleOrRing"
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
				
			UNITY_INSTANCING_BUFFER_END( Props )


			half SDRing( half2 p, half2 r )
			{
				return abs( length( p ) - r.x ) - r.y;
			}
			

			ToFrag Vert( ToVert v )
			{
				ToFrag o;

				UNITY_SETUP_INSTANCE_ID( v );			// Support instancing
				UNITY_TRANSFER_INSTANCE_ID( v, o );		// Support instanced properties in fragment Shader.

				o.vertex = UnityObjectToClipPos( v.vertex );
				o.pos = v.vertex.xy * GetModelScale2D();

				UNITY_TRANSFER_FOG( o, o.vertex ); 		// Support fog.

				return o;
			}
			

			fixed4 Frag( ToFrag i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( i ); // Support instanced properties in fragment Shader.

				half2 fillExtents = UNITY_ACCESS_INSTANCED_PROP( Props, _FillExtents );
				half strokeThickness = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeThickness );
				half strokeOffsetMin = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeOffsetMin );

				half d = SDRing( i.pos, fillExtents ) - strokeOffsetMin;
				if( d > strokeThickness ) discard;

				fixed4 fillCol = UNITY_ACCESS_INSTANCED_PROP( Props, _FillColor );
				fixed4 strokeCol = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeColor );
				
				return EvaluateFillStrokeColor( d, strokeThickness, fillCol, strokeCol );
			}
			ENDCG
		}
	}
}