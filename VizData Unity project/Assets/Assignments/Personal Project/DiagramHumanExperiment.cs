using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiagramHumanExperiment : MonoBehaviour
{
	public Material material;
	Transform[] outlinePointTransforms = null;
	int pointcount; 

	Mesh _mesh;
	PolygonOld _polygon;


	void Awake()
	{
		_mesh = new Mesh();
		_polygon = new PolygonOld();
	}


	void Update()
	{
		 

		// Add outline to the polygon object. (Expects clockwise order)
		_polygon.SetPointCount(outlinePointTransforms.Length );

		for( int p1 = 0; p1 < outlinePointTransforms.Length; p1++ ) {


			Transform point1 = outlinePointTransforms[p1];
			Vector3 pos1 = point1.position; 

			pos1 = new Vector3(Time.timeSinceLevelLoad, GetComponent<CovidSimulation>().infectedHumans,0);

			_polygon.SetPoint( p1, pos1 );

		}

		// Get mesh data from polygon and forward it to the mesh.
		_mesh.SetVertices( _polygon.GetVertices() );
		_mesh.SetIndices( _polygon.GetTriangleIndices(), MeshTopology.Triangles, 0 );
		_mesh.SetNormals( _polygon.GetNormals() );

		// Draw triangulated polygon mesh.
		Graphics.DrawMesh( _mesh, transform.localToWorldMatrix, material, gameObject.layer );
	}

	void OnDrawGizmos()
	{
		if( outlinePointTransforms == null ) return;

		for (int p1 = 0; p1 < outlinePointTransforms.Length; p1++)
		{


			Transform point1 = outlinePointTransforms[p1];
			Vector3 pos1 = point1.position;

			pos1 = new Vector3(Time.timeSinceLevelLoad, GetComponent<CovidSimulation>().infectedHumans, 0);

			_polygon.SetPoint(p1, pos1);

		
		Gizmos.DrawSphere( pos1, 0.08f );

			#if UNITY_EDITOR // Only include this code in the editor. UnityEditor class does not exist in builds.
			UnityEditor.Handles.Label( pos1, p1.ToString() );
			#endif
		}
	}
}
