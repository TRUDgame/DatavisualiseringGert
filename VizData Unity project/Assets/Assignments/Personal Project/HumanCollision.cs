using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

public class HumanCollision : MonoBehaviour
{
    int _humanCount;
    Human[] humans;

    private void Awake()
    {
        GetComponent<CovidSimulation>();

        //_humanCount = CovidSimulation._humanCount;
        humans = new Human[_humanCount];

        for (int h1 = 0; h1 < humans.Length; h1++)
        {

            Human human1 = humans[h1];
            // Move position.
            var forward = Vector3.forward;

            if (Physics2D.Raycast(human1.position, forward, 10))
            {
                Debug.Log("There is an object in front of the human");

            }
        }
    }
}
