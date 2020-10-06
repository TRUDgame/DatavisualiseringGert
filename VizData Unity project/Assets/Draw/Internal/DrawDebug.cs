/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

using UnityEngine;
using static Draw;

public static class DrawDebug
{
	public static void DrawLinePoints( float ax, float ay, float bx, float by, float pointDiameter, float rotation = 0 )
	{
		Pivot prevPivot = GetPivot();
		SetPivot( Pivot.Center );

		PushCanvas();
		if( rotation != 0 ) RotateCanvas( rotation );

		DrawCircle( new Vector3( ax, ay ), pointDiameter );
		DrawCircle( new Vector3( bx, by ), pointDiameter );

		PopCanvas();

		SetPivot( prevPivot );
	}

	public static void DrawLinePoints( Vector2 positionA, Vector2 positionB, float pointDiameter, float rotation = 0 )
	{
		DrawLinePoints( positionA.x, positionA.y, positionB.x, positionB.y, pointDiameter, rotation );
	}


	public static void DrawPolygonPoints( Polygon polygon, float x, float y, float pointDiameter, float rotation = 0 )
	{
		Pivot prevPivot = GetPivot();
		SetPivot( Pivot.Center );
		PushCanvas();

		TranslateCanvas( x, y );
		if( rotation != 0 ) RotateCanvas( rotation );

		for( int p = 0; p < polygon.pointCount; p++ ) DrawCircle( polygon.GetPoint( p ), pointDiameter );

		PopCanvas();
		SetPivot( prevPivot );
	}


	public static void DrawPolylinePoints( Polyline polyline, float x, float y, float pointDiameter, float rotation = 0 )
	{
		Pivot prevPivot = GetPivot();
		SetPivot( Pivot.Center );
		PushCanvas();

		TranslateCanvas( x, y );
		if( rotation != 0 ) RotateCanvas( rotation );

		for( int p = 0; p < polyline.pointCount; p++ ) DrawCircle( polyline.GetPoint( p ), pointDiameter );

		PopCanvas();
		SetPivot( prevPivot );
	}
}