using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ProcedualIcon : MonoBehaviour
{
    public Material material = null;
    public int circleCount = 1;
    public int circleResolution = 64;
    public float radius = 0.5f;
    public Color color = new Color(1, 0.5f, 1);
    

    public int randomSeed = 0;

    public Color horLeftColor, horRightColor, verBotColor, verTopColor;
    [Range(0, 1)]
    public float valence = 0.0f;
    [Range(0, 1)]
    public float activation = 0.0f;

    public float wavyCircleWavyness = 0f; 

    const int wavyCircleWaveCount = 5; 


    void OnRenderObject()
    {

        material.SetPass(0);

        Random.InitState(randomSeed);
        for (int i = 0; i < circleCount; i++)
        {
            GL.PushMatrix();
            float x = Random.Range(1f,10f);
            float y = Random.Range(1f,6f); 
            //X and Y position of circles. X increases with 1, X is a random value between 0 and 1
            GL.MultMatrix(Matrix4x4.Translate(new Vector3(x, y, 0)));
            //calls method GLCircle with radius value of 0.5f
            //GLCircle(0.5f, color);
            WavyCircle(0.5f, color);
            GL.PopMatrix();
        }
    }

    private void Update()
    {
        Color valCol = Color.Lerp(horLeftColor, horRightColor, valence);
        Color actCol = Color.Lerp(verBotColor, verTopColor, activation);

        color = Color.Lerp(valCol,actCol,0.5f);

        wavyCircleWavyness = Mathf.Lerp(0.5f, 0, valence);

    }

    void GLCircle(float radius, Color color)
    {
        GL.Begin(GL.TRIANGLE_STRIP);

            GL.Color(color);

        for (int i = 0; i < circleResolution; i = i + 1)
        {
            float t = Mathf.InverseLerp(0, circleResolution - 1, i); // Normalized value of i.         
                                                                     //arc measure?
            float angle = t * Mathf.PI * 2;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;


            GL.Vertex3(x, y, 0);
            GL.Vertex3(0, 0, 0);
        }
        GL.End();


    }

    void WavyCircle(float radius, Color color)
    {
        GL.Begin(GL.TRIANGLE_STRIP);

        GL.Color(color);

        for (int i = 0; i < circleResolution; i++)
        {
            float t = Mathf.InverseLerp(0, circleResolution - 1, i); //normalized value of i
            float circleangle = t * Mathf.PI * 2;
            float waveAngle = t * Mathf.PI * 2 * wavyCircleWaveCount;
            float waveAmplitude = Mathf.Sin(waveAngle * Random.Range(0f,1f)) * wavyCircleWavyness;
            float circleRadius = radius + waveAmplitude;
            float x = Mathf.Cos(circleangle) * circleRadius;
            float y = Mathf.Sin(circleangle) * circleRadius;



            GL.Vertex3(x, y, 0);
            GL.Vertex3(0, 0, 0);

        }

        GL.End();
    }

    }
