using UnityEngine;
using System.Collections;

public class PlanetSpawner : MonoBehaviour {
    
    public int minPlanets;
    public int maxPlanets;

    public float MinPlanetScale;
    public float MaxPlanetScale;

    float planetSizeCoefficient = 1.5f;

    public float MinimumPlanetDistance;
    
	public GameObject PlanetPrefab;
	public GameObject gameobject_SpaceStationPrefab;

	public Sprite[] sprite_PlanetTextures;

	private GameObject EarthGameObject;
	private GameObject gameobject_KennedyStation;

	private Vector3 _gameOrigin = new Vector3(0,0,0);
	
    int desiredPlanetCount;
    ArrayList planets;

    int adjustmentAttemptThreshold = 2;
    float maxPlanetScaleAdjustmentRatio = .7f;

    public IEnumerator GenerateSolarSystem()
    {
        EarthGameObject = Instantiate(PlanetPrefab, _gameOrigin, Quaternion.identity) as GameObject;
        EarthGameObject.GetComponent<Planet>().HumanCount = GameManager.SharedInstance.startingHumans;
        GameManager.SharedInstance.HumanCount += GameManager.SharedInstance.startingHumans;
        GameManager.SharedInstance.CurrentSelectedPlanet = EarthGameObject;
        EarthGameObject.GetComponent<Planet>().SetSelectedState(true);

        Debug.Log("PlanetSpawner: Intended count is " + desiredPlanetCount);

        int spawnFailures = 0;
        planets.Add(EarthGameObject);
        
        while (planets.Count != desiredPlanetCount)
        {
            //place on map
            Vector2 position = Random.insideUnitCircle * GameManager.SharedInstance.SolarSystemRadius;
            GameObject _tempPlanet = Instantiate(PlanetPrefab, position, Quaternion.identity) as GameObject;
            _tempPlanet.GetComponent<Planet>().HumanCount = 0;

            //random planet sprite
            int planetTextureIndex = Random.Range(0, sprite_PlanetTextures.Length);
            _tempPlanet.GetComponent<SpriteRenderer>().sprite = sprite_PlanetTextures[planetTextureIndex];

            //random scale
            float planetScale = Random.Range(MinPlanetScale, MaxPlanetScale);
            _tempPlanet.transform.localScale = new Vector3(planetScale, planetScale, 1);

            //prevent planets from being way too close to eachother
            CircleCollider2D collider = _tempPlanet.GetComponent<CircleCollider2D>();
            Collider2D[] planetCollider = Physics2D.OverlapCircleAll(_tempPlanet.transform.position, planetScale + MinimumPlanetDistance);

            if (planetCollider.Length > 1)
            {
                Destroy(_tempPlanet);
                Debug.Log(string.Format("Planet {0} is too close to {1}, removing it", _tempPlanet.transform.position, planetCollider[0].transform.position));
                spawnFailures++;

                if(spawnFailures>=adjustmentAttemptThreshold)
                {
                    spawnFailures = 0;
                    //reduce possible planet size to make them all fit
                    MaxPlanetScale *= maxPlanetScaleAdjustmentRatio; 
                }
            }
            else
            {
                Debug.Log(string.Format("Planet {0} Location {1} Scale {2}  NearbyColliders:{3}", planets.Count, position, planetScale, planetCollider.Length));
                planets.Add(_tempPlanet);
            }
            yield return null;
        }
        Debug.Log("Planet count after spawner operation:" + planets.Count);
    }

	// Use this for initialization
	void Start () 
	{
        desiredPlanetCount = Random.Range(minPlanets, maxPlanets);
        planets = new ArrayList();

        StartCoroutine(GenerateSolarSystem());           
	}
}
