/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static Draw;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class CovidSimulation : MonoBehaviour
{
	[SerializeField] public int _humanCount;
	[SerializeField] float _sceneRadius;
	[SerializeField] float _contactDistace = 1f;
	[SerializeField] float _speed = 1.0f;
	[SerializeField] float _lineThickness = 0.05f;
	[SerializeField] float _circleThickness = 0.05f;
	[SerializeField] float _strokeThickness = 0.05f;
	public StrokeAlignment strokeAlignment = StrokeAlignment.Outside;
	[SerializeField] Color _notInfectedColor;
	[SerializeField] Color _infectedColor;
	[SerializeField] Color _recoveredColor;
	[SerializeField] Color _infectiousColor;
	[SerializeField] Color _exposedColor;
	[SerializeField] bool _displayCircles = true;
	[SerializeField] bool _displayLines = true;
	[SerializeField] bool _logLineCount = false;
	[SerializeField] bool _socialDistancing = false;
	[SerializeField] public float padding = 1.5f;
	[SerializeField] public float repulsionForce = 1.5f;

	

	public GameObject infectedText;
	public GameObject infectiousText;
	public GameObject exposedText;
	public GameObject recoveredText;
	public GameObject timeText;

	public int exposedHumans = 0;
	public int infectedHumans = 0;
	public int infectiousHumans = 0;
	public int recoveredHumans = 0;
	public int deadHumans = 0; 


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
			humans[i].velocity = new Vector2(Random.Range(-3f, 2f), Random.Range(-3f, 2f));
			humans[i].id = i; 
		
		}
		Human patientZero = humans[Random.Range(0, _humanCount)];
		//patientZero.isInfected = true;
		StartCoroutine("Infected", patientZero); 
	
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

					float _personalSpaceDist = _circleThickness * padding; 

					if ( _socialDistancing == true && sqDist < _personalSpaceDist)
					{
						float distance = Mathf.Sqrt(sqDist);
						float forceMult = distance / _personalSpaceDist; // normalize.
						forceMult = 1 - forceMult; // Invert.
						towards2 /= distance; // Normalize vector.
						towards2 *= forceMult * repulsionForce;

						pos1 -= towards2 * Time.deltaTime;
						pos2 += towards2 * Time.deltaTime;

						human2.position = pos2; // Update!
					}
					if (sqDist < _sqConstactDist)
					{ // Cheap distance check using square magnitude.
						/*float dist = Mathf.Sqrt(sqDist);
						towards2 /= dist; //towards2 peger fra pos1 til pos2, og har samme længde som dist. Vi normalisere derfor vektoren, så den bliver 1 lang. 
						towards2 *= _lineThickness * 0.5f; 
						float alpha = Mathf.InverseLerp(_contactDistace, _contactDistace * 0.5f, dist); */
						SetStrokeColor(Color.white);
						DrawLine(pos1, pos2);
						lineCount++;
					
						//SetFillColor(_notInfectedColor);
						if (human1.isInfectious && !human1.isRecovered && !human2.isExposed)
						{
							
							StartCoroutine("Infected", human2);
						}
						if (human2.isInfectious && !human2.isRecovered && !human1.isExposed)
						{
							//human1.isInfected = true;
							StartCoroutine("Infected", human1);
						}


					}
					}


					// Draw circle.
					if (_displayCircles)
					{

					SetStrokeColor(GetStrokeColor(human1));

					SetStrokeThickness(_strokeThickness);
					SetStrokeAlignement(strokeAlignment); 
					SetFillColor(GetInfectionColor(human1));
					DrawCircle(pos1, _circleThickness);
					
					}
					

				}

				if (_logLineCount) Debug.Log("lineCount: " + lineCount + "\n");


			//GameObject textObject = Instantiate(infectedText); //tager originale textprefab, og laver en kopi
			//infectedText.SetActive(true);
			//textObject.transform.SetParent(gameObject.transform);
			infectedText.GetComponent<TextMesh>().text = infectedHumans.ToString();
			exposedText.GetComponent<TextMesh>().text = exposedHumans.ToString();
			recoveredText.GetComponent<TextMesh>().text = recoveredHumans.ToString();
			infectiousText.GetComponent<TextMesh>().text = infectiousHumans.ToString();
			timeText.GetComponent<TextMesh>().text = Time.timeSinceLevelLoad.ToString();



		}

			
		}


	Color GetInfectionColor(Human human)
	{
		if (human.isRecovered)
		{
			return _recoveredColor;
		}
		else if (human.isInfected)
		{
			return _infectedColor;
		}
		else
		{
			return _notInfectedColor;
		}
	}
	Color GetStrokeColor(Human human)
	{
		if (human.isRecovered)
		{
			return _recoveredColor;
		}
		else if (human.isInfectious)
		{
			return _infectiousColor;
		}
		
		else if (human.isInfected)
		{
			return _infectedColor;
		}
		else if (human.isExposed)
		{
			return _exposedColor;
		}
		else
		{
			return _notInfectedColor;
		}
	}
	IEnumerator Infected(Human human)
	{
		Debug.Log(human.id);
		
		human.isExposed = true;

		exposedHumans++; 

		yield return new WaitForSeconds(Random.Range(1f,14f));
 
		human.isInfected = true;

		infectedHumans++;
		exposedHumans--;

		//float randValue = Random.value; 
		//if(randValue < 1f)
		//{
			//deadHumans++; 
		//}

		yield return new WaitForSeconds(Random.Range(2f, 4f));
		human.isInfectious = true;

		infectiousHumans++; 

		yield return new WaitForSeconds(Random.Range(8f, 10f));

		human.isRecovered = true;
		human.isInfectious = false;

		infectiousHumans--;
		recoveredHumans++; 
				
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
	void OnClickSocial()
	{
		_socialDistancing = true;

		Update(); 
	}

	void OnClickRevert()
	{
		
		SceneManager.LoadScene(0);
		 
	}

}

