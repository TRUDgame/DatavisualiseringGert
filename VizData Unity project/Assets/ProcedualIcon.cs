using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcedualIcon : MonoBehaviour
{

    public Material material = null;
    public float radiusMax = 1;
    public int circleResolution = 64;


    void OnRenderObject()
    {
        material.SetPass(0);

        // Draw a cirlce. 
        GL.Begin(GL.TRIANGLE_STRIP);

        for (int i = 0; i < circleResolution; i++)
        {
            float t = Mathf.InverseLerp(0, circleResolution - 1, i); //normalized value of i
            float angle = t * Mathf.PI * 2;
            float x = Mathf.Cos(angle) * radiusMax;
            float y = Mathf.Sin(angle) * radiusMax;
            GL.Vertex3(x, y, 0);
            GL.Vertex3(0, 0, 0);

        }

        GL.End();
    }
    

}
