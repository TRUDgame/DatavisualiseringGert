/*
	Copyright © Carl Emil Carlsen 2018
	http://cec.dk
*/

using UnityEngine;
using System.Collections;

namespace CEC.Synths
{
    public class Bezier
    {
	    /// <summary>
	    /// Generate 2D curve from anchor points.
	    /// </summary>
	    ///
	    public static Vector2[] Curve2D( Anchor[] anchors, int segmentResolution )
	    {
		    Vector2[] curve = new Vector2[ CurvePointCount( anchors.Length, segmentResolution ) ];
		    int c = 0;
		    for( int a=0; a<anchors.Length-1; a++ ){
			    int nextA = a+1;
			    Vector3 a1control2 = anchors[a].position + anchors[a].control2;
			    Vector3 a2control1 = anchors[nextA].position + anchors[nextA].control1;
			    for( int r=0; r<segmentResolution; r++ ){
				    if( !( a > 0 && r == 0 ) ){
					    float t = r / ( segmentResolution - 1f );
					    float x = QuadraticInterpolation( anchors[ a ].position.x, a1control2.x, a2control1.x, anchors[ nextA ].position.x, t );
					    float y = QuadraticInterpolation( anchors[ a ].position.y, a1control2.y, a2control1.y, anchors[ nextA ].position.y, t );
					    curve[ c++ ] = new Vector2( x, y );
				    }
			    }
		    }
		    return curve;
	    }
	
	
	    /// <summary>
	    /// Generate 3D curve from anchor points.
	    /// </summary>
	    public static void Curve3D( Anchor[] anchors, int segmentResolution, ref Vector3[] linePoints )
	    {
		    int pointCount = CurvePointCount( anchors.Length, segmentResolution );
		    if( linePoints == null || linePoints.Length < pointCount ) linePoints = new Vector3[pointCount];
		    int c = 0;
		    for( int a=0; a<anchors.Length-1; a++ ){
			    for( int r=0; r<segmentResolution; r++ ){
				    if( !( a > 0 && r == 0 ) ){
					    float t = r / ( segmentResolution - 1f );
					    float x = QuadraticInterpolation( anchors[a].position.x, anchors[a].position.x+anchors[a].control2.x, anchors[a+1].position.x+anchors[a+1].control1.x, anchors[a+1].position.x, t );
					    float y = QuadraticInterpolation( anchors[a].position.y, anchors[a].position.y+anchors[a].control2.y, anchors[a+1].position.y+anchors[a+1].control1.y, anchors[a+1].position.y, t );
					    float z = QuadraticInterpolation( anchors[a].position.z, anchors[a].position.z+anchors[a].control2.z, anchors[a+1].position.z+anchors[a+1].control1.z, anchors[a+1].position.z, t );
					    linePoints[ c++ ] = new Vector3( x, y, z );
				    }
			    }
		    }
	    }
	


	    /// <summary>
	    /// Calculate number of points that will be in a curve with number of anchors with segment resolution.
	    /// </summary>
	    public static int CurvePointCount( int anchorCount, int segmentResolution )
	    {
		    return (anchorCount-1) * (segmentResolution-1) + 1;
	    }


	    /// <summary>
	    /// Displays gizmos. Call from OnDrawGizmosSelected() or OnDrawGizmosSelected() inside a MonoBehaivour.
	    /// </summary>m>
	    public static void DisplayGizmos( Anchor[] anchors, int segmentResolution, float anchorSize, Color color )
	    {
		    Vector3[] curve = null;
		    Bezier.Curve3D( anchors, segmentResolution, ref curve );
		    Gizmos.color = color;
		    for( int c=0; c<curve.Length-1; c++ ) Gizmos.DrawLine( curve[ c ], curve[ c+1 ] );
		    for( int a=0; a<anchors.Length; a++ ){
			    if( a == 0 ){
				    anchors[a].DrawGizmo( anchorSize, color, false, true );
			    } else if( a == anchors.Length-1 ){
				    anchors[a].DrawGizmo( anchorSize, color, true, false );
			    } else {
				    anchors[a].DrawGizmo( anchorSize, color );
			    }
		    }
	    }


	    public static void CurveSegment( Vector2 anchor1, Vector2 control1, Vector2 control2, Vector2 anchor2, int resolution, ref Vector2[] points )
	    {
		    if( points == null || points.Length < resolution ) points = new Vector2[resolution];

		    // make control points relative
		    control1 += anchor1;
		    control2 += anchor2;
		    for( int r=0; r<resolution; r++ ) {
			    float t = r / (float) resolution;
			    float x = QuadraticInterpolation( anchor1.x, control1.x, control2.x, anchor2.x, t );
			    float y = QuadraticInterpolation( anchor1.y, control1.y, control2.y, anchor2.y, t );
			    points[ r ] = new Vector2( x, y );
		    }
	    }
	    
	    
	    public static void CurveSegment( Vector3 anchor1, Vector3 control1, Vector3 control2, Vector3 anchor2, int resolution, ref Vector3[] points )
	    {
		    if( points == null || points.Length < resolution ) points = new Vector3[resolution];

		    // make control points relative
		    control1 += anchor1;
		    control2 += anchor2;
		    for( int r=0; r<resolution; r++ ) {
			    float t = r / (float) ( resolution - 1 );
			    float x = QuadraticInterpolation( anchor1.x, control1.x, control2.x, anchor2.x, t );
			    float y = QuadraticInterpolation( anchor1.y, control1.y, control2.y, anchor2.y, t );
			    float z = QuadraticInterpolation( anchor1.z, control1.z, control2.z, anchor2.z, t );
			    points[ r ] = new Vector3( x, y, z );
		    }
	    }
	
	    /// <summary>
	    /// Evalutes quadratic bezier at point t for points a, b, c, d.
	    ///	t varies between 0 and 1, and a and d are the curve points,
	    ///	b and c are the control points. this can be done once with the
	    ///	x coordinates and a second time with the y coordinates to get
	    ///	the location of a bezier curve at t.
	    /// </summary>
	    static float QuadraticInterpolation( float a, float b, float c, float d, float t ) {
		    float t1 = 1f - t;
		    return a*t1*t1*t1 + 3*b*t*t1*t1 + 3*c*t*t*t1 + d*t*t*t;
	    }

	    /// <summary>
	    /// Expands an array of anchors to two arrays; one with positions and one with sets of control points
	    /// for each position. All values are absolute.
	    /// </summary>
	    public static void ExpandAnchorArrays( Anchor[] anchors, out Vector3[] positions, out Vector3[] controlPoints )
	    {
		    positions = new Vector3[ anchors.Length ];
		    controlPoints = new Vector3[ anchors.Length*2 ];
		    for( int a=0; a<anchors.Length; a++ ){
			    positions[a] = anchors[a].position;
			    controlPoints[a*2] = anchors[a].position + anchors[a].control1;
			    controlPoints[a*2+1] = anchors[a].position + anchors[a].control2;
		    }
	    }


        public static void ComputeLineInterpolationData( Vector3[] linePoints, ref float[] accumulatingLengths, out float totalLength )
        {
            totalLength = 0;
            if( accumulatingLengths == null || accumulatingLengths.Length < linePoints.Length-1 ) accumulatingLengths = new float[linePoints.Length-1];
            for( int i = 0; i < linePoints.Length-1; i++ ){
                float length = ( linePoints[i+1] - linePoints[i] ).magnitude;
                totalLength += length;
                accumulatingLengths[i] = totalLength;
            }
        }


        public static Vector3 InterpolateLine( Vector3[] linePoints, float[] accumulatingLengths, float totalLength, float t )
        {
            int lastIndex = linePoints.Length-1;
            if( accumulatingLengths.Length != linePoints.Length-1 ){
                Debug.LogWarning( "accumulatingLengths.Length must equal linePoints.Length-1.\n" );
                return Vector3.zero;
            }

            if( t <= 0 ) return linePoints[0];
            if( t >= 1 ) return linePoints[lastIndex];

            float targetLength = t * totalLength;
            float prevLength = 0;
            for( int i = 0; i < lastIndex; i++ ){
                float length = accumulatingLengths[i];
                if( targetLength < length ) return Vector3.Lerp( linePoints[i], linePoints[i+1], Mathf.InverseLerp( prevLength, length, targetLength ) );
                prevLength = length;
            }

            return Vector3.zero;
        }


	    public struct Anchor
	    {
		    /// <summary>
		    /// The anchor position.
		    /// </summary>
		    public Vector3 position;

		    /// <summary>
		    /// The control1 position, relative to anchor position.
		    /// </summary>
		    public Vector3 control1;

		    /// <summary>
		    /// The control2 position, relative to anchor position.
		    /// </summary>
		    public Vector3 control2;

		    /// <summary>
		    /// Anchor at position zero with control points at zero.
		    /// </summary>
		    public static Anchor zero {
			    get { return new Bezier.Anchor( Vector3.zero, Vector3.zero, Vector3.zero ); }
		    }
		
		    /// <summary>
		    /// Create a new Anchor instance.
		    /// </summary>
		    public Anchor( Vector3 control1, Vector3 position, Vector3 control2 )
		    {
			    this.control1 = control1;
			    this.position = position;
			    this.control2 = control2;
		    }
		
		    /// <summary>
		    /// Create a new Anchor instance.
		    /// </summary>
		    public Anchor( Vector2 control1, Vector2 position, Vector2 control2 )
		    {
			    this.control1 = new Vector3( control1.x, control1.y, 0 );
			    this.position = new Vector3( position.x, position.y, 0 );
			    this.control2 = new Vector3( control2.x, control2.y, 0 );
		    }

		    /// <summary>
		    /// Display anchor gizmo.
		    /// </summary>
		    public void DrawGizmo( float anchorSize, Color color )
		    {
			    DrawGizmo( anchorSize, color, true, true );
		    }
		
		    /// <summary>
		    /// Display anchor gizmo.
		    /// </summary>
		    public void DrawGizmo( float anchorSize, Color color, bool displayControl1, bool displayControl2 )
		    {
			    Gizmos.color = color;
			    Gizmos.DrawSphere( position, anchorSize );
			    if( displayControl1 ){
				    Gizmos.DrawSphere( position+control1, anchorSize * 0.5f );
				    Gizmos.DrawLine( position, position+control1 );
			    }
			    if( displayControl2 ){
				    Gizmos.DrawSphere( position+control2, anchorSize * 0.5f );
				    Gizmos.DrawLine( position, position+control2 );
			    }
		    }
	    }
    }
}