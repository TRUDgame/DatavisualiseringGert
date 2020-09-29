﻿/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

using UnityEngine;
using static Draw;

[ExecuteInEditMode]
public class BezierCurveExample : MonoBehaviour
{
	public Transform anchorALocator;
	public Transform controlALocator;
	public Transform controlBLocator;
	public Transform anchorBLocator;
	public int resolution = 32;
	public float strokeThickness = 0.2f;

	Polyline _polyline = new Polyline();


	void Update()
	{
		if( !anchorALocator || !controlALocator || !controlBLocator || !anchorBLocator ) return;

		SetStrokeThickness( strokeThickness );
		SetStrokeColor( Color.white, 0.8f );
		_polyline.SetBezierCurve( anchorALocator.position, controlALocator.position, controlBLocator.position, anchorBLocator.position, resolution );
		DrawPolyline( _polyline, 0, 0 );

		DrawGuides();
	}


	void DrawGuides()
	{
		PushCanvas();
		TranslateCanvas( 0, 0, 0.01f ); // Draw behind the bezier line.

		SetStrokeThickness( 0.02f );
		SetStrokeColor( Color.red );
		DrawLine( anchorALocator.position, controlALocator.position );
		DrawLine( anchorBLocator.position, controlBLocator.position );

		SetFillColor( Color.red );
		SetNoStroke();
		DrawCircle( anchorALocator.position, 0.05f );
		DrawCircle( controlALocator.position, 0.05f );
		DrawCircle( controlBLocator.position, 0.05f );
		DrawCircle( anchorBLocator.position, 0.05f );

		PopCanvas();
	}
}