/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk

	TODO: support holes.
*/

using System.Collections.Generic;
using UnityEngine;

public class Polygon
{
	double[] _data;
	Vector3[] _vertices;
	Vector3[] _normals;
	List<int> _indices;
	bool _isDirty;


	public void SetPointCount( int pointCount )
	{
		if( _data == null || _data.Length != pointCount * 2 ) {
			_data = new double[ pointCount * 2 ];
			_vertices = new Vector3[ pointCount ];
			_normals = new Vector3[ pointCount ];
		}
		for( int i = 0; i < pointCount; i++ ) _normals[i] = Vector3.back;
		_isDirty = true;
	}


	public void SetPoint( int index, Vector2 point )
	{
		// Reverse. Earcut expects anti-clockwise, Unity expects clockwise.
		int d = index*2;
		_data[ d ] = point.x;
		_data[ d+1 ] = point.y;
		_vertices[ index ] = point;
		_isDirty = true;
	}


	public Vector3[] GetVertices()
	{
		if( _isDirty ) Triangulate();

		return _vertices;
	}


	public List<int> GetTriangleIndices()
	{
		if( _isDirty ) Triangulate();

		return _indices;
	}


	public Vector3[] GetNormals()
	{
		if( _isDirty ) Triangulate();

		return _normals;
	}


	void Triangulate()
	{
		_indices = EarcutNet.Earcut.Tessellate( _data, new int[] { } );
		_indices.Reverse(); // TODO avoid garbage. Earcut always returns a flipped mesh.
		_isDirty = false;
	}
}