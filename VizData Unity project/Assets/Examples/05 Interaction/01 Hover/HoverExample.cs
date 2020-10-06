/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Draw;

public class HoverExample : MonoBehaviour
{

	Vector2 circlePosition = new Vector2( 2, 3 );
	float circleRadius = 0.5f;


	void Update()
	{
		SetNoStroke();

		bool isInside = InsideCircle( circlePosition, circleRadius );

		if( isInside ) {
			SetFillColor( Color.green );
		} else {
			SetFillColor( Color.white );
		}
		DrawCircle( circlePosition, circleRadius * 2 );
	}


	static Vector3 GetZeroPlaneMousePosition()
	{
		Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
		Plane canvasPlane = new Plane( Vector3.back, Vector3.zero );
		float hitDistance;
		bool hitSucceess = canvasPlane.Raycast( ray, out hitDistance );
		if( hitSucceess ) return ray.origin + ray.direction * hitDistance;
		return Vector3.zero;
	}


	public static bool InsideCircle( Vector3 position, float radius )
	{
		Vector3 mouseWorldPosition = GetZeroPlaneMousePosition();

		SetFillColor( Color.red );
		DrawCircle( mouseWorldPosition, 0.1f );

		float circleDistace = Vector3.Distance( mouseWorldPosition, position );
		if( circleDistace < radius ) {
			return true;
		}

		return false;
	}
}
