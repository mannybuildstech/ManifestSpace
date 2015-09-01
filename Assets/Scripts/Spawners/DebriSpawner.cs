using UnityEngine;
using System.Collections;

public class DebriSpawner : MonoBehaviour {

    public GameObject debriModel;
	private float scale;

    public ArrayList spawnedDebri;

    public int maxDebriCount;
    public int minDebriCount;

    public float minOrbitRadius;
    public float maxOrbitRadius;

    // Use this for initialization
	void Start () {

        int debriSpawnCount = Random.Range(minDebriCount, maxDebriCount);
        spawnedDebri = new ArrayList(debriSpawnCount);
		scale = this.gameObject.GetComponent<CircleCollider2D>().radius;
        
        for(int i=0;i<debriSpawnCount;i++)
        {
            Vector2 insideUnitCircle = Random.insideUnitCircle;
            insideUnitCircle.Normalize();

            Vector2 startPosition = (Vector2)transform.position + insideUnitCircle*Random.Range(scale + minOrbitRadius, scale + maxOrbitRadius);
            
            GameObject newDebri = Instantiate(debriModel,startPosition, Quaternion.identity) as GameObject;
            newDebri.GetComponent<DebriBehavior>().orbitOrigin = this.gameObject.transform.position;
            spawnedDebri.Add(newDebri);
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
