/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

using UnityEngine;

public class MeshQuadExample : MonoBehaviour
{
	public Material material;
	
	Mesh _mesh;


	void Awake()
	{
		Vector3[] vertices = new Vector3[ 4 ];
		vertices[ 0 ] = new Vector3( 0, 1 );
		vertices[ 1 ] = new Vector3( 1, 1 );
		vertices[ 2 ] = new Vector3( 1, 0 );
		vertices[ 3 ] = new Vector3( 0, 0 );

		int[] quadIndices = new int[] { 0, 1, 2, 3 };

		Vector3[] normals = new Vector3[ 4 ];
		for( int i = 0; i < normals.Length; i++ ) normals[ i ] = Vector3.back;

		Vector2[] uvs = new Vector2[ 4 ];
		uvs[ 0 ] = new Vector2( 0, 1 );
		uvs[ 1 ] = new Vector2( 1, 1 );
		uvs[ 2 ] = new Vector2( 1, 0 );
		uvs[ 3 ] = new Vector2( 0, 0 );

		_mesh = new Mesh();
		_mesh.SetVertices( vertices );
		_mesh.SetIndices( quadIndices, MeshTopology.Quads, 0 );
		_mesh.SetNormals( normals );
		_mesh.SetUVs( 0, uvs );
	}


	void Update()
	{
		Graphics.DrawMesh( _mesh, transform.localToWorldMatrix, material, gameObject.layer );
	}
}
