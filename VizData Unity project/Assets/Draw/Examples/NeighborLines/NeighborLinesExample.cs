/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

using UnityEngine;
using static Draw;

namespace DrawExamples
{
	[ExecuteInEditMode]
	public class NeighborLinesExample : MonoBehaviour
	{
		[SerializeField] int _objectCount = 100;
		[SerializeField] float _sceneRadius = 5;
		[SerializeField] float _contactDistace = 0.5f;
		[SerializeField] float _speed = 1.0f;
		[SerializeField] float _lineThickness = 0.05f;
		[SerializeField] bool _displayCircles = true;
		[SerializeField] bool _displayLines = true;
		[SerializeField] bool _logLineCount = false;

		Vector2[] _positions;
		Vector2[] _velocities;
		float _sqSceneRadius;
		float _sqConstactDist;


		void Awake()
		{
			OnValidate();
		}


		void Update()
		{
			int lineCount = 0;

			for( int p1 = 0; p1 < _positions.Length; p1++ )
			{
				// Move position.
				Vector2 pos1 = _positions[ p1 ];
				pos1 += _velocities[ p1 ] * ( _speed * Time.deltaTime );
				if( pos1.sqrMagnitude > _sqSceneRadius ) pos1 = Random.insideUnitCircle * _sceneRadius;
				_positions[ p1 ] = pos1;

				// Display a line when close to another position.
				if( _displayLines ) {
					SetStrokeThickness( _lineThickness );
					for( int p2 = 0; p2 < p1; p2++ ) { // Only check a pair once ( p2 < p1 ).
						Vector2 pos2 = _positions[ p2 ];
						Vector2 towards2 = pos2 - pos1;
						float sqDist = towards2.sqrMagnitude;
						if( sqDist < _sqConstactDist ) { // Cheap distance check using square magnitude.
							float dist = Mathf.Sqrt( sqDist );
							towards2 /= dist;
							towards2 *= _lineThickness * 0.5f;
							float alpha = Mathf.InverseLerp( _contactDistace, _contactDistace * 0.5f, dist );
							SetStrokeColor( Color.white, alpha );
							DrawLine( pos1 + towards2, pos2 - towards2 );
							lineCount++;
						}
					}
				}

				// Draw circle.
				if( _displayCircles ) {
					SetNoStroke();
					SetFillColor( Color.red );
					DrawCircle( pos1, _lineThickness );
				}
			}

			if( _logLineCount ) Debug.Log( "lineCount: " + lineCount + "\n" );
		}


		void OnValidate()
		{
			// Create new start position and velocities.
			if( _positions == null || _objectCount != _positions.Length ) {
				_positions = new Vector2[ _objectCount ];
				_velocities = new Vector2[ _objectCount ];
				for( int i = 0; i < _objectCount; i++ ) {
					_positions[ i ] = Random.insideUnitCircle * _sceneRadius;
					_velocities[ i ] = Random.insideUnitCircle.normalized;
				}
			}

			// Compute reusable values.
			_sqSceneRadius = _sceneRadius * _sceneRadius;
			_sqConstactDist = _contactDistace * _contactDistace;

			// Constrain input values.
			if( _speed < 0 ) _speed = 0;
			if( _sceneRadius < 1 ) _sceneRadius = 1;
			if( _contactDistace < 0 ) _contactDistace = 0;
			if( _lineThickness < 0 ) _lineThickness = 0;
		}
	}
}