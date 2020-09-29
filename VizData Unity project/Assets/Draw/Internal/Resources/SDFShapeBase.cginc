/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

#define ANTIALIAS_SIZE 1.5

struct ToVert
{
	float4 vertex : POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};


struct ToFrag
{
	float4 vertex : SV_POSITION;
	float2 pos : TEXCOORD0;
	UNITY_FOG_COORDS( 1 ) 					// Support fog.
	UNITY_VERTEX_INPUT_INSTANCE_ID 			// Support instanced properties in fragment Shader.
};


float2 GetModelScale2D()
{
	return float2( length( unity_ObjectToWorld._m00_m10_m20 ), length( unity_ObjectToWorld._m01_m11_m21 ) );
}


ToFrag Vert( ToVert v )
{
	ToFrag o;

	UNITY_SETUP_INSTANCE_ID( v );			// Support instancing
	UNITY_TRANSFER_INSTANCE_ID( v, o );		// Support instanced properties in fragment Shader.

	o.vertex = UnityObjectToClipPos( v.vertex );
	o.pos = GetModelScale2D() * v.vertex.xy;

	UNITY_TRANSFER_FOG( o, o.vertex ); 		// Support fog.

	return o;
}


ToFrag VertNoScale( ToVert v )
{
	ToFrag o;

	UNITY_SETUP_INSTANCE_ID( v );			// Support instancing
	UNITY_TRANSFER_INSTANCE_ID( v, o );		// Support instanced properties in fragment Shader.

	o.vertex = UnityObjectToClipPos( v.vertex );
	o.pos = v.vertex.xy;

	UNITY_TRANSFER_FOG( o, o.vertex ); 		// Support fog.

	return o;
}