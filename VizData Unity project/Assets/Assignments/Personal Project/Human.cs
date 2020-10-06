using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human 
{
   public bool isInfected;
    public bool isRecovered;
    public bool isInfectious;
    public bool isExposed; 
    public Vector2 position;
    public Vector2 velocity;
    public Collider2D collider;
    public Rigidbody2D rBCircle;
    public int id; 
}
