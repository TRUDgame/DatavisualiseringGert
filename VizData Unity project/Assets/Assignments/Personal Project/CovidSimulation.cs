/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

using UnityEngine;
using UnityEngine.UIElements;
using static Draw;

public class CovidSimulation : MonoBehaviour
{
	[SerializeField] int _humanCount = 100;
	[SerializeField] float _sceneRadius = 5;
	[SerializeField] float _contactDistace = 0.5f;
	[SerializeField] float _speed = 1.0f;
	[SerializeField] float _lineThickness = 0.05f;
	[SerializeField] float _circleThickness = 0.05f;
	[SerializeField] Color _notInfectedColor;
	[SerializeField] Color _infectedColor;
	[SerializeField] bool _displayCircles = true;
	[SerializeField] bool _displayLines = true;
	[SerializeField] bool _logLineCount = false;

	Human[] humans; 
	float _sqSceneRadius;
	float _sqConstactDist;


	void Awake()
	{
		humans = new Human[_humanCount];
		for (int i = 0; i < _humanCount; i++)
		{
			humans[i] = new Human();
			humans[i].position = Random.insideUnitCircle * _sceneRadius;
			humans[i].velocity = new Vector2(Random.Range(-2,2),Random.Range(-2,2)); 
		}
	}


	void Update()
	{
		int lineCount = 0;

		for (int h1 = 0; h1 < humans.Length; h1++)
		{

			Human human1 = humans[h1];
			// Move position.
			Vector2 pos1 = human1.position; 

			pos1 += human1.velocity * (_speed * Time.deltaTime);
			if (pos1.sqrMagnitude > _sqSceneRadius) pos1 = Random.insideUnitCircle * _sceneRadius;
			human1.position = pos1;

			// Display a line when close to another position.
			if (_displayLines)
			{
				SetStrokeThickness(_lineThickness);
				for (int h2 = 0; h2 < h1; h2++)
				{ // Only check a pair once ( p2 < p1 ).
					Human human2 = humans[h2];

					Vector2 pos2 = human2.position;
					Vector2 towards2 = pos2 - pos1;
					float sqDist = towards2.sqrMagnitude;
					if (sqDist < _sqConstactDist)
					{ // Cheap distance check using square magnitude.
						float dist = Mathf.Sqrt(sqDist);
						towards2 /= dist;
						towards2 *= _lineThickness * 0.5f;
						float alpha = Mathf.InverseLerp(_contactDistace, _contactDistace * 0.5f, dist);
						SetStrokeColor(Color.white, alpha);
						DrawLine(pos1 + towards2, pos2 - towards2);
						lineCount++;

					}
				}

				// Draw circle.
				if (_displayCircles)
				{
					
					SetNoStroke();
					SetFillColor(_notInfectedColor);
					DrawCircle(pos1, _circleThickness);

				}
				
			}

			if (_logLineCount) Debug.Log("lineCount: " + lineCount + "\n");
		}
	}

	void OnValidate()
	{
		
	// Compute reusable values.
		_sqSceneRadius = _sceneRadius * _sceneRadius;
		_sqConstactDist = _contactDistace * _contactDistace;

			
		// Constrain input values.
		if (_speed < 0) _speed = 0;
		if (_sceneRadius < 1) _sceneRadius = 1;
		if (_contactDistace < 0) _contactDistace = 0;
		if (_lineThickness < 0) _lineThickness = 0;
	}
}
