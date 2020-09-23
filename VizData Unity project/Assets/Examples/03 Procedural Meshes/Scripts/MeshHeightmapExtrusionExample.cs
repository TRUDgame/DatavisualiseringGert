/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshHeightmapExtrusionExample : MonoBehaviour
{
	public Material material;
	public Texture2D heightmapTexture;
	public Vector2Int resolution = new Vector2Int( 32, 32 );
	public Vector2 size = new Vector2( 10, 10 );
	public float heightMax = 1;

	Mesh _mesh;


	void Awake()
	{
		_mesh = new Mesh();

		int vertexCount = resolution.x * resolution.y;
		int quadCount = (resolution.x-1) * (resolution.y-1);
		int quadIndexCount = quadCount * 4;
		Vector3[] vertices = new Vector3[ vertexCount ];
		int[] quadIndicies = new int[ quadIndexCount ];

		int v = 0; // Vertex index.
		int i = 0; // quad index index.
		for( int nz = 0; nz < resolution.y; nz++ ) {
			float tz = Mathf.InverseLerp( 0, resolution.y-1, nz ); // Normalized position z (0.0 to 1.0)
			float z = Mathf.Lerp( 0, size.y, tz ); // Scale to size.y

			for( int nx = 0; nx < resolution.x; nx++ ) {
				float tx = Mathf.InverseLerp( 0, resolution.x - 1, nx ); // Normalized position z (0.0 to 1.0)
				float x = Mathf.Lerp( 0, size.x, tx ); // Scale to size.x
				//Debug.Log( nx + " -> " + tx + " -> " + x );

				Color heightMapColor = heightmapTexture.GetPixelBilinear( tx, tz ); // Read interpolated (between pixels) color from texture

				float y = heightMapColor.r * heightMax;// Random.value;

				Vector3 position = new Vector3( x, y, z );

				vertices[ v ] = position;

				// Add quad if we are inside right and depth edge of grid.
				if( nx < resolution.x-1 && nz < resolution.y-1 ) {
					quadIndicies[ i+0 ] = v;						// Current vertex
					quadIndicies[ i+1 ] = v + resolution.x;			// Vertex in next row
					quadIndicies[ i+2 ] = v + resolution.x + 1;		// Vertex in next row plus 1. 
					quadIndicies[ i+3 ] = v + 1;					// Next vertex
					i += 4;
				}

				v++; // Next vertex index (increase index by one)
			}
		}

		_mesh.SetVertices( vertices );
		_mesh.SetIndexBufferParams( quadIndicies.Length, UnityEngine.Rendering.IndexFormat.UInt32 ); // Enable large mesh support
		_mesh.SetIndices( quadIndicies, MeshTopology.Quads, 0 );
		_mesh.RecalculateNormals(); // Let Unity compute the normals of the surface (slow, but ok in Awake).
	}


	void Update()
	{
		Graphics.DrawMesh( _mesh, transform.localToWorldMatrix, material, gameObject.layer );
	}
}