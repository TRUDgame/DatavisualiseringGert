Shader "Hidden/00HardCodedColorWithComments"
{
	SubShader
	{
		// First pass, this referenced by material.SetPass(0).
		Pass
		{
			CGPROGRAM // Begin writing in the CG language.

			#pragma vertex Vert			// Declare that we have a vertex function in this shader named "Vert"
			#pragma fragment Frag		// Declare that we have a fragment function in this shader named "Frag"
		
			// This struct defines what we wish to recieve from out app (GL class or Mesh).
			// You can name it what you want, sometimes you will find it named as "AppData".
			struct ToVert
			{
				float4 vertex : POSITION; // POSITION is one of "predefined" semantics, that is telling the GPU "this is the position".
			};

			// This struct defines what you promise to forward (return) from your vertex function
			// to your fragment function.
			struct ToFrag
			{
				float4 vertex : SV_POSITION; // For the fragment, you need to prepend POSITION with SV_ to make it compatible with multiple platforms.
			};

			// The vertex function. Responsible for per vertex computation.
			ToFrag Vert( ToVert v ) // Receive data from app.
			{
				// Define a struct value that we can fill and forward to the fragment function.
				ToFrag o; // o for Output.

				// Ask Unity to transform your input vertex position from modelspace into world
				// space then into camera space and then into clip space.
				// https://jsantell.com/model-view-projection/
				o.vertex = UnityObjectToClipPos( v.vertex );

				// Forward to your fragment function
				return o;
			}
			
			// The fragment function. Responsible for per output image pixel processing.
			half4 Frag( ToFrag i ) : SV_Target
			{
				return half4( 0.0, 1.0, 0.0, 1.0 );
			}

			ENDCG
		}
	}
}