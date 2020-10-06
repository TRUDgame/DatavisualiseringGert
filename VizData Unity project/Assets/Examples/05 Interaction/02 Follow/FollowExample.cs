/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Draw;

public class FollowExample : MonoBehaviour
{
	public float speedMax = 10;
	float influenceRadius = 5;
	Vector2 circlePosition = new Vector2( 2, 3 );
	float circleRadius = 0.5f;


	void Update()
	{
		Vector2 mouseWorldPosition = GetZeroPlaneMousePosition();

		//circlePosition = Vector3.Lerp( circlePosition, mouseWorldPosition, Time.deltaTime * 5 );

		Vector3 towardsMouse = mouseWorldPosition - circlePosition;
		float mouseDistance = towardsMouse.magnitude;

		if( mouseDistance < influenceRadius ) {
			float forceMult = mouseDistance / influenceRadius; // Normalized value.
			forceMult = 1 - forceMult; // Invert
			towardsMouse /= mouseDistance;// Normalize vector
			towardsMouse *= forceMult * speedMax;
			circlePosition = circlePosition + (Vector2) towardsMouse * Time.deltaTime;
		}


		SetFillColor( Color.white );
		SetNoStroke();
		DrawCircle( circlePosition, circleRadius * 2 );

		SetFillColor( new Color( 1, 1, 1, 0.1f ));
		DrawCircle( circlePosition, influenceRadius * 2 );
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
}