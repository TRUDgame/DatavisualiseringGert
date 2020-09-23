/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

using System.Collections.Generic;
using UnityEngine;

public class MeshBatchingMultieShapesExample : MonoBehaviour
{
	public Material material;
	public int circleCount = 16;
	public int circleResolution = 64;

	Mesh _mesh;


	void Awake()
	{
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangleIndices = new List<int>();
		List<Color> colors = new List<Color>();

		for( int c = 0; c < circleCount; c++ ) {
			Vector2 position = Random.insideUnitCircle * 5;
			float radius = Random.Range( 0.2f, 0.5f );
			Color color = Color.HSVToRGB( Random.value, 1, 1 );
			AddCircle( position, radius, color, vertices, triangleIndices, colors );
		}

		_mesh = new Mesh();
		_mesh.SetVertices( vertices );
		//_mesh.SetIndexBufferParams( triangleIndices.Count, UnityEngine.Rendering.IndexFormat.UInt32 ); // Increase max index count
		_mesh.SetIndices( triangleIndices, MeshTopology.Triangles, 0 );
		_mesh.SetColors( colors );
	}


	void Update()
	{
		Graphics.DrawMesh( _mesh, transform.localToWorldMatrix, material, gameObject.layer );
	}


	void AddCircle( Vector2 position, float radius, Color color, List<Vector3> vertices, List<int> triangleIndices, List<Color> colors )
	{
		Vector2 prevPoint = position + new Vector2( radius, 0 ); // Angle 0.
		for( int i = 1; i < circleResolution; i = i + 1 ) {
			float t = Mathf.InverseLerp( 0, circleResolution - 1, i ); // Normalized value of i.
			float angle = t * Mathf.PI * 2;
			float x = Mathf.Cos( angle ) * radius;
			float y = Mathf.Sin( angle ) * radius;
			Vector2 point = position + new Vector2( x, y );

			 

			prevPoint = point;
		}
	}
}