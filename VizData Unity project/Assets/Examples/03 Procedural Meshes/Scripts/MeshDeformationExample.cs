
using UnityEngine;

public class MeshDeformationExample : MonoBehaviour
{
    public Material material;
    public Mesh originalMesh;
    public float waveCount = 2; //Per Unity Unit 
    public float waveAmplitude = 0.1f; 


    Mesh _mesh;
    Vector3[] _originalVertices;
    Vector3[] _deformedVertices;

    float waveAngleOffset; 

    void Awake()
    {
        Vector3[] _originalVertices = originalMesh.vertices;
        int[] triangleindices = originalMesh.triangles;

        _mesh = new Mesh(); 
        _mesh.vertices = _originalVertices;
        _mesh.triangles = triangleindices;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        _deformedVertices = new Vector3[_originalVertices.Length];
    }
   void Update()
    {
        waveAngleOffset += Time.deltaTime; 

        for (int v = 0; v < _originalVertices.Length; v++)
        {
            Vector3 vertexPosition = _originalVertices[v];
            //Manipulate 
            float angle = vertexPosition.x * Mathf.PI * 2 * waveCount + waveAngleOffset; 
            vertexPosition.y += Mathf.Sin(angle) * waveAmplitude;

            _deformedVertices[v] = vertexPosition; 
        }

        _mesh.vertices = _deformedVertices;
        _mesh.RecalculateNormals();

        Graphics.DrawMesh(_mesh, transform.localToWorldMatrix, material, gameObject.layer);
    }
}
