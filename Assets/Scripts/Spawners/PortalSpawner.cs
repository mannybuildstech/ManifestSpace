using UnityEngine;
using System.Collections;

public class PortalSpawner : MonoBehaviour 
{
    public GameObject PortalPrefab;
    public float PortalDistanceFromFurthestPlanet = 30.0f;

    public void OnEnable()
    {
        EventManager.StartListening(EventManager.ePortalShouldOpenEvent,GeneratePortal);
    }

    public void OnDisable()
    {
        EventManager.StopListening(EventManager.ePortalShouldOpenEvent,GeneratePortal);
    }

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void GeneratePortal()
    {
        GameObject furthestPlanet  = gameObject.GetComponent<SolarSystemGenerator>().FurthestPlanetFromOrigin();
        Vector2 distance = furthestPlanet.transform.position * (PortalDistanceFromFurthestPlanet);
    }
}
