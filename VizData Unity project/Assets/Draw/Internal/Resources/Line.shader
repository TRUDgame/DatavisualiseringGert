/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

Shader "Hidden/Draw/Line"
{
	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM

			#pragma vertex VertLine
			#pragma fragment FragLine

			#pragma multi_compile_fog 				// Support fog.
			#pragma multi_compile_instancing		// Support instancing

			#include "UnityCG.cginc"
			#include "SDFShapeBase.cginc"

			struct ToVertLine
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};


			struct ToFragLine
			{
				float4 vertex : SV_POSITION;
				float2 pos : TEXCOORD0;
				UNITY_FOG_COORDS( 1 ) 					// Support fog.
				UNITY_VERTEX_INPUT_INSTANCE_ID 			// Support instanced properties in fragment Shader.
			};


			UNITY_INSTANCING_BUFFER_START( Props )
				UNITY_DEFINE_INSTANCED_PROP( fixed4, _StrokeColor )
				UNITY_DEFINE_INSTANCED_PROP( half2, _FillExtents )
				UNITY_DEFINE_INSTANCED_PROP( fixed2, _RoundedCapFlags )
			UNITY_INSTANCING_BUFFER_END( Props )


			ToFragLine VertLine( ToVertLine v )
			{
				ToFragLine o;

				UNITY_SETUP_INSTANCE_ID( v );			// Support instancing
				UNITY_TRANSFER_INSTANCE_ID( v, o );		// Support instanced properties in fragment Shader.

				o.vertex = UnityObjectToClipPos( v.vertex );
				o.pos = v.vertex.xy;

				UNITY_TRANSFER_FOG( o, o.vertex ); 		// Support fog.

				return o;
			}


			fixed4 FragLine( ToFragLine i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( i ); // Support instanced properties in fragment Shader.

				half2 extents = UNITY_ACCESS_INSTANCED_PROP( Props, _FillExtents );
				half2 roundCapFlags = UNITY_ACCESS_INSTANCED_PROP( Props, _RoundedCapFlags );

				// Similar to a rounded rect SDF. https://iquilezles.org/www/articles/distfunctions/distfunctions.htm
				half2 p = i.pos * extents;
				half r = ( ( p.x < 0.0 ) ? roundCapFlags.x : roundCapFlags.y ) * extents.y;
				half2 q = abs( p ) - extents + r;
				half d = min( max( q.x, q.y ), 0.0 ) + length( max( q, 0.0 ) ) - r;
				if( d > 0 ) discard;

				// Compute the absolute difference between 'd' at this fragment and 'd' at the neighboring fragments.
				// The actual values of 'd' are picked up from neightbor threads, which are executing in same group
				// on modern graphics cards - so it should be cheap.
				// https://computergraphics.stackexchange.com/questions/61/what-is-fwidth-and-how-does-it-work/64
				half dDelta = fwidth( d );

				// Get instanced properties.
				fixed4 col = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeColor );

				// Apply smooth edge.
				half edgeAlpha = smoothstep( 0.0, 0.0 - dDelta * ANTIALIAS_SIZE, d );
				col.a *= edgeAlpha;

				UNITY_APPLY_FOG( i.fogCoord, col ); // Support fog.
				return col;
			}

			ENDCG
		}
	}
}
