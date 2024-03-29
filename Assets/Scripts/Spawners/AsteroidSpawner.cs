﻿using UnityEngine;
using System.Collections;

public class AsteroidSpawner : MonoBehaviour 
{
    public GameObject asteroidClone;
    public Vector3  asteroidTargetPosition;
    public float minSpawnInterval = 60.0f;
    public float maxSpawnInterval = 90.0f;
    public float distanceFromSolarSystemBoundary = 50.0f;

    float lastSpawnTime;
    float spawnInterval;

    public void OnEnable()
    {
        EventManager.StartListening(EventManager.ePortalEnteredEvent, Cleanup);
    }

    public void OnDisable()
    {
        EventManager.StopListening(EventManager.ePortalEnteredEvent, Cleanup);
    }

	// Use this for initialization
    void Start()
    {
        lastSpawnTime = Time.time;
        spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
    }
	
	// Update is called once per frame
	void Update () 
    {
	    if((Time.time-lastSpawnTime)>=spawnInterval)
        {
            lastSpawnTime = Time.time;
            spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            SpawnAsteroid();
        }
	}

    void SpawnAsteroid()
    {
        if(GameManager.SharedInstance.AsteroidThreatList!=null)
        {
            Vector2 insideUnitCircle = Random.insideUnitCircle;
            insideUnitCircle.Normalize();
            GameObject newAsteroidThreat = Instantiate(asteroidClone, (Vector2)transform.position + insideUnitCircle * (GameManager.SharedInstance.CurrentLevel.SolarSystemRadius + distanceFromSolarSystemBoundary), Quaternion.identity) as GameObject;
            newAsteroidThreat.GetComponent<AsteroidThreat>().target = GameManager.SharedInstance.CurrentSelectedPlanet.transform.position;
        }
    }

    void Cleanup()
    {
        foreach(GameObject asteroid in GameManager.SharedInstance.AsteroidThreatList)
        {
            Destroy(asteroid);
        }
    }

}
