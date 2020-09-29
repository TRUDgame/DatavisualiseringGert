/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

using UnityEngine;
using static Draw;

[ExecuteInEditMode]
public class ShapeLineUpExample : MonoBehaviour
{
	[Header("Global")]
	[Range( 0.0f, 0.2f )] public float strokeThickness = 0.15f;
	public StrokeAlignment strokeAlignment = StrokeAlignment.Inside;
	[Range(0,360)] public float rotation = 0;

	[Header("Rect")]
	[Range(0,1)] public float lowerLeftRoundness = 0.2f;
	[Range(0,1)] public float upperLeftRoundness = 0.2f;
	[Range(0,1)] public float upperRightRoundness = 0.2f;
	[Range(0,1)] public float lowerRightRoundness = 0.2f;

	[Header("Pie")]
	public float pieAngleMin = -20;
	public float pieAngleMax = 100;

	[Header( "Arc" )]
	public float arcAngleMin = -30;
	public float arcAngleMax = 80;
	[Range(0,1)] public float arcInnerRadiusRelative = 0.5f;

	Polygon _polygon;
	Polyline _polyline;


	void Update()
	{
		SetStrokeThickness( strokeThickness );
		SetStrokeAlignement( strokeAlignment );

		SetFillColor( Color.white );
		SetStrokeColor( Color.gray );
		DrawCircle( 0, 0, 1 );

		SetFillColor( Color.HSVToRGB( 0.0f, 0.7f, 1 ) );
		SetStrokeColor( Color.HSVToRGB( 0.0f, 0.6f, 0.5f ) );
		DrawRect( 1.5f, 0, 1, 1, lowerLeftRoundness, upperLeftRoundness, upperRightRoundness, lowerRightRoundness, rotation );

		SetFillColor( Color.HSVToRGB( 0.1f, 0.7f, 1 ) );
		SetStrokeColor( Color.HSVToRGB( 0.1f, 0.6f, 0.5f ) );
		DrawPie( 3f, 0, 1, pieAngleMin, pieAngleMax, rotation );

		SetFillColor( Color.HSVToRGB( 0.2f, 0.8f, 1 ) );
		SetStrokeColor( Color.HSVToRGB( 0.2f, 0.6f, 0.5f ) );
		DrawArc( 4.5f, 0, arcInnerRadiusRelative, 1, arcAngleMin, arcAngleMax, rotation );

		if( _polygon is null ) _polygon = MakeAPolygon();
		SetFillColor( Color.HSVToRGB( 0.35f, 0.7f, 1 ) );
		DrawPolygon( _polygon, 6, 0, rotation );

		if( _polyline is null ) _polyline = MakeAPolyline();
		SetStrokeColor( Color.HSVToRGB( 0.5f, 0.7f, 1 ) );
		DrawPolyline( _polyline, 7.5f, 0, rotation );

		SetStrokeColor( Color.HSVToRGB( 0.7f, 0.7f, 1 ) );
		DrawLine( 9f, -0.5f, 9f, 0.5f, Cap.Rounded, Cap.Rounded, rotation );
	}


	static Polygon MakeAPolygon()
	{
		Polygon polygon = new Polygon();
		polygon.SetPointCount( 10 );
		for( int p = 0; p < polygon.pointCount; p++ ) {
			float a = Mathf.InverseLerp( 0, polygon.pointCount, p ) * Mathf.PI * 2;
			float r = Mathf.Lerp( 0.3f, 0.5f, p % 2 );
			float x = Mathf.Cos( a ) * r;
			float y = Mathf.Sin( a ) * r;
			polygon.SetPoint( p, x, y );
		}
		return polygon;
	}

	static Polyline MakeAPolyline()
	{
		Polyline polyline = new Polyline();
		polyline.SetPointCount( 5 );
		for( int p = 0; p < polyline.pointCount; p++ ) {
			float t = Mathf.InverseLerp( 0, polyline.pointCount-1, p );
			float x = Mathf.Lerp( -0.5f, 0.5f, p % 2 );
			float y = Mathf.Lerp( -0.5f, 0.5f, t );
			polyline.SetPoint( p, x, y );
		}
		return polyline;
	}
}