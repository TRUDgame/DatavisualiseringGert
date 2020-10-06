/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

using UnityEngine;

public static class DrawMath
{
	/// <summary>
	/// Finds the intersection point between two continous lines. Returns true on success.
	/// </summary>
	public static bool TryIntersectLineLine( Vector2 lineAp1, Vector2 lineAp2, Vector2 lineBp1, Vector2 lineBp2, out Vector2 intersection )
	{
		intersection = Vector2.zero;

		float x2x1 = lineAp2.x - lineAp1.x;
		float y2y1 = lineAp2.y - lineAp1.y;
		float x4x3 = lineBp2.x - lineBp1.x;
		float y4y3 = lineBp2.y - lineBp1.y;
		float d = y4y3 * x2x1 - x4x3 * y2y1;
		if( d == 0 ) return false;
		float ua = ( x4x3 * ( lineAp1.y - lineBp1.y ) - y4y3 * ( lineAp1.x - lineBp1.x ) ) / d;
		intersection.x = lineAp1.x + ua * x2x1;
		intersection.y = lineAp1.y + ua * y2y1;
		return true;
	}


	/// <summary>
	/// Evalutes quadratic bezier at point t for points a, b, c, d.
	///	t varies between 0 and 1, and a and d are the curve points,
	///	b and c are the control points. this can be done once with the
	///	x coordinates and a second time with the y coordinates to get
	///	the location of a bezier curve at t.
	/// </summary>
	public static float QuadraticInterpolation( float a, float b, float c, float d, float t )
	{
		float t1 = 1f - t;
		return a * t1 * t1 * t1 + 3 * b * t * t1 * t1 + 3 * c * t * t * t1 + d * t * t * t;
	}


	/// <summary>
	/// Takes a series of points and fills an array with normalized directions ponting from one to the next.
	/// </summary>
	public static void ComputeNormalizedDirections( Vector2[] points, ref Vector2[] directions, bool wrap = false )
	{
		if( directions == null || directions.Length != points.Length ) directions = new Vector2[ points.Length ];

		Vector2 prev = points[ 0 ];
		if( wrap ) {
			Vector2 dir = prev - points[ points.Length - 1 ];
			dir.Normalize();
			directions[ points.Length - 1 ] = dir;
		}
		for( int p = 1; p < points.Length; p++ ) {
			Vector2 point = points[ p ];
			Vector2 dir = point - prev;
			dir.Normalize();
			directions[ p - 1 ] = dir;
			prev = point;
		}
		if( !wrap ) {
			directions[ points.Length - 1 ] = directions[ points.Length - 2 ];
		}
	}
}
