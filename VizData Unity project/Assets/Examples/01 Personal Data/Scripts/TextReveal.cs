using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextReveal : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject textObject = null; 
    void Start()
    {
        textObject.SetActive(false); 
    }

   void OnMouseEnter()
    {
        textObject.SetActive(true);
    }

    void OnMouseExit()
    {
        textObject.SetActive(false);
    }
}
