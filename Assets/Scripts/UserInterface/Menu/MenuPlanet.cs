using UnityEngine;
using System.Collections;

public class MenuPlanet : MonoBehaviour {

    public int rotateDirection;
    public float rotateSpeed; 

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        gameObject.transform.Rotate(0, 0, rotateSpeed*rotateDirection);            
	}
}
