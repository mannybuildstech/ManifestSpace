using UnityEngine;
using System.Collections;

public class DebriSpawner : MonoBehaviour 
{
    public GameObject debriModel;
	private float planetRadius;

    public ArrayList spawnedDebri;

    public int maxDebriCount;
    public int minDebriCount;

    public float minOrbitRadius;
    public float maxOrbitRadius;

	void Start () 
    {
        StartCoroutine(GenerateDebri());
	}

    public IEnumerator GenerateDebri()
    {
        int debriSpawnCount = Random.Range(minDebriCount, maxDebriCount);
        spawnedDebri = new ArrayList(debriSpawnCount);
        planetRadius = this.gameObject.GetComponent<CircleCollider2D>().radius;

        for (int i = 0; i < debriSpawnCount; i++)
        {
            //random angle
            Vector2 randomUnitCirclePoint = Random.insideUnitCircle;
            randomUnitCirclePoint.Normalize();
            
            //distance from surface determined by min & max
            Vector2 distanceFromPlanetOrigin = randomUnitCirclePoint * (planetRadius*gameObject.transform.localScale.y + Random.Range(minOrbitRadius,maxOrbitRadius));

            Vector2 junkPosition = (Vector2)transform.position + distanceFromPlanetOrigin;

            GameObject newDebri = Instantiate(debriModel, junkPosition, Quaternion.identity) as GameObject;
            newDebri.GetComponent<DebriBehavior>().orbitOrigin = this.gameObject.transform.position;
            spawnedDebri.Add(newDebri);

            yield return null;
        }
    }

    public void OnDestroy()
    {
        if(spawnedDebri!=null)
        {
            foreach (GameObject junk in spawnedDebri)
            {
                if (junk != null)
                    Destroy(junk);
            }
        }
    }
}
