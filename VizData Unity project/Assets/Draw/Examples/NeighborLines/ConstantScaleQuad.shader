Shader "Unlit/Constant Scale Quad"
{
	Properties
	{
		_MainTex( "Texture", 2D ) = "white" {}
	}
		SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

	Pass
	{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		#include "UnityCG.cginc"

		struct v2f
		{
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
		};

		sampler2D _MainTex;

		v2f vert( appdata_base v )
		{
			v2f o;

			// extract world pivot position from object to world transform matrix
			float3 pivotWorldPos = unity_ObjectToWorld._m03_m13_m23;

			// extract x and y scale from object to world transform matrix
			float2 scale = float2(
				length( unity_ObjectToWorld._m00_m10_m20 ),
				length( unity_ObjectToWorld._m01_m11_m21 )
			);

			// transform pivot position into view space
			float4 viewPos = mul( UNITY_MATRIX_V, float4( pivotWorldPos, 1.0 ) );

			// apply transform scale to xy vertex positions
			float2 vertex = v.vertex.xy * scale;

			// multiply by view depth for constant view size scaling
			vertex *= viewPos.z;

			// divide by perspective projection matrix [1][1] if you don't want camera FOV to displayed size
			// the * 0.5 is to make a default quad with a scale of 1 be exactly the height of the view
			//vertex /= UNITY_MATRIX_P._m11 * 0.5;

			// along with the perspective projection divide by screen height if you want the scale to be in screen pixels
			//vertex /= _ScreenParams.y;

			// add vertex positions to view position pivot
			viewPos.xy += vertex;

			// transform into clip space
			o.pos = mul( UNITY_MATRIX_P, viewPos );

			o.uv = v.texcoord;
			return o;
		}

		fixed4 frag( v2f i ) : SV_Target
		{
			fixed4 col = tex2D( _MainTex, i.uv );
			return col;
		}
			ENDCG
		}
	}
}