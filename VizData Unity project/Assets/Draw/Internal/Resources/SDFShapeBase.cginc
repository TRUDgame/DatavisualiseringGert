/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

#define ANTIALIAS_SIZE 1.2


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


fixed4 EvaluateFillStrokeColor( half d, half strokeThickness, fixed4 fillCol, fixed4 strokeCol )
{
	#ifdef _ANTIALIAS
		// Compute the absolute difference between 'd' at this fragment and 'd' at the neighboring fragments.
		// The actual values of 'd' are picked up from neightbor threads, which are executing in same group
		// on modern graphics cards - so it should be cheap.
		// https://computergraphics.stackexchange.com/questions/61/what-is-fwidth-and-how-does-it-work/64
		half dDelta = fwidth( d ) * ANTIALIAS_SIZE;

		// Interpolate fill and line colors.
		half innerT = smoothstep( 0, -dDelta, d );
		fixed4 col = lerp( strokeCol, fillCol, innerT );

		// Apply smooth edge.
		col.a *= smoothstep( strokeThickness, strokeThickness - dDelta, d );
	#else
		fixed4 col = lerp( fillCol, strokeCol, ceil( d ) );
	#endif

	UNITY_APPLY_FOG( i.fogCoord, col ); // Support fog.

	return col;
}


fixed4 EvaluateStrokeColor( half d, fixed4 strokeCol )
{
	#ifdef _ANTIALIAS
		// Compute the absolute difference between 'd' at this fragment and 'd' at the neighboring fragments.
		// The actual values of 'd' are picked up from neightbor threads, which are executing in same group
		// on modern graphics cards - so it should be cheap.
		// https://computergraphics.stackexchange.com/questions/61/what-is-fwidth-and-how-does-it-work/64
		half dDelta = fwidth( d ) * ANTIALIAS_SIZE;

		// Apply smooth edge.
		strokeCol.a *= smoothstep( 0.0, -dDelta, d );
	#endif

	UNITY_APPLY_FOG( i.fogCoord, strokeCol ); // Support fog.

	return strokeCol;
}


float2 GetModelScale2D()
{
	return float2(
		length( unity_ObjectToWorld._m00_m10_m20 ),
		length( unity_ObjectToWorld._m01_m11_m21 )
	);
}