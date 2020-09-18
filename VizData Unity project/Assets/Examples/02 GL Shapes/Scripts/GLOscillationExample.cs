using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLOscillationExample : MonoBehaviour
{
    public Material material = null;
    public int resolution = 64;
    [Header("wave")]
    public float waveRevolutions = 6;
    [Header("circle and spiral")]
    public float spiralRadiusMax = 1;
    [Header("wavy circle")]
    public float wavyCircleRadius = 5;
    public float wavyCircleWavyness = 0.5f;
    public int wavyCircleWaveCount = 6;
    [Header("distorted circle")]
    public float distortedCircleRadius = 0;
    public float distortedCircleRandomness = 0;

  void OnRenderObject()
    {
        material.SetPass(0);

        //Draw a wave. 
        GL.Begin(GL.LINE_STRIP);

        for (int i = 0; i < resolution; i++)
        {
            float t = Mathf.InverseLerp(0, resolution - 1, i); //normalized value of i
            float angle = t * Mathf.PI * 2 * waveRevolutions; 
            float x = t* 10;
            float y = Mathf.Sin(angle) * 1f; 
            GL.Vertex3(x, y, 0);

        }

        GL.End();

        // Draw a cirlce. 
        GL.Begin(GL.LINE_STRIP);

        for (int i = 0; i < resolution; i++)
        {
            float t = Mathf.InverseLerp(0, resolution - 1, i); //normalized value of i
            float angle = t * Mathf.PI * 2;
           
            float x = Mathf.Cos(angle) * spiralRadiusMax; 
            float y = Mathf.Sin(angle) * spiralRadiusMax;
            GL.Vertex3(x, y, 0);

        }

        GL.End();

        //Draw a spiral. 

        GL.Begin(GL.LINE_STRIP);

        for (int i = 0; i < resolution; i++)
        {
            float t = Mathf.InverseLerp(0, resolution - 1, i); //normalized value of i
            float angle = t * Mathf.PI * 2;
            float radius = spiralRadiusMax * t;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            GL.Vertex3(x, y, 0);

        }

        GL.End();

        //Draw a wavy circle. 
        GL.Begin(GL.LINE_STRIP);

        for (int i = 0; i < resolution; i++)
        {
            float t = Mathf.InverseLerp(0, resolution - 1, i); //normalized value of i
            float circleangle = t * Mathf.PI * 2;
            float waveAngle = t * Mathf.PI * 2 * wavyCircleWaveCount;
            float waveAmplitude = Mathf.Sin(waveAngle) * wavyCircleWavyness;
            float radius = wavyCircleRadius + waveAmplitude;
            float x = Mathf.Cos(circleangle) * radius;
            float y = Mathf.Sin(circleangle) * radius;
            GL.Vertex3(x, y, 0);

        }

        GL.End();



        // Draw a cirlce. 
        GL.Begin(GL.LINE_STRIP);
        Random.InitState(0);
        for (int i = 0; i < resolution; i++)
        {
            float t = Mathf.InverseLerp(0, resolution - 1, i); //normalized value of i
            float angle = t * Mathf.PI * 2;
            float radius = distortedCircleRadius + Random.value * distortedCircleRandomness; 
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius; 
            GL.Vertex3(x, y, 0);

        }

        GL.End();

    }
}
