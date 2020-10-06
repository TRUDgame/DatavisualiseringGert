/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Draw;

public class RepulseNeighbors : MonoBehaviour
{
	public int circleCount = 10;
	public float repulsionForce = 5;
	public float centerForce = 0.1f;
	public float padding = 0.2f;

	Vector2[] positions;
	float[] radiuses;

	void Awake()
	{
		positions = new Vector2[ circleCount ];
		radiuses = new float[ circleCount ];
		for( int i = 0; i < circleCount; i++ ) {
			positions[ i ] = Random.insideUnitCircle * 5;
			radiuses[ i ] = Random.Range( 0.2f, 0.5f );
		}
	}


	void Update()
	{
		SetNoStroke();


		for( int c1 = 0; c1 < circleCount; c1++ ) {
			Vector2 pos1 = positions[ c1 ];
			float radius1 = radiuses[ c1 ];

			// Add neightbor force.
			for( int c2 = 0; c2 < c1; c2++ ) {

				Vector2 pos2 = positions[ c2 ];
				float radius2 = radiuses[ c2 ];

				Vector2 towards2 = pos2 - pos1;
				float sqrDistace = towards2.sqrMagnitude;
				float collisionDistance = radius1 + radius2 + padding;
				float sqrCollisionDistance = collisionDistance * collisionDistance;
				if( sqrDistace < sqrCollisionDistance ) {
					float distance = Mathf.Sqrt( sqrDistace );
					float forceMult = distance / collisionDistance; // normalize.
					forceMult = 1 - forceMult; // Invert.
					towards2 /= distance; // Normalize vector.
					towards2 *= forceMult * repulsionForce;

					pos1 -= towards2 * Time.deltaTime;
					pos2 += towards2 * Time.deltaTime;

					positions[ c2 ] = pos2; // Update!
				}
			}

			// Add center force.
			Vector2 center = Vector2.zero;
			Vector2 towardsCenter = center - pos1;
			pos1 += towardsCenter * centerForce * Time.deltaTime;

			positions[ c1 ] = pos1; // Update!
		}

		for( int i = 0; i < circleCount; i++ ) {
			DrawCircle( positions[ i ], radiuses[ i ] * 2 );
		}
	}
}
