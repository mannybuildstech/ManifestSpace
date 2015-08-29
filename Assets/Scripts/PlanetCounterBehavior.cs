using UnityEngine;
using System.Collections;

public class PlanetCounterBehavior : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		var rotation = Quaternion.LookRotation(Vector3.up , Vector3.forward);
		transform.rotation = rotation;
	}
}
