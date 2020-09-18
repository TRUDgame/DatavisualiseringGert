using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLPrimitivesExample : MonoBehaviour
{
    public Material material;

   void OnRenderObject()
    {
        material.SetPass( 0 );

        //Draw a triangle.
        GL.Begin( GL.TRIANGLES );

        GL.Vertex3(0, 0, 0);
        GL.Vertex3(0, 1, 0);
        GL.Vertex3(1, 1, 0);

        GL.End();

        // Draw a quad.

        GL.Begin(GL.QUADS);

        GL.Vertex3(2, 0, 0);
        GL.Vertex3(2, 1, 0);
        GL.Vertex3(3, 1, 0);
        GL.Vertex3(3, 0, 0);

        GL.End();

        // Draw a line strip.

        GL.Begin(GL.LINE_STRIP);

        GL.Vertex3(5-0.5f, -2, 0 );
        GL.Vertex3(5+0.5f, -1, 0);
        GL.Vertex3(5-0.5f, 0 , 0  );
        GL.Vertex3(5+0.5f, 1, 0);
        GL.Vertex3(5-0.5f, 2, 0);


        // Draw seperated lines. 
        GL.End();

        GL.Begin(GL.LINES);

        GL.Vertex3(7 - 0.5f, -2, 0);
        GL.Vertex3(7 + 0.5f, -1, 0);
        GL.Vertex3(7 - 0.5f, 0, 0);
        GL.Vertex3(7 + 0.5f, 1, 0);
        GL.Vertex3(7 - 0.5f, 2, 0);


        GL.End();

        // Draw multiple triangles using jagged positions. 

        GL.Begin(GL.TRIANGLE_STRIP);

        GL.Vertex3(9 + 0.5f, -2, 0);
        GL.Vertex3(9 - 0.5f, -1, 0);
        GL.Vertex3(9 + 0.5f, 0, 0);
        GL.Vertex3(9 - 0.5f, 1, 0);
        GL.Vertex3(9 + 0.5f, 2, 0);
        GL.End();

        GL.wireframe = false; 
    }
}
