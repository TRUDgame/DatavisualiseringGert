using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLTranslationRotationScaleExample : MonoBehaviour
{
    public Material material = null;

    void OnRenderObject()
    {
        material.SetPass(0);

        GLRect();

    }


    void GLRect()
    {

 GL.Begin(GL.QUADS);

        GL.Vertex3(0, 0, 0);
        GL.Vertex3(0, 1, 0);
        GL.Vertex3(1, 1, 0);
        GL.Vertex3(1, 0, 0);

        GL.End();
    }
   


}
