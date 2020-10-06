/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk

	TODO
		- Polyline
			- Closed option
			- SetSplineCurve
		- Polygon
			- Stroke (using internal Polyline with stroke alignment)
*/

using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using DrawInternals;

public class Draw
{
	static Draw _self;

	Material _circleOrRingMaterial;
	Material _pieMaterial;
	Material _arcMaterial;
	Material _rectMaterial;
	Material _lineMaterial;
	Material _polygonMaterial;
	Material _polylineMaterial;
	Material[] _sdfMaterials;

	Mesh _quadMesh;

	MaterialPropertyBlock _circleOrRingProps = new MaterialPropertyBlock();
	MaterialPropertyBlock _pieProps = new MaterialPropertyBlock();
	MaterialPropertyBlock _arcProps = new MaterialPropertyBlock();
	MaterialPropertyBlock _rectProps = new MaterialPropertyBlock();
	MaterialPropertyBlock _lineProps = new MaterialPropertyBlock();
	MaterialPropertyBlock _polygonProps = new MaterialPropertyBlock();
	MaterialPropertyBlock _polylineProps = new MaterialPropertyBlock();

	Matrix4x4 _activeMatrix = Matrix4x4.identity;
	Stack<Matrix4x4> _matrixStack;

	// Global states.
	Color _fillColor = defaultFillColor;
	Color _strokeColor = defaultStrokeColor;
	float _strokeThickness = defaultStrokeThickness; // Always stored in meters
	float _halfStrokeThickness = defaultStrokeThickness * 0.5f;
	StrokeAlignment _strokeAlignment = defaultStrokeAlignment;
	bool _antialias = defaultAntialias;
	Pivot _pivot = defaultPivot;

	Vector2 _pivotPosition = Vector2.zero;
	bool _fillEnabled = true;
	bool _strokeEnabled = true;

	const MeshUpdateFlags meshFlags = MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices;
	const string logPrepend = "<b>[" + nameof( Draw ) + "]</b> ";

	public const float Pi = Mathf.PI;
	public const float HalfPi = Pi * 0.5f;
	public const float Tau = Pi * 2;
	public const string antialiasKeyword = "_ANTIALIAS";

	static Color defaultFillColor = Color.white;
	static Color defaultStrokeColor = Color.black;
	const float defaultStrokeThickness = 0.1f;
	const StrokeAlignment defaultStrokeAlignment = StrokeAlignment.Inside;
	const bool defaultAntialias = true;
	const Pivot defaultPivot = Pivot.Center;

	public enum StrokeAlignment { Inside, Edge, Outside }
	public enum Pivot { Center, TopLeft, Top, TopRight, Right, BottomRight, Bottom, BottomLeft, Left }
	public enum Cap { Round, Square, None }


	static class ShaderIDs
	{
		public static readonly int fillColor = Shader.PropertyToID( "_FillColor" );
		public static readonly int strokeColor = Shader.PropertyToID( "_StrokeColor" );
		public static readonly int strokeOffsetMin = Shader.PropertyToID( "_StrokeOffsetMin" );
		public static readonly int strokeThickness = Shader.PropertyToID( "_StrokeThickness" );
		public static readonly int halfStrokeThickness = Shader.PropertyToID( "_HalfStrokeThickness" );
		public static readonly int fillExtents = Shader.PropertyToID( "_FillExtents" );
		public static readonly int roundedness = Shader.PropertyToID( "_Roundedness" );
		public static readonly int angleExtents = Shader.PropertyToID( "_AngleExtents" );
		public static readonly int roundedCapFlags = Shader.PropertyToID( "_RoundedCapFlags" );
	}


	#region Setup


	Draw()
	{
		_circleOrRingMaterial = CreateInstancedSDFMaterial( "CircleOrRing" );
		_pieMaterial = CreateInstancedSDFMaterial( "Pie" );
		_arcMaterial = CreateInstancedSDFMaterial( "Arc" );
		_rectMaterial = CreateInstancedSDFMaterial( "Rect" );
		_lineMaterial = CreateInstancedSDFMaterial( "Line" );
		_polygonMaterial = CreateInstancedSDFMaterial( "Polygon" );
		_polylineMaterial = CreateInstancedSDFMaterial( "Polyline" );
		_sdfMaterials = new Material[]{
			_circleOrRingMaterial, 
			_pieMaterial, 
			_arcMaterial, 
			_rectMaterial, 
			_lineMaterial, 
			_polygonMaterial, 
			_polylineMaterial
		};

		_quadMesh = CreateQuadMesh();

		_matrixStack = new Stack<Matrix4x4>( 10 );
	}


	static void EnsureSingletonInstance()
	{
		if( _self == null ){
			_self = new Draw();
			SetAntialiasing( true );
		}
	}


	public static void Reset()
	{
		if( _self == null ) return; // Will reset on create anyway.

		SetFillColor( defaultFillColor );
		SetStrokeColor( defaultStrokeColor );
		SetStrokeThickness( defaultStrokeThickness );
		SetStrokeAlignement( defaultStrokeAlignment );
		SetPivot( defaultPivot );
		SetAntialiasing( defaultAntialias );
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

		_self._strokeColor = color;
		_self._strokeEnabled = _self._strokeColor.a > 0 && _self._strokeThickness > 0;
	}

	public static void SetStrokeColor( Color color, float alphaOverride )
	{
		color.a = alphaOverride;
		SetStrokeColor( color );
	}


	public static void SetNoStroke()
	{
		EnsureSingletonInstance();

		_self._strokeColor = Color.clear;
		_self._strokeEnabled = false;
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


	public static void SetAntialiasing( bool isOn )
	{
		EnsureSingletonInstance();

		foreach( Material mat in _self._sdfMaterials ) {
			if( isOn ) mat.EnableKeyword( antialiasKeyword );
			else mat.DisableKeyword( antialiasKeyword );
		}
		_self._antialias = isOn;
	}


	/// <summary>
	/// Set the point from which Circle will be drawn. Default is Pivot.Center.
	/// </summary>
	public static void SetPivot( Pivot pivot )
	{
		EnsureSingletonInstance();

		_self._pivot = pivot;
		_self._pivotPosition = GetPivotPosition( pivot );
	}


	public static Pivot GetPivot()
	{
		EnsureSingletonInstance();

		return _self._pivot;
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

		_self._activeMatrix *= Matrix4x4.Rotate( Quaternion.AngleAxis( angleZ, Vector3.back ) );
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


	public static void SetCanvasMatrix( Matrix4x4 matrix )
	{
		_self._activeMatrix = matrix;
	}

	#endregion // TransformModifiers


	#region Shapes

	public static void DrawCircle( float x, float y, float diameter, float rotation = 0 )
	{
		EnsureSingletonInstance();

		if( !_self._strokeEnabled && !_self._fillEnabled ) return;

		float fillExtents = diameter * 0.5f;
		float strokeOffsetMin = GetStokeOffsetMin();
		float meshExtents = fillExtents + strokeOffsetMin + _self._strokeThickness;
		const float ringRadius = 0;

		Matrix4x4 matrix = _self._activeMatrix;
		matrix *= Matrix4x4.TRS(
			new Vector3( x, y ),
			Quaternion.AngleAxis( rotation, Vector3.back ),
			new Vector3( meshExtents, meshExtents, meshExtents )
		);
		if( _self._pivot != Pivot.Center ) matrix *= Matrix4x4.Translate( new Vector3( -_self._pivotPosition.x, -_self._pivotPosition.y ) );

		if( _self._fillEnabled ) _self._circleOrRingProps.SetColor( ShaderIDs.fillColor, _self._fillColor );
		_self._circleOrRingProps.SetColor( ShaderIDs.strokeColor, _self._strokeEnabled ? _self._strokeColor : new Color( _self._fillColor.r, _self._fillColor.g, _self._fillColor.b, 0 ) );
		_self._circleOrRingProps.SetFloat( ShaderIDs.strokeThickness, _self._strokeThickness );
		_self._circleOrRingProps.SetFloat( ShaderIDs.strokeOffsetMin, strokeOffsetMin );
		_self._circleOrRingProps.SetVector( ShaderIDs.fillExtents, new Vector2( ringRadius, fillExtents ) );

		Graphics.DrawMesh( _self._quadMesh, matrix, _self._circleOrRingMaterial, 0, null, 0, _self._circleOrRingProps, false, false, false );
	}

	public static void DrawCircle( Vector2 position, float diameter, float rotation = 0 )
	{
		DrawCircle( position.x, position.y, diameter, rotation );
	}



	public static void DrawRing( float x, float y, float innerDiameter, float outerDiameter, float rotation = 0 )
	{
		EnsureSingletonInstance();

		if( !_self._strokeEnabled && !_self._fillEnabled ) return;

		float innerFillExtents = innerDiameter * 0.5f;
		float outerFillExtents = outerDiameter * 0.5f;
		float strokeOffsetMin = GetStokeOffsetMin();
		float meshExtents = outerFillExtents + strokeOffsetMin + _self._strokeThickness;
		float ringExtents = ( outerFillExtents - innerFillExtents ) * 0.5f;
		float ringRadius = innerFillExtents + ringExtents;

		Matrix4x4 matrix = _self._activeMatrix;
		matrix *= Matrix4x4.TRS(
			new Vector3( x, y ),
			Quaternion.AngleAxis( rotation, Vector3.back ),
			new Vector3( meshExtents, meshExtents, meshExtents )
		);
		if( _self._pivot != Pivot.Center ) matrix *= Matrix4x4.Translate( new Vector3( -_self._pivotPosition.x, -_self._pivotPosition.y ) );

		if( _self._fillEnabled ) _self._circleOrRingProps.SetColor( ShaderIDs.fillColor, _self._fillColor );
		_self._circleOrRingProps.SetColor( ShaderIDs.strokeColor, _self._strokeEnabled ? _self._strokeColor : new Color( _self._fillColor.r, _self._fillColor.g, _self._fillColor.b, 0 ) );
		_self._circleOrRingProps.SetFloat( ShaderIDs.strokeThickness, _self._strokeThickness );
		_self._circleOrRingProps.SetFloat( ShaderIDs.strokeOffsetMin, strokeOffsetMin );
		_self._circleOrRingProps.SetVector( ShaderIDs.fillExtents, new Vector2( ringRadius, ringExtents ) );

		Graphics.DrawMesh( _self._quadMesh, matrix, _self._circleOrRingMaterial, 0, null, 0, _self._circleOrRingProps, false, false, false );
	}

	public static void DrawRing( Vector2 position, float innerDiameter, float OuterDiameter, float rotation = 0 )
	{
		DrawRing( position.x, position.y, innerDiameter, OuterDiameter, rotation );
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
		float strokeOffsetMin = GetStokeOffsetMin();
		float meshExtents = fillExtents + strokeOffsetMin + _self._strokeThickness;

		float angleOffset = angleBegin + angleExtents;
		Matrix4x4 matrix = _self._activeMatrix;
		matrix *= Matrix4x4.TRS(
			new Vector3( x, y ),
			Quaternion.AngleAxis( rotation, Vector3.back ),
			new Vector3( meshExtents, meshExtents, meshExtents )
		);
		if( _self._pivot != Pivot.Center ) matrix *= Matrix4x4.Translate( new Vector3( -_self._pivotPosition.x, -_self._pivotPosition.y ) );
		matrix *= Matrix4x4.Rotate( Quaternion.AngleAxis( angleOffset, Vector3.back ) );

		if( _self._fillEnabled ) _self._pieProps.SetColor( ShaderIDs.fillColor, _self._fillColor );
		_self._pieProps.SetColor( ShaderIDs.strokeColor, _self._strokeEnabled ? _self._strokeColor : new Color( _self._fillColor.r, _self._fillColor.g, _self._fillColor.b, 0 ) );
		_self._pieProps.SetFloat( ShaderIDs.fillExtents, fillExtents );
		_self._pieProps.SetFloat( ShaderIDs.angleExtents, angleExtents * Mathf.Deg2Rad );
		_self._pieProps.SetFloat( ShaderIDs.strokeOffsetMin, strokeOffsetMin );
		_self._pieProps.SetFloat( ShaderIDs.strokeThickness, _self._strokeThickness );

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
			DrawRing( x, y, innerDiameter, outerDiameter, rotation );
			return;
		}

		float innerFillExtents = innerDiameter * 0.5f;
		float outerFillExtents = outerDiameter * 0.5f;
		float fillThicknessExtents = ( outerFillExtents - innerFillExtents ) * 0.5f;
		float centerFillExtents = innerFillExtents + fillThicknessExtents;
		float strokeOffsetMin = GetStokeOffsetMin();
		float meshExtents = outerFillExtents + strokeOffsetMin + _self._strokeThickness;

		Vector3 offset = new Vector2( x, y );
		float angleOffset = angleBegin + angleExtents;
		Matrix4x4 matrix = _self._activeMatrix;
		matrix *= Matrix4x4.TRS(
			offset,
			Quaternion.AngleAxis( rotation, Vector3.back ),
			Vector3.one * meshExtents
		);
		if( _self._pivot != Pivot.Center ) matrix *= Matrix4x4.Translate( new Vector3( -_self._pivotPosition.x, -_self._pivotPosition.y ) );
		matrix *= Matrix4x4.Rotate( Quaternion.AngleAxis( 180 + angleOffset, Vector3.back ) );

		if( _self._fillEnabled ) _self._arcProps.SetColor( ShaderIDs.fillColor, _self._fillColor );
		_self._arcProps.SetColor( ShaderIDs.strokeColor, _self._strokeEnabled ? _self._strokeColor : new Color( _self._fillColor.r, _self._fillColor.g, _self._fillColor.b, 0 ) );
		_self._arcProps.SetVector( ShaderIDs.fillExtents, new Vector2( centerFillExtents, fillThicknessExtents ) );
		_self._arcProps.SetFloat( ShaderIDs.angleExtents, ( 180 - angleExtents ) * Mathf.Deg2Rad );
		_self._arcProps.SetFloat( ShaderIDs.strokeOffsetMin, strokeOffsetMin );
		_self._arcProps.SetFloat( ShaderIDs.strokeThickness, _self._strokeThickness );

		Graphics.DrawMesh( _self._quadMesh, matrix, _self._arcMaterial, 0, null, 0, _self._arcProps, false, false, false );
	}

	public static void DrawArc( Vector2 position, float innerDiameter, float outerDiameter, float angleBegin, float angleEnd )
	{
		DrawArc( position.x, position.y, innerDiameter, outerDiameter, angleBegin, angleEnd );
	}



	public static void DrawLine( float ax, float ay, float bx, float by, Cap beginCap = Cap.Round, Cap endCap = Cap.Round, float rotation = 0 )
	{
		EnsureSingletonInstance();

		if( !_self._strokeEnabled ) return;

		Vector2 towardsB = new Vector2( bx-ax, by-ay );
		float length = towardsB.magnitude;
		float xCenter = length * 0.5f;
		float lineThickness = _self._strokeThickness;
		if( beginCap == Cap.Round && endCap == Cap.Round ) {
			length += _self._strokeThickness;
		} else if( beginCap == Cap.Round ) {
			length += _self._strokeThickness * 0.5f;
			xCenter -= _self._strokeThickness * 0.25f;
		} else if( endCap == Cap.Round ) {
			length += _self._strokeThickness * 0.5f;
			xCenter += _self._strokeThickness * 0.25f;
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
		_self._lineProps.SetVector( ShaderIDs.roundedCapFlags, new Vector2( beginCap == Cap.Round ? 1 : 0, endCap == Cap.Round ? 1 : 0 ) );

		Graphics.DrawMesh( _self._quadMesh, matrix, _self._lineMaterial, 0, null, 0, _self._lineProps, false, false, false );
	}

	public static void DrawLine( Vector2 positionA, Vector2 positionB, Cap beginCap, Cap endCap, float rotation = 0 )
	{
		DrawLine( positionA.x, positionA.y, positionB.x, positionB.y, beginCap, endCap, rotation );
	}

	public static void DrawLine( Vector2 positionA, Vector2 positionB, Cap caps, float rotation = 0 )
	{
		DrawLine( positionA.x, positionA.y, positionB.x, positionB.y, caps, caps, rotation );
	}

	public static void DrawLine( Vector2 positionA, Vector2 positionB, float rotation = 0 )
	{
		DrawLine( positionA.x, positionA.y, positionB.x, positionB.y, Cap.Round, Cap.Round, rotation );
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

		Matrix4x4 matrix = _self._activeMatrix; // TODO: Optimize matrix operations
		matrix *= Matrix4x4.TRS(
			new Vector2( x, y ),
			Quaternion.AngleAxis( rotation, Vector3.back ),
			new Vector3( outerExtentsX, outerExtentsY )
		);
		if( _self._pivot != Pivot.Center ) matrix *= Matrix4x4.Translate( new Vector3( -_self._pivotPosition.x, -_self._pivotPosition.y ) );

		if( _self._fillEnabled ) _self._rectProps.SetColor( ShaderIDs.fillColor, _self._fillColor );
		_self._rectProps.SetColor( ShaderIDs.strokeColor, _self._strokeEnabled ? _self._strokeColor : new Color( _self._fillColor.r, _self._fillColor.g, _self._fillColor.b, 0 ) );
		_self._rectProps.SetVector( ShaderIDs.fillExtents, new Vector2( width * 0.5f, height * 0.5f ) );
		_self._rectProps.SetFloat( ShaderIDs.strokeThickness, strokeThickness );
		_self._rectProps.SetFloat( ShaderIDs.strokeOffsetMin, strokeOffsetMin );

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

	public static void DrawRect( float x, float y, float width, float height )
	{
		DrawRect( x, y, width, height, 0, 0, 0, 0, 0 );
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

		Matrix4x4 matrix = _self._activeMatrix;
		matrix *= Matrix4x4.Translate( new Vector2( x, y ) );
		if( rotation != 0 ) matrix *= Matrix4x4.Rotate( Quaternion.AngleAxis( rotation, Vector3.back) );

		_self._polygonProps.SetColor( ShaderIDs.fillColor, _self._fillColor );
		_self._polygonProps.SetColor( ShaderIDs.strokeColor, _self._strokeEnabled ? _self._strokeColor : new Color( _self._fillColor.r, _self._fillColor.g, _self._fillColor.b, 0 ) );

		Mesh mesh;
		polygon.GetRenderObjects( _self._strokeThickness, _self._strokeAlignment, _self._antialias, out mesh );
		Graphics.DrawMesh( mesh, matrix, _self._polygonMaterial, 0, null, 0, _self._polygonProps, false, false, false );
	}


	public static void DrawPolyline( Polyline polyline, float x, float y, Cap beginCap = Cap.Round, Cap endCap = Cap.Round, float rotation = 0 )
	{
		EnsureSingletonInstance();

		if( !_self._strokeEnabled ) return;

		Matrix4x4 matrix = _self._activeMatrix; // TODO: Optimize matrix operations
		matrix *= Matrix4x4.Translate( new Vector2( x, y ) );
		if( rotation != 0 ) matrix *= Matrix4x4.Rotate( Quaternion.AngleAxis( rotation, Vector3.back ) );

		_self._polylineProps.SetColor( ShaderIDs.strokeColor, _self._strokeColor );
		_self._polylineProps.SetFloat( ShaderIDs.halfStrokeThickness, _self._halfStrokeThickness );

		Mesh mesh;
		polyline.GetRenderObjects( _self._strokeThickness, beginCap, endCap, out mesh );
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
		if( !_self._strokeEnabled ) return 0;
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


	#region Polygon

	public class Polygon
	{
		Mesh _mesh;
		Vector2[] _points;
		Vector2[] _strokeAdjustedPoints;
		Vector2[] _directions;
		Vector3[] _vertices;
		Vector4[] _uv0; // Points as uvs.
		int[] _indices;
		List<int> _polygonIndices;
		StrokeAlignment _strokeAlignment;
		float _strokeThickness;
		bool _antialias;
		bool _isDirty;
		static int[] noHoles = new int[0];


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
			if( _points == null || index < 0 || index >= _points.Length ) return Vector2.zero;
			return _points[ index ];
		}


		public void GetRenderObjects( float strokeThickness, StrokeAlignment strokeAlignment, bool antialias, out Mesh mesh )
		{
			if( !Mathf.Approximately( strokeThickness, _strokeThickness ) ) {
				_strokeThickness = strokeThickness;
				_isDirty = true;
			}
			if( strokeAlignment != _strokeAlignment ) {
				_strokeAlignment = strokeAlignment;
				_isDirty = true;
			}
			if( antialias != _antialias ) { // When antialiasing, we need to render stroke even though strokeThickness is zero.
				_antialias = antialias;
				_isDirty = true;
			}

			if( _isDirty ) Build();

			mesh = _mesh;
		}


		void Build()
		{
			if( !_mesh ) {
				_mesh = new Mesh();
				_mesh.hideFlags = HideFlags.HideAndDontSave;
			}

			bool hasStroke = _strokeThickness > 0;

			int vertexCount = _points.Length * ( hasStroke ? 3 : 1 );
			if( _vertices == null || _vertices.Length != vertexCount ) {
				_vertices = new Vector3[ vertexCount ];
			}

			/*
			if( hasStroke ) {
	
			

			if( _strokeAdjustedPoints == null || _strokeAdjustedPoints.Length != _points.Length ) {
					_strokeAdjustedPoints = new Vector2[ _points.Length ];
				}
				if( _uv0 )

				DrawMath.ComputeNormalizedDirections( _points, ref _directions );

				float halfStrokeThickness = _strokeThickness * 0.5f;
				int lastIndex = pointCount - 1;
				int i = 0, v = 0;
				Vector2 prev = Vector2.zero;
				Vector2 prevOffset = Vector2.zero;
				Vector2 nextPoint = Vector2.zero;
				for( int p = 0; p < pointCount; p++ ) {
					int v0 = v;
					Vector2 point = _points[ p ];
					Vector2 dir = _directions[ p ];
					Vector2 offset = new Vector2( -dir.y * halfStrokeThickness, dir.x * halfStrokeThickness ); // Rotate 45 degrees and scale
					if( p < lastIndex ) nextPoint = _points[ p + 1 ];

					if( p == 0 || p == lastIndex ) {
						// Two verts for begin and end.
						_vertices[ v++ ] = point + offset;
						_vertices[ v++ ] = point - offset;
					} else {
						// Four verts for inner points.
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
						_vertices[ v++ ] = _vertices[ v - 3 ];
						_vertices[ v++ ] = _vertices[ v - 3 ];
						v0 += 2;
					}

					if( p < lastIndex ) {
						Vector4 points;
						if( p == 0 ) {
							points = new Vector4( point.x + dir.x * halfStrokeThickness, point.y + dir.y * halfStrokeThickness, nextPoint.x, nextPoint.y );
						} else if( p == lastIndex - 1 ) {
							points = new Vector4( point.x, point.y, nextPoint.x - dir.x * halfStrokeThickness, nextPoint.y - dir.y * halfStrokeThickness );
						} else {
							points = new Vector4( point.x, point.y, nextPoint.x, nextPoint.y );
						}
						for( int j = 0, vn = v0; j < 4; j++, vn++ ) {
							_uv0[ vn ] = points;
						}
						_indices[ i++ ] = v0;
						_indices[ i++ ] = v0 + 2;
						_indices[ i++ ] = v0 + 3;
						_indices[ i++ ] = v0 + 1;
					}

					prev = point;
					prevOffset = offset;
				}
			}
			*/

			for( int p = 0; p < _points.Length; p++ ) {
				_vertices[ p ] = _points[ p ];
			}

			Earcut.Tessellate( _points, _points.Length, noHoles, ref _polygonIndices );
			

			_mesh.SetVertices( _vertices );
			_mesh.SetIndices( _polygonIndices, MeshTopology.Triangles, 0 );

			_isDirty = false;
		}
	}

	#endregion // Polygon


	#region Polyline

	public class Polyline
	{
		Mesh _mesh;
		Vector2[] _points;
		Vector2[] _directions;
		Vector3[] _vertices;
		Vector4[] _uv0;	// Points as uvs.
		Vector2[] _uv1;	// Rounded cap flags as uvs.
		int[] _indices;
		bool _isDirty;
		float _strokeThickness;
		Cap _beginCap = Cap.Round;
		Cap _endCap = Cap.Round;

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
			if( _points == null || index < 0 || index >= _points.Length ) return Vector2.zero;
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


		public void GetRenderObjects( float strokeThickness, Cap beginCap, Cap endCap, out Mesh mesh )
		{
			if( !Mathf.Approximately( _strokeThickness, strokeThickness ) ){
				_strokeThickness = strokeThickness;
				_isDirty = true;
			}
			if( beginCap != _beginCap || _endCap != endCap ) {
				_beginCap = beginCap;
				_endCap = endCap;
				_isDirty = true;
			}

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
				_uv0 = new Vector4[ vertexCount ];
				_uv1 = new Vector2[ vertexCount ];
				_indices = new int[ quadIndexCount ];
				_isDirty = true;
			}

			DrawMath.ComputeNormalizedDirections( _points, ref _directions );

			float halfStrokeThickness = _strokeThickness * 0.5f;
			int lastIndex = pointCount - 1;
			int i = 0, v = 0;
			Vector2 prev = Vector2.zero;
			Vector2 prevOffset = Vector2.zero;
			Vector2 nextPoint = Vector2.zero;
			for( int p = 0; p < pointCount; p++ )
			{
				int v0 = v;
				Vector2 point = _points[ p ];
				Vector2 dir = _directions[ p ];
				Vector2 offset = new Vector2( -dir.y * halfStrokeThickness, dir.x * halfStrokeThickness ); // Rotate 45 degrees and scale
				if( p < lastIndex ) nextPoint = _points[ p + 1 ];
				if( p == 0 && _beginCap != Cap.None ) point -= dir * halfStrokeThickness;
				else if( p == lastIndex-1 && _endCap != Cap.None ) nextPoint += dir * halfStrokeThickness;
				else if( p == lastIndex && _endCap != Cap.None ) point += dir * halfStrokeThickness;

				if( p == 0 || p == lastIndex ) {
					// Two verts for begin and end.
					_vertices[ v++ ] = point + offset;
					_vertices[ v++ ] = point - offset;
				} else {
					// Four verts for inner points.
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

				if( p < lastIndex ) {
					Vector4 points;
					Vector2 roundedCapFlags;
					if( p == 0 ){
						points = new Vector4( point.x + dir.x * halfStrokeThickness, point.y + dir.y * halfStrokeThickness, nextPoint.x, nextPoint.y );
						roundedCapFlags = new Vector2( _beginCap == Cap.Round ? 1 : 0, 1 );
					} else if( p == lastIndex - 1 ){
						points = new Vector4( point.x, point.y, nextPoint.x - dir.x * halfStrokeThickness, nextPoint.y - dir.y * halfStrokeThickness );
						roundedCapFlags = new Vector2( 1, _endCap == Cap.Round ? 1 : 0 );
					} else {
						points = new Vector4( point.x, point.y, nextPoint.x, nextPoint.y );
						roundedCapFlags = Vector2.one;
					}
					for( int j = 0, vn = v0; j < 4; j++, vn++ ) {
						_uv0[ vn ] = points;
						_uv1[ vn ] = roundedCapFlags;
					}
					_indices[ i++ ] = v0;
					_indices[ i++ ] = v0 + 2;
					_indices[ i++ ] = v0 + 3;
					_indices[ i++ ] = v0 + 1;
				}

				prev = point;
				prevOffset = offset;
			}
			
			_mesh.SetVertices( _vertices );
			_mesh.SetUVs( 0, _uv0 );
			_mesh.SetUVs( 1, _uv1 );
			_mesh.SetIndices( _indices, MeshTopology.Quads, 0 );
			_mesh.RecalculateBounds();

			_isDirty = false;
		}

	}

	#endregion // Polyline

}