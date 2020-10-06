/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

using UnityEngine;
using static Draw;

namespace DrawExamples
{
	[ExecuteInEditMode]
	public class ShapeLineUpExample : MonoBehaviour
	{
		[Header("Global")]
		[Range( 0.0f, 0.2f )] public float strokeThickness = 0.15f;
		public StrokeAlignment strokeAlignment = StrokeAlignment.Inside;
		public Cap beginCap = Cap.Round;
		public Cap endCap = Cap.Round;
		[Range(0,360)] public float rotation = 0;
		public Pivot pivot = Pivot.Center;
		public bool applyAntialiasing = true;
		public bool displayPivots = false;
		public bool displayPoints = false;

		[Header("Ring")]
		[Range(0,1)] public float ringThickness = 0.5f;

		[Header("Pie")]
		public float pieAngleMin = -20;
		public float pieAngleMax = 100;

		[Header( "Arc" )]
		public float arcAngleMin = -30;
		public float arcAngleMax = 80;
		[Range(0,1)] public float arcInnerRadiusRelative = 0.5f;

		[Header("Rect")]
		[Range(0,1)] public float lowerLeftRoundness = 0.2f;
		[Range(0,1)] public float upperLeftRoundness = 0.2f;
		[Range(0,1)] public float upperRightRoundness = 0.2f;
		[Range(0,1)] public float lowerRightRoundness = 0.2f;

		Polygon _polygon;
		Polyline _polyline;

		const float strokeAlpha = 0.3f;
		const float xOffset = 1.5f;
		const float pointDiameter = 0.05f;
		const float pointStrokeThickness = 0.01f;


		void Update()
		{
			SetStrokeThickness( strokeThickness );
			SetStrokeAlignement( strokeAlignment );
			SetPivot( pivot );
			SetAntialiasing( applyAntialiasing );

			Color color;
			float x = -xOffset;
			float hStep = 0.1f;
			float h = -hStep;

			color = Color.HSVToRGB( h+=hStep, 0.7f, 1 );
			x += xOffset;
			SetFillColor( color );
			SetStrokeColor( color, strokeAlpha );
			DrawCircle( x, 0, 1, rotation );
			color = Color.white;

			x += xOffset;
			color = Color.HSVToRGB( h+=hStep, 0.7f, 1 );
			SetFillColor( color );
			SetStrokeColor( color, strokeAlpha );
			DrawRing( x, 0, 1-ringThickness, 1f, rotation );

			color = Color.HSVToRGB( h+=hStep, 0.7f, 1 );
			x += xOffset;
			SetFillColor( color );
			SetStrokeColor( color, strokeAlpha );
			DrawPie( x, 0, 1, pieAngleMin, pieAngleMax, rotation );

			color = Color.HSVToRGB( h+=hStep, 0.7f, 1 );
			x += xOffset;
			SetFillColor( color );
			SetStrokeColor( color, strokeAlpha );
			DrawArc( x, 0, arcInnerRadiusRelative, 1, arcAngleMin, arcAngleMax, rotation );

			color = Color.HSVToRGB( h+=hStep, 0.7f, 1 );
			x += xOffset;
			SetFillColor( color );
			SetStrokeColor( color, strokeAlpha );
			DrawRect( x, 0, 1, 1, lowerLeftRoundness, upperLeftRoundness, upperRightRoundness, lowerRightRoundness, rotation );

			if( _polygon is null ) _polygon = MakeAPolygon();
			color = Color.HSVToRGB( h+=hStep, 0.7f, 1 );
			x += xOffset;
			SetFillColor( color );
			DrawPolygon( _polygon, x, 0, rotation );

			if( _polyline is null ) _polyline = MakeAPolyline();
			color = Color.HSVToRGB( h+=hStep, 0.7f, 1 );
			x += xOffset;
			SetStrokeColor( color );
			DrawPolyline( _polyline, x, 0, beginCap, endCap, rotation );

			color = Color.HSVToRGB( h+=hStep, 0.7f, 1 );
			x += xOffset;
			SetStrokeColor( color );
			PushCanvas();
			TranslateCanvas( x, 0 );
			DrawLine( 0, -0.5f, 0, 0.5f, beginCap, endCap, rotation );
			PopCanvas();

			if( displayPivots ) DrawPivots();
			if( displayPoints ) DrawPoints();
		}


		void DrawPivots()
		{
			PushCanvas();
			TranslateCanvas( 0, 0, -0.01f ); // Place in front.

			float x = 0;
			SetPivot( Pivot.Center );
			SetFillColor( Color.red );
			SetStrokeColor( Color.black );
			SetStrokeThickness( pointStrokeThickness );
			SetStrokeAlignement( StrokeAlignment.Outside );
			for( int p = 0; p < 7; p++ ) {
				DrawCircle( x, 0, pointDiameter );
				x += xOffset;
			}

			PopCanvas();
		}


		void DrawPoints()
		{
			SetFillColor( Color.HSVToRGB( 0.6f, 0.7f, 1 ) );
			SetStrokeColor( Color.black );
			SetStrokeThickness( pointStrokeThickness );
			SetStrokeAlignement( StrokeAlignment.Outside );

			PushCanvas();
			TranslateCanvas( 0, 0, -0.01f ); // Place in front.

			DrawDebug.DrawPolygonPoints( _polygon, xOffset * 5, 0, pointDiameter, rotation );
			DrawDebug.DrawPolylinePoints( _polyline, xOffset * 6, 0, pointDiameter, rotation );
			PushCanvas();
			TranslateCanvas( xOffset * 7, 0 );
			DrawDebug.DrawLinePoints( 0, -0.5f, 0, 0.5f, pointDiameter, rotation );
			PopCanvas();

			PopCanvas();
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
				float y = Mathf.Lerp( -0.5f, 0.5f, t );
				float x = Mathf.Lerp( -0.5f, 0.5f, ( p % 2 ) );
				if( p == 2 ) x *= 0.5f;
				polyline.SetPoint( p, x, y );
			}
			return polyline;
		}
	}
}