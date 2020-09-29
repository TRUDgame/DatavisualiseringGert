/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk

	TODO
		- Polyline
			- Antialias caps.
			- Closed option
			- SetSplineCurve
		- Polygon
			- Stroke (using internal Polyline with stroke alignment)
		- Ring
		- Triangle (equilateral triangle)
		- Ellipse
		- SetAntialiasing (should be optional. for now its always on)
*/

using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class Draw
{
	static Draw _self;

	Material _circleMaterial;
	Material _pieMaterial;
	Material _arcMaterial;
	Material _lineMaterial;
	Material _rectMaterial;
	Material _polygonMaterial;
	Material _polylineMaterial;

	Mesh _quadMesh;

	MaterialPropertyBlock _circleProps = new MaterialPropertyBlock();
	MaterialPropertyBlock _pieProps = new MaterialPropertyBlock();
	MaterialPropertyBlock _arcProps = new MaterialPropertyBlock();
	MaterialPropertyBlock _lineProps = new MaterialPropertyBlock();
	MaterialPropertyBlock _rectProps = new MaterialPropertyBlock();
	MaterialPropertyBlock _polygonProps = new MaterialPropertyBlock();
	MaterialPropertyBlock _polylineProps = new MaterialPropertyBlock();

	Matrix4x4 _activeMatrix = Matrix4x4.identity;
	Stack<Matrix4x4> _matrixStack;

	// Global states.
	bool _fillEnabled = true;
	bool _strokeEnabled = true;
	Color _fillColor = Color.white;
	Color _strokeColor = Color.black;
	float _strokeThickness = 0.1f; // Always stored in meters
	float _halfStrokeThickness = 0.05f;
	StrokeAlignment _strokeAlignment = StrokeAlignment.Inside;
	CapAlignment _capAlignment = CapAlignment.Inside;

	Vector2 _circlePivotPosition = Vector2.zero;
	Vector2 _piePivotPosition = Vector2.zero;
	Vector2 _arcPivotPosition = Vector2.zero;
	Vector2 _rectPivotPosition = Vector2.zero;

	const MeshUpdateFlags meshFlags = MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices;
	const string logPrepend = "<b>[" + nameof( Draw ) + "]</b> ";

	public const float Pi = Mathf.PI;
	public const float HalfPi = Pi * 0.5f;
	public const float Tau = Pi * 2;

	public enum StrokeAlignment { Inside, Edge, Outside }
	public enum Pivot { Center, TopLeft, Top, TopRight, Right, BottomRight, Bottom, BottomLeft, Left }
	public enum Cap { None, Rounded }
	public enum CapAlignment { Inside, Outside }


	static class ShaderIDs
	{
		public static readonly int fillColor = Shader.PropertyToID( "_FillColor" );
		public static readonly int strokeColor = Shader.PropertyToID( "_StrokeColor" );
		public static readonly int strokeMin = Shader.PropertyToID( "_StrokeMin" );
		public static readonly int strokeThickness = Shader.PropertyToID( "_StrokeThickness" );
		public static readonly int halfStrokeThickness = Shader.PropertyToID( "_HalfStrokeThickness" );
		public static readonly int innerRadiusRel = Shader.PropertyToID( "_InnerRadiusRel" );
		public static readonly int fillExtents = Shader.PropertyToID( "_FillExtents" );
		public static readonly int roundedness = Shader.PropertyToID( "_Roundedness" );
		public static readonly int angleExtents = Shader.PropertyToID( "_AngleExtents" );
		public static readonly int roundedCapFlags = Shader.PropertyToID( "_RoundedCapFlags" );
	}


	#region Setup


	Draw()
	{
		_circleMaterial = CreateInstancedSDFMaterial( "Circle" );
		_lineMaterial = CreateInstancedSDFMaterial( "Line" );
		_rectMaterial = CreateInstancedSDFMaterial( "Rect" );
		_pieMaterial = CreateInstancedSDFMaterial( "Pie" );
		_arcMaterial = CreateInstancedSDFMaterial( "Arc" );
		_polygonMaterial = CreateInstancedSDFMaterial( "Polygon" );
		_polylineMaterial = CreateInstancedSDFMaterial( "Polyline" );

		_quadMesh = CreateQuadMesh();

		_matrixStack = new Stack<Matrix4x4>( 10 );
	}


	static void EnsureSingletonInstance()
	{
		if( _self == null ) _self = new Draw();
	}


	#endregion


	#region ModeModifiers
	

	public static void SetFillColor( Color color )
	{
		EnsureSingletonInstance();

		_self._fillEnabled = color.a > 0;
		_self._fillColor = color;
	}

	public static void SetFillColor( Color color, float alphaOverride )
	{
		color.a = alphaOverride;
		SetFillColor( color );
	}


	public static void SetNoFill()
	{
		EnsureSingletonInstance();

		_self._fillEnabled = false;
		_self._fillColor = Color.clear;
	}


	public static void SetStrokeColor( Color color )
	{
		EnsureSingletonInstance();

		_self._strokeEnabled = color.a > 0 && _self._strokeThickness > 0;
		_self._strokeColor = color;
	}

	public static void SetStrokeColor( Color color, float alphaOverride )
	{
		color.a = alphaOverride;
		SetStrokeColor( color );
	}


	public static void SetNoStroke()
	{
		EnsureSingletonInstance();

		_self._strokeEnabled = false;
		_self._strokeColor = Color.clear;
	}


	public static void SetStrokeThickness( float thickness )
	{
		EnsureSingletonInstance();

		_self._strokeThickness = thickness;
		_self._halfStrokeThickness = thickness * 0.5f;
		_self._strokeEnabled = _self._strokeColor.a > 0 && _self._strokeThickness > 0;
	}


	public static void SetStrokeAlignement( StrokeAlignment alignment )
	{
		EnsureSingletonInstance();

		_self._strokeAlignment = alignment;
	}


	public static void SetCapAlignement( CapAlignment alignment )
	{
		EnsureSingletonInstance();

		_self._capAlignment = alignment;
	}


	/// <summary>
	/// Set the point from which Circle will be drawn. Default is Pivot.Center.
	/// </summary>
	public static void SetCirclePivot( Pivot pivot )
	{
		EnsureSingletonInstance();

		_self._circlePivotPosition = GetPivotPosition( pivot );
	}


	/// <summary>
	/// Set the point from which Circle will be drawn in normalized coordinates. Default is (0,0).
	/// Lower-Left is (-1,-1). Upper-Right is (1,1).
	/// </summary>
	public static void SetCirclePivot( Vector2 pivotOffset )
	{
		EnsureSingletonInstance();

		_self._circlePivotPosition = pivotOffset;
	}


	/// <summary>
	/// Set the point from which Arc will be drawn. Default is Pivot.Center.
	/// </summary>
	public static void SetArcPivot( Pivot pivot )
	{
		EnsureSingletonInstance();

		_self._arcPivotPosition = GetPivotPosition( pivot );
	}


	/// <summary>
	/// Set the point from which Arc will be drawn in normalized coordinates. Default is (0,0).
	/// Lower-Left is (-1,-1). Upper-Right is (1,1).
	/// </summary>
	public static void SetArcPivot( Vector2 pivotOffset )
	{
		EnsureSingletonInstance();

		_self._arcPivotPosition = pivotOffset;
	}


	// <summary>
	/// Set the point from which Pie will be drawn. Default is Pivot.Center.
	/// </summary>
	public static void SetPiePivot( Pivot pivot )
	{
		EnsureSingletonInstance();

		_self._piePivotPosition = GetPivotPosition( pivot );
	}


	/// <summary>
	/// Set the point from which Pie will be drawn in normalized coordinates. Default is (0,0).
	/// Lower-Left is (-1,-1). Upper-Right is (1,1).
	/// </summary>
	public static void SetPiePivot( Vector2 pivotOffset )
	{
		EnsureSingletonInstance();

		_self._piePivotPosition = pivotOffset;
	}


	/// <summary>
	/// Set the point from which Rect will be drawn. Default is Pivot.Center.
	/// </summary>
	public static void SetRectPivot( Pivot pivot )
	{
		EnsureSingletonInstance();

		_self._rectPivotPosition = GetPivotPosition( pivot );
	}


	/// <summary>
	/// Set the point from which Rect will be drawn in normalized coordinates. Default is (0,0).
	/// Lower-Left is (-1,-1). Upper-Right is (1,1).
	/// </summary>
	public static void SetRectPivot( Vector2 pivotOffset )
	{
		EnsureSingletonInstance();

		_self._rectPivotPosition = pivotOffset;
	}



	#endregion // ModeModifiers


	#region TransformModifiers


	public static void PushCanvas()
	{
		EnsureSingletonInstance();

		_self._matrixStack.Push( _self._activeMatrix );
	}


	public static void MultCanvas( Matrix4x4 matrix )
	{
		EnsureSingletonInstance();

		_self._activeMatrix *= matrix;
	}


	public static void PopCanvas()
	{
		EnsureSingletonInstance();

		if( _self._matrixStack.Count == 0 ) {
			Debug.LogWarning( logPrepend + "Canvas stack is empty. The number of PopMatrix calls have exceeded the number of PushMatrix calls.\n" );
			return;
		}

		_self._activeMatrix = _self._matrixStack.Pop();
	}


	public static void TranslateCanvas( float x, float y, float z = 0 )
	{
		EnsureSingletonInstance();

		_self._activeMatrix *= Matrix4x4.Translate( new Vector3( x, y, z ) );
	}


	public static void TranslateCanvas( Vector3 translation )
	{
		EnsureSingletonInstance();

		_self._activeMatrix *= Matrix4x4.Translate( translation );
	}


	public static void RotateCanvas( float angleZ )
	{
		EnsureSingletonInstance();

		_self._activeMatrix *= Matrix4x4.Rotate( Quaternion.AngleAxis( angleZ * Mathf.Deg2Rad, Vector3.back ) );
	}


	public static void RotateCanvas( Quaternion rotation )
	{
		EnsureSingletonInstance();

		_self._activeMatrix *= Matrix4x4.Rotate( rotation );
	}


	public static void ScaleCanvas( float scale )
	{
		EnsureSingletonInstance();

		_self._activeMatrix *= Matrix4x4.Scale( Vector3.one * scale );
	}


	public static void ScaleCanvas( Vector3 scale )
	{
		EnsureSingletonInstance();

		_self._activeMatrix *= Matrix4x4.Scale( scale );
	}


	public static void TranslateRotateScaleCanvas( Vector3 translation, Quaternion rotation, Vector3 scale )
	{
		EnsureSingletonInstance();

		_self._activeMatrix *= Matrix4x4.TRS( translation, rotation, scale );
	}


	public static Matrix4x4 GetCanvasMatrix()
	{
		return _self._activeMatrix;
	}

	#endregion // TransformModifiers


	#region Shapes

	public static void DrawCircle( float x, float y, float diameter )
	{
		EnsureSingletonInstance();

		if( !_self._strokeEnabled && !_self._fillEnabled ) return;

		float radius = diameter * 0.5f;
		float innerRadius, outerRadius;
		GetInnerOuterRadius( radius, out innerRadius, out outerRadius );

		Vector3 offset = new Vector3( x, y );
		offset.x -= _self._circlePivotPosition.x;
		offset.y -= _self._circlePivotPosition.y;
		Matrix4x4 matrix = _self._activeMatrix; // TODO: Optimize matrix operations
		matrix *= Matrix4x4.Translate( offset );
		matrix *= Matrix4x4.Scale( Vector3.one * outerRadius );
		
		if( _self._fillEnabled ) _self._circleProps.SetColor( ShaderIDs.fillColor, _self._fillColor );
		_self._circleProps.SetColor( ShaderIDs.strokeColor, _self._strokeEnabled ? _self._strokeColor : new Color( _self._fillColor.r, _self._fillColor.g, _self._fillColor.b, 0 ) );
		_self._circleProps.SetFloat( ShaderIDs.innerRadiusRel, innerRadius / outerRadius );

		Graphics.DrawMesh( _self._quadMesh, matrix, _self._circleMaterial, 0, null, 0, _self._circleProps, false, false, false );
	}

	public static void DrawCircle( Vector3 position, float diameter )
	{
		DrawCircle( position.x, position.y, diameter );
	}


	public static void DrawPie( float x, float y, float diameter, float angleBegin, float angleEnd, float rotation = 0 )
	{
		EnsureSingletonInstance();

		if( !_self._strokeEnabled && !_self._fillEnabled ) return;
		if( diameter < 0 ) return;

		while( angleBegin > angleEnd ) angleEnd += 360;
		float angleExtents = ( angleEnd - angleBegin ) * 0.5f;
		if( angleExtents >= 180 ) {
			DrawCircle( x, y, diameter );
			return;
		}

		float fillExtents = diameter * 0.5f;
		float strokeThickness = _self._strokeThickness;
		float strokeOffsetMin = GetStokeOffsetMin();
		float meshExtents = fillExtents + strokeOffsetMin + strokeThickness;

		Vector3 offset = new Vector2( x, y );
		offset.x -= _self._piePivotPosition.x;
		offset.y -= _self._piePivotPosition.y;
		float angleOffset = angleBegin + angleExtents;
		Matrix4x4 matrix = _self._activeMatrix; // TODO: Optimize matrix operations
		matrix *= Matrix4x4.TRS(
			offset,
			Quaternion.AngleAxis( angleOffset + rotation, Vector3.back ),
			Vector3.one * meshExtents
		);

		if( _self._fillEnabled ) _self._pieProps.SetColor( ShaderIDs.fillColor, _self._fillColor );
		_self._pieProps.SetColor( ShaderIDs.strokeColor, _self._strokeEnabled ? _self._strokeColor : new Color( _self._fillColor.r, _self._fillColor.g, _self._fillColor.b, 0 ) );
		_self._pieProps.SetFloat( ShaderIDs.fillExtents, fillExtents );
		_self._pieProps.SetFloat( ShaderIDs.angleExtents, angleExtents * Mathf.Deg2Rad );
		_self._pieProps.SetFloat( ShaderIDs.strokeMin, strokeOffsetMin );
		_self._pieProps.SetFloat( ShaderIDs.strokeThickness, strokeThickness );

		Graphics.DrawMesh( _self._quadMesh, matrix, _self._pieMaterial, 0, null, 0, _self._pieProps, false, false, false );
	}

	public static void DrawPie( Vector2 position, float diameter, float angleBegin, float angleEnd, float rotation = 0 )
	{
		DrawPie( position.x, position.y, diameter, angleBegin, angleEnd, rotation );
	}


	public static void DrawArc( float x, float y, float innerDiameter, float outerDiameter, float angleBegin, float angleEnd, float rotation = 0 )
	{
		EnsureSingletonInstance();

		if( !_self._strokeEnabled && !_self._fillEnabled ) return;
		if( outerDiameter < 0 ) return;

		while( angleBegin > angleEnd ) angleEnd += 360;
		float angleExtents = ( angleEnd - angleBegin ) * 0.5f;
		if( angleExtents >= 180 ) {
			// TODO: Ring.
			//return;
		}
		//Debug.Log( "angleBegin: " + angleBegin + ", angleEnd: " + angleEnd + ", extents: " + angleExtents );

		float innerFillExtents = innerDiameter * 0.5f;
		float outerFillExtents = outerDiameter * 0.5f;
		float fillThicknessExtents = ( outerFillExtents - innerFillExtents ) * 0.5f;
		float centerFillExtents = innerFillExtents + fillThicknessExtents;
		float strokeThickness = _self._strokeThickness;
		float strokeOffsetMin = GetStokeOffsetMin();
		float meshExtents = outerFillExtents + strokeOffsetMin + strokeThickness;

		Vector3 offset = new Vector2( x, y );
		offset.x -= _self._piePivotPosition.x;
		offset.y -= _self._piePivotPosition.y;
		float angleOffset = angleBegin + angleExtents;
		Matrix4x4 matrix = _self._activeMatrix; // TODO: Optimize matrix operations
		matrix *= Matrix4x4.TRS(
			offset,
			Quaternion.AngleAxis( 180 + angleOffset + rotation, Vector3.back ),
			Vector3.one * meshExtents
		);

		if( _self._fillEnabled ) _self._arcProps.SetColor( ShaderIDs.fillColor, _self._fillColor );
		_self._arcProps.SetColor( ShaderIDs.strokeColor, _self._strokeEnabled ? _self._strokeColor : new Color( _self._fillColor.r, _self._fillColor.g, _self._fillColor.b, 0 ) );
		_self._arcProps.SetVector( ShaderIDs.fillExtents, new Vector2( centerFillExtents, fillThicknessExtents ) );
		_self._arcProps.SetFloat( ShaderIDs.angleExtents, ( 180 - angleExtents ) * Mathf.Deg2Rad );
		_self._arcProps.SetFloat( ShaderIDs.strokeMin, strokeOffsetMin );
		_self._arcProps.SetFloat( ShaderIDs.strokeThickness, strokeThickness );

		Graphics.DrawMesh( _self._quadMesh, matrix, _self._arcMaterial, 0, null, 0, _self._arcProps, false, false, false );
	}

	public static void DrawArc( Vector2 position, float innerDiameter, float outerDiameter, float angleBegin, float angleEnd )
	{
		DrawArc( position.x, position.y, innerDiameter, outerDiameter, angleBegin, angleEnd );
	}



	public static void DrawLine( float ax, float ay, float bx, float by, Cap beginCap = Cap.Rounded, Cap endCap = Cap.Rounded, float rotation = 0 )
	{
		EnsureSingletonInstance();

		if( !_self._strokeEnabled ) return;

		Vector2 towardsB = new Vector2( bx-ax, by-ay );
		float length = towardsB.magnitude;
		float xCenter = length * 0.5f;
		float lineThickness = _self._strokeThickness;
		if( _self._capAlignment == CapAlignment.Outside ) {
			if( beginCap == Cap.Rounded && endCap == Cap.Rounded ) {
				length += _self._strokeThickness;
			} else if( beginCap == Cap.Rounded ) {
				length += _self._strokeThickness * 0.5f;
				xCenter -= _self._strokeThickness * 0.25f;
			} else if( endCap == Cap.Rounded ) {
				length += _self._strokeThickness * 0.5f;
				xCenter += _self._strokeThickness * 0.25f;
			}
		}
		if( lineThickness > length ) lineThickness = length;

		Matrix4x4 matrix = _self._activeMatrix; // TODO: Optimize matrix operations
		matrix *= Matrix4x4.Translate( new Vector2( ax, ay ) );
		matrix *= Matrix4x4.Rotate( Quaternion.AngleAxis( Mathf.Atan2( towardsB.y, towardsB.x ) * Mathf.Rad2Deg, Vector3.forward ) );
		matrix *= Matrix4x4.Translate( new Vector3( xCenter, 0, 0 ) );
		if( rotation != 0 ) matrix *= Matrix4x4.Rotate( Quaternion.AngleAxis( rotation, Vector3.back ) );
		matrix *= Matrix4x4.Scale( new Vector3( length * 0.5f, lineThickness * 0.5f, 1 ) );

		_self._lineProps.SetColor( ShaderIDs.strokeColor, _self._strokeColor );
		_self._lineProps.SetVector( ShaderIDs.fillExtents, new Vector2( length * 0.5f, lineThickness * 0.5f ) );
		_self._lineProps.SetVector( ShaderIDs.roundedCapFlags, new Vector2( beginCap == Cap.Rounded ? 1 : 0, endCap == Cap.Rounded ? 1 : 0 ) );

		Graphics.DrawMesh( _self._quadMesh, matrix, _self._lineMaterial, 0, null, 0, _self._lineProps, false, false, false );
	}

	public static void DrawLine( Vector2 positionA, Vector2 positionB, Cap beginCap, Cap endCap )
	{
		DrawLine( positionA.x, positionA.y, positionB.x, positionB.y, beginCap, endCap );
	}

	public static void DrawLine( Vector2 positionA, Vector2 positionB, Cap caps )
	{
		DrawLine( positionA.x, positionA.y, positionB.x, positionB.y, caps, caps );
	}

	public static void DrawLine( Vector2 positionA, Vector2 positionB )
	{
		DrawLine( positionA.x, positionA.y, positionB.x, positionB.y, Cap.Rounded, Cap.Rounded );
	}


	public static void DrawRect( float x, float y, float width, float height, float lowerLeftRoundness, float upperLeftRoundness, float upperRightRoundness, float lowerRightRoundness, float rotation = 0 )
	{
		EnsureSingletonInstance();

		if( !_self._strokeEnabled && !_self._fillEnabled ) return;

		float extentsX = width * 0.5f;
		float extentsY = height * 0.5f;
		float strokeOffsetMin = GetStokeOffsetMin();
		float strokeThickness = _self._strokeThickness;
		float strokeOffsetMax = strokeOffsetMin + strokeThickness;
		float outerExtentsX = extentsX + strokeOffsetMax;
		float outerExtentsY = extentsY + strokeOffsetMax;

		Vector3 offset = new Vector2( x, y );
		offset.x -= _self._rectPivotPosition.x;
		offset.y -= _self._rectPivotPosition.y;
		Matrix4x4 matrix = _self._activeMatrix; // TODO: Optimize matrix operations
		matrix *= Matrix4x4.TRS(
			offset,
			Quaternion.AngleAxis( rotation, Vector3.back ),
			new Vector3( outerExtentsX, outerExtentsY )
		);

		if( _self._fillEnabled ) _self._rectProps.SetColor( ShaderIDs.fillColor, _self._fillColor );
		_self._rectProps.SetColor( ShaderIDs.strokeColor, _self._strokeEnabled ? _self._strokeColor : new Color( _self._fillColor.r, _self._fillColor.g, _self._fillColor.b, 0 ) );
		_self._rectProps.SetVector( ShaderIDs.fillExtents, new Vector2( width * 0.5f, height * 0.5f ) );
		_self._rectProps.SetFloat( ShaderIDs.strokeThickness, strokeThickness );
		_self._rectProps.SetFloat( ShaderIDs.strokeMin, strokeOffsetMin );

		bool isRounded = upperLeftRoundness > 0 || upperRightRoundness > 0 || lowerRightRoundness > 0 || lowerLeftRoundness > 0;
		if( isRounded ) {
			if( upperLeftRoundness < 0 ) upperLeftRoundness = 0;
			else if( upperLeftRoundness > 1 ) upperLeftRoundness = 1;
			if( upperRightRoundness < 0 ) upperRightRoundness = 0;
			else if( upperRightRoundness > 1 ) upperRightRoundness = 1;
			if( lowerRightRoundness < 0 ) lowerRightRoundness = 0;
			else if( lowerRightRoundness > 1 ) lowerRightRoundness = 1;
			if( lowerLeftRoundness < 0 ) lowerLeftRoundness = 0;
			else if( lowerLeftRoundness > 1 ) lowerLeftRoundness = 1;
			float innerExtentsMin = ( extentsX < extentsY ? extentsX : extentsY );
			float upperLeftRadius = innerExtentsMin * upperLeftRoundness;
			float upperRightRadius = innerExtentsMin * upperRightRoundness;
			float lowerRightRadius = innerExtentsMin * lowerRightRoundness;
			float lowerLeftRadius = innerExtentsMin * lowerLeftRoundness;
			_self._rectProps.SetVector( ShaderIDs.roundedness, new Vector4( lowerLeftRadius, upperLeftRadius, upperRightRadius, lowerRightRadius ) );
		}

		Graphics.DrawMesh( _self._quadMesh, matrix, _self._rectMaterial, 0, null, 0, _self._rectProps, false, false, false );
	}

	public static void DrawRect( float x, float y, float width, float height, float roundness, float rotation = 0 )
	{
		DrawRect( x, y, width, height, roundness, roundness, roundness, roundness, rotation );
	}

	public static void DrawRect( Vector2 position, float width, float height, float lowerLeftRoundness, float upperLeftRoundness, float upperRightRoundness, float lowerRightRoundness, float rotation = 0 )
	{
		DrawRect( position.x, position.y, width, height, upperLeftRoundness, upperRightRoundness, lowerRightRoundness, lowerLeftRoundness, rotation );
	}

	public static void DrawRect( Vector2 position, float width, float height, float roundness = 0, float rotation = 0 )
	{
		DrawRect( position.x, position.y, width, height, roundness, roundness, roundness, roundness, rotation );
	}


	public static void DrawPolygon( Polygon polygon, float x, float y, float rotation = 0 )
	{
		EnsureSingletonInstance();

		if( !_self._strokeEnabled && !_self._fillEnabled ) return;

		Matrix4x4 matrix = _self._activeMatrix; // TODO: Optimize matrix operations
		matrix *= Matrix4x4.Translate( new Vector2( x, y ) );
		if( rotation != 0 ) matrix *= Matrix4x4.Rotate( Quaternion.AngleAxis( rotation, Vector3.back) );

		_self._polygonProps.SetColor( ShaderIDs.fillColor, _self._fillColor );
		_self._polygonProps.SetColor( ShaderIDs.strokeColor, _self._strokeEnabled ? _self._strokeColor : new Color( _self._fillColor.r, _self._fillColor.g, _self._fillColor.b, 0 ) );

		Mesh mesh;
		polygon.GetRenderObjects( _self._strokeThickness, _self._strokeAlignment, out mesh );
		Graphics.DrawMesh( mesh, matrix, _self._polygonMaterial, 0, null, 0, _self._polygonProps, false, false, false );
	}


	public static void DrawPolyline( Polyline polyline, float x, float y, float rotation = 0 )
	{
		EnsureSingletonInstance();

		if( !_self._strokeEnabled ) return;

		Matrix4x4 matrix = _self._activeMatrix; // TODO: Optimize matrix operations
		matrix *= Matrix4x4.Translate( new Vector2( x, y ) );
		if( rotation != 0 ) matrix *= Matrix4x4.Rotate( Quaternion.AngleAxis( rotation, Vector3.back ) );

		_self._polylineProps.SetColor( ShaderIDs.strokeColor, _self._strokeColor );
		_self._polylineProps.SetFloat( ShaderIDs.halfStrokeThickness, _self._halfStrokeThickness );

		Mesh mesh;
		polyline.GetRenderObjects( _self._strokeThickness, out mesh );
		Graphics.DrawMesh( mesh, matrix, _self._polylineMaterial, 0, null, 0, _self._polylineProps, false, false, false );
	}


	#endregion // Shapes


	static Material CreateInstancedSDFMaterial( string shapeName )
	{
		Material mat = new Material( Shader.Find( "Hidden/Draw/" + shapeName ) );
		mat.hideFlags = HideFlags.DontSave;
		mat.enableInstancing = true;
		return mat;
	}


	static Mesh CreateQuadMesh()
	{
		Mesh mesh = new Mesh();
		mesh.hideFlags = HideFlags.HideAndDontSave;

		// Verticies.
		VertexAttributeDescriptor[] vertexDataLayout = new VertexAttributeDescriptor[] {
			new VertexAttributeDescriptor( VertexAttribute.Position, VertexAttributeFormat.Float32, 2 )
		};
		mesh.SetVertexBufferParams( 4, vertexDataLayout );
		mesh.SetVertexBufferData(
			new Vector2[]{
					new Vector2( -1, -1 ),
					new Vector2( -1,  1 ),
					new Vector2(  1,  1 ),
					new Vector2(  1, -1 ),
			},
			0, 0, 4, 0,
			meshFlags
		);

		// Indices.
		mesh.SetIndexBufferParams( 4, IndexFormat.UInt16 );
		mesh.SetIndexBufferData( new ushort[] { 0, 1, 2, 3 }, 0, 0, 4, meshFlags );

		// Sub mesh.
		SubMeshDescriptor meshDescriptor = new SubMeshDescriptor( 0, 4, MeshTopology.Quads );
		meshDescriptor.bounds = new Bounds( Vector3.zero, Vector3.one * 2 );
		meshDescriptor.vertexCount = 4;
		mesh.subMeshCount = 1;
		mesh.SetSubMesh( 0, meshDescriptor, meshFlags );

		// Bounds.
		mesh.bounds = meshDescriptor.bounds;

		mesh.UploadMeshData( true );

		return mesh;
	}


	static void GetInnerOuterRadius( float radius, out float innerRadius, out float outerRadius )
	{
		innerRadius = radius;
		outerRadius = radius;
		if( _self._strokeEnabled ) {
			switch( _self._strokeAlignment ) {
				case StrokeAlignment.Inside:
					innerRadius -= _self._strokeThickness;
					break;
				case StrokeAlignment.Edge:
					innerRadius -= _self._strokeThickness * 0.5f;
					outerRadius += _self._strokeThickness * 0.5f;
					break;
				case StrokeAlignment.Outside:
					outerRadius += _self._strokeThickness;
					break;
			}
		}
		if( innerRadius < 0 ) {
			outerRadius -= innerRadius;
			innerRadius = 0;
			if( outerRadius > radius ) outerRadius = radius;
		}
	}


	static float GetStokeOffsetMin()
	{
		switch( _self._strokeAlignment ) {
			case StrokeAlignment.Inside: return -_self._strokeThickness;
			case StrokeAlignment.Edge: return -_self._halfStrokeThickness;
		}
		return 0;
	}


	static Vector2 GetPivotPosition( Pivot pivot )
	{
		switch( pivot )
		{
			case Pivot.Center:		return new Vector2(  0,  0  );
			case Pivot.TopLeft:		return new Vector2( -1,  1 );
			case Pivot.Top:			return new Vector2(  0,  1 );
			case Pivot.TopRight:	return new Vector2(  1,  1 );
			case Pivot.Right:		return new Vector2(  1,  0 );
			case Pivot.BottomRight: return new Vector2(  1, -1 );
			case Pivot.Bottom:		return new Vector2(  0, -1 );
			case Pivot.BottomLeft:	return new Vector2( -1, -1 );
			case Pivot.Left:		return new Vector2( -1,  0 );
		}
		return Vector2.zero;
	}



	public class Polygon
	{
		Mesh _mesh;
		protected double[] _data;
		Vector3[] _vertices;
		List<int> _indices;
		bool _isDirty;

		public int pointCount { get { return _vertices == null ? 0 : _vertices.Length; } }

		public void SetPointCount( int pointCount )
		{
			if( _data == null || _data.Length != pointCount * 2 ) {
				_data = new double[ pointCount * 2 ];
				_vertices = new Vector3[ pointCount ];
			}
			
			_isDirty = true;
		}


		public void SetPoint( int index, Vector2 point )
		{
			int d = index * 2;
			_data[ d ] = point.x;
			_data[ d + 1 ] = point.y;
			_vertices[ index ] = point;
			_isDirty = true;
		}

		public void SetPoint( int index, float x, float y )
		{
			SetPoint( index, new Vector2( x, y ) );
		}


		public void GetRenderObjects( float strokeThickness, StrokeAlignment strokeAlignment, out Mesh mesh )
		{
			// TODO. Handle stroke.

			if( _isDirty ) Build();

			mesh = _mesh;
		}


		void Build()
		{
			if( !_mesh ) {
				_mesh = new Mesh();
				_mesh.hideFlags = HideFlags.HideAndDontSave;
			}
			if( _mesh.vertexCount != pointCount ) _mesh.Clear();

			_indices = EarcutNet.Earcut.Tessellate( _data, new int[] { } );
			_indices.Reverse(); // TODO avoid garbage. Earcut always returns a flipped mesh.

			_mesh.SetVertices( _vertices );
			_mesh.SetIndices( _indices, MeshTopology.Triangles, 0 );

			_isDirty = false;
		}
	}


	public class Polyline
	{
		Mesh _mesh;
		Vector2[] _points;
		Vector2[] _directions;
		Vector3[] _vertices;
		Vector4[] _uvs;
		int[] _indices;
		bool _isDirty;
		float _strokeThickness;

		public int pointCount { get { return _points == null ? 0 : _points.Length; } }


		public void SetPointCount( int pointCount )
		{
			if( _points == null || _points.Length != pointCount ) _points = new Vector2[ pointCount ];
			
			_isDirty = true;
		}


		public void SetPoint( int index, Vector2 point )
		{
			_points[ index ] = point;
			_isDirty = true;
		}

		public void SetPoint( int index, float x, float y )
		{
			SetPoint( index, new Vector2( x, y ) );
		}


		public Vector2 GetPoint( int index )
		{
			if( _points == null || index < 0 || index >= _points.Length ) return Vector3.zero;
			return _points[ index ];
		}


		public void SetBezierCurve( Vector2 anchorA, Vector2 controlA, Vector2 controlB, Vector2 anchorB, int resolution = 32 )
		{
			if( resolution < 3 ) resolution = 3;
			if( _points == null || _points.Length != resolution ) _points = new Vector2[ resolution ];
			_points[ 0 ] = anchorA;
			_points[ resolution-1 ] = anchorB;
			for( int r = 1; r < resolution-1; r++ ) {
				float t = r / (resolution-1f);
				float x = DrawMath.QuadraticInterpolation( anchorA.x, controlA.x, controlB.x, anchorB.x, t );
				float y = DrawMath.QuadraticInterpolation( anchorA.y, controlA.y, controlB.y, anchorB.y, t );
				_points[ r ] = new Vector2( x, y );
			}
			_isDirty = true;
		}


		public void GetRenderObjects( float strokeThickness, out Mesh mesh )
		{
			if( !Mathf.Approximately( _strokeThickness, strokeThickness ) ){
				_strokeThickness = strokeThickness;
				_isDirty = true;
			}

			// TODO. Handle stroke.

			if( _isDirty ) Build();

			mesh = _mesh;
		}
		

		void Build()
		{
			if( !_mesh ) {
				_mesh = new Mesh();
				_mesh.hideFlags = HideFlags.HideAndDontSave;
			}

			if( _points.Length < 2 ) {
				_mesh.Clear();
				return;
			}

			int pointCount = _points.Length;
			int vertexCount = 4 + ( pointCount - 2 ) * 4;
			int quadIndexCount = ( pointCount - 1 ) * 4;

			if( _mesh.vertexCount != vertexCount ) _mesh.Clear();

			if( _vertices == null || _vertices.Length != vertexCount ){
				_vertices = new Vector3[ vertexCount ];
				_uvs = new Vector4[ vertexCount ];
				_indices = new int[ quadIndexCount ];
				_directions = new Vector2[ vertexCount ];
				_isDirty = true;
			}

			Vector2 prev = _points[ 0 ];
			for( int p = 1; p < pointCount; p++ ) {
				Vector2 point = _points[ p ];
				Vector2 dir = point - prev;
				dir.Normalize();
				_directions[ p-1 ] = dir;
				prev = point;
			}
			_directions[ pointCount - 1 ] = _directions[ pointCount - 2 ];

			float halfStrokeThickness = _strokeThickness * 0.5f;
			int i = 0, v = 0;
			Vector2 prevOffset = Vector2.zero;
			Vector2 nextPoint = Vector2.zero;
			for( int p = 0; p < pointCount; p++ )
			{
				int v0 = v;
				Vector2 point = _points[ p ];
				Vector2 dir = _directions[ p ];
				Vector2 offset = new Vector2( -dir.y * halfStrokeThickness, dir.x * halfStrokeThickness ); // Rotate 45 degrees and scale
				if( p < pointCount - 1 ) nextPoint = _points[ p + 1 ];

				if( p == 0 || p == pointCount-1 ) {
					_vertices[ v++ ] = point + offset;
					_vertices[ v++ ] = point - offset;
				} else {
					Vector2 intersection;
					if( DrawMath.TryIntersectLineLine( prev + prevOffset, point + prevOffset, point + offset, nextPoint + offset, out intersection ) ) {
						_vertices[ v++ ] = intersection;
					} else {
						_vertices[ v++ ] = point + offset;
					}
					if( DrawMath.TryIntersectLineLine( prev - prevOffset, point - prevOffset, point - offset, nextPoint - offset, out intersection ) ) {
						_vertices[ v++ ] = intersection;
					} else {
						_vertices[ v++ ] = point - offset;
					}
					_vertices[ v++ ] = _vertices[ v-3 ];
					_vertices[ v++ ] = _vertices[ v-3 ];
					v0 += 2;
				}

				if( p < pointCount - 1 ) {
					Vector4 points = new Vector4( point.x, point.y, nextPoint.x, nextPoint.y ); 
					_uvs[ v0 ] = points;
					_uvs[ v0 + 1 ] = points;
					_uvs[ v0 + 2 ] = points;
					_uvs[ v0 + 3 ] = points;
					_indices[ i++ ] = v0;
					_indices[ i++ ] = v0 + 2;
					_indices[ i++ ] = v0 + 3;
					_indices[ i++ ] = v0 + 1;
				}

				prev = point;
				prevOffset = offset;
			}
			
			_mesh.SetVertices( _vertices );
			_mesh.SetUVs( 0, _uvs );
			_mesh.SetIndices( _indices, MeshTopology.Quads, 0 );
			_mesh.RecalculateBounds();

			_isDirty = false;
		}
	}
}