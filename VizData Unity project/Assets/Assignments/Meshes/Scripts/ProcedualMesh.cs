using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProcedualMesh : MonoBehaviour
{
    public int xSize, ySize, zSize;


    private Vector3[] vertices;
    Vector3[] _deformedVertices; 
   private Mesh mesh;

    public float waveCount = 2; //Per Unity Unit 
    public float waveAmplitude = 0.1f;
    float waveAngleOffset;
    public Material material; 



    private void Awake()
    {
        Generate();
    }

    private void Generate()
    {

        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Grid";

        vertices = new Vector3[(xSize + 1) * (ySize + 1) * (zSize + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        
        for (int i = 0, y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                for (int z = 0; z <= zSize; z++, i++)
                {
                    vertices[i] = new Vector3(x, y, z);
                    uv[i] = new Vector2((float)x/xSize, (float)z / zSize);
                  
                }
            }
        }

    
       mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds(); 
        _deformedVertices = new Vector3[vertices.Length];


        int[] triangles = new int[zSize * xSize * 6];

        for (int ti = 0, vi = 0, x = 0; x < xSize; x++, vi++)
        {
for (int z = 0; z < zSize; z++, ti +=6, vi++)
        {
                triangles[ti] = vi;
        triangles[ti +3] = triangles[ti + 1] = vi+ 1;
        triangles[ti +4] = vi + zSize + 2;
        triangles[ti +5] = triangles[ti + 2] = vi + zSize + 1;
        mesh.triangles = triangles; 
        
        }
        }

        
   
}
    private void OnDrawGizmos()
    {
        if (vertices == null)
        {
            return;
        }

        Gizmos.color = Color.green;
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
    }

    private void Update()
    {
        waveAngleOffset += Time.deltaTime;

        for (int v = 0; v < vertices.Length; v++)
        {
            Vector3 vertexPosition = vertices[v];
            //Manipulate 
            float angle = vertexPosition.x * Mathf.PI * 2 * waveCount + waveAngleOffset;
            vertexPosition.y += Mathf.Sin(angle) * waveAmplitude;

            _deformedVertices[v] = vertexPosition;
        }

        mesh.vertices = _deformedVertices;
        mesh.RecalculateNormals();

        Graphics.DrawMesh(mesh, transform.localToWorldMatrix, material, gameObject.layer);
    }

}
