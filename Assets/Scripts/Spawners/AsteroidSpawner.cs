using UnityEngine;
using System.Collections;

public class AsteroidSpawner : MonoBehaviour {

    public GameObject asteroidClone;

    public float spawnRadius = 250;

    float lastSpawnTime;
    float spawnInterval;

    public float minSpawnInterval = 60.0f;
    public float maxSpawnInterval = 90.0f;


	// Use this for initialization
    void Start()
    {
        lastSpawnTime = Time.time;
        spawnInterval = Random.RandomRange(minSpawnInterval, maxSpawnInterval);
    }
	
	// Update is called once per frame
	void Update () 
    {
	    if((Time.time-lastSpawnTime)>=spawnInterval)
        {
            lastSpawnTime = Time.time;
            spawnInterval = Random.RandomRange(minSpawnInterval, maxSpawnInterval);
            SpawnAsteroid();
        }
	}

    void SpawnAsteroid()
    {
        Vector2 insideUnitCircle = Random.insideUnitCircle;
        insideUnitCircle.Normalize();
        GameObject newAsteroidThreat = Instantiate(asteroidClone, (Vector2)transform.position + insideUnitCircle * spawnRadius, Quaternion.identity) as GameObject;
        newAsteroidThreat.GetComponent<AsteroidThreat>().target = gameObject.transform;
        GameManager.SharedInstance.asteroidQueue.Enqueue(newAsteroidThreat);
		EventManager.PostEvent(EventManager.eAsteroidSpawnedEvent);
    }

}
