/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk

	Learn about Phyllotaxis with Daniel Shiffman
	https://www.youtube.com/watch?v=KWoJgHFYWxY
*/

using UnityEngine;
using static Draw;

namespace DrawExamples
{
	[ExecuteInEditMode]
	public class InstancingExample : MonoBehaviour
	{
		public int count = 64;
		public Mode mode = Mode.Circles;

		const float shapeSize = 0.05f;
		const int shapeGroupCount = 4;

		public enum Mode { Circles, MixedAlternating, MixedRandom, MixedSorted }


		void Update()
		{
			SetNoStroke();

			Random.InitState( 0 );
			for( int i = 1; i < count+1; i++ )
			{
				float a = i * 137.5f * Mathf.Deg2Rad;
				float r = shapeSize * Mathf.Sqrt( i );
				float x = Mathf.Cos( a ) * r;
				float y = Mathf.Sin( a ) * r;

				int modeInt = (int) mode;
				if( mode == Mode.MixedAlternating ) modeInt = i % shapeGroupCount;
				else if( mode == Mode.MixedRandom ) modeInt = Random.Range( 0, shapeGroupCount );
				else if( mode == Mode.MixedSorted ) modeInt = (int) ( 4 * Mathf.Pow( i / (float) count, 0.5f ) );

				switch( modeInt )
				{
					case 0:
						DrawCircle( x, y, shapeSize );
						break;
					case 1:
						float pieAngleBegin = Random.value * 360;
						float pieAngleSpan = Random.Range( 180, 350 );
						DrawPie( x, y, shapeSize, pieAngleBegin, pieAngleBegin + pieAngleSpan );
						break;
					case 2:
						float arcAngleBegin = Random.value * 360;
						float arcAngleSpan = Random.Range( 180, 350 );
						DrawArc( x, y, shapeSize*0.4f, shapeSize, arcAngleBegin, arcAngleBegin + arcAngleSpan );
						break;
					case 3:
						DrawRect( x, y, shapeSize, shapeSize, 0.5f );
						break;
				}
			}
		}
	}
}