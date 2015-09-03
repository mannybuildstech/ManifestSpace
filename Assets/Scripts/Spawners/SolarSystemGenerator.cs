using UnityEngine;
using System.Collections;

public class SolarSystemGenerator : MonoBehaviour {
    
    public int minPlanets;
    public int maxPlanets;

    public float MinPlanetScale;
    public float MaxPlanetScale;

    float planetSizeCoefficient = 1.5f;

    public float MinimumPlanetDistance;
    
	public GameObject PlanetPrefab;
	public Sprite[] PlanetTextures;

    int desiredPlanetCount;
    ArrayList planets;

    int adjustmentAttemptThreshold = 2;
    float maxPlanetScaleAdjustmentRatio = .7f;

    // Use this for initialization
	void Start () 
	{
	}

    public void configurePlanetObjectForCurrentlevel(GameObject planetObject)
    {
        // forward current level parameters
        Planet planetModel = planetObject.GetComponent<Planet>();
        planetModel.HumanCount = 0;
        planetModel.minRotationSpeed = GameManager.SharedInstance.CurrentLevel.MinRotationSpeed;
        planetModel.maxRotationSpeed = GameManager.SharedInstance.CurrentLevel.MaxRotationSpeed;

        SpaceStation stationModel = planetModel.SpaceStationPrefab.GetComponent<SpaceStation>();
        stationModel.MissileReloadSeconds = GameManager.SharedInstance.CurrentLevel.MissileRechargeDuration;
        stationModel.HumanReloadSeconds = GameManager.SharedInstance.CurrentLevel.HumanLoadDuration;

        Projectile projectileModel = stationModel.ProjectilePrefab.GetComponent<Projectile>();
        projectileModel.OxygenDurationSeconds = GameManager.SharedInstance.CurrentLevel.SpaceshipLifeTime;
        projectileModel.NumPassengers = GameManager.SharedInstance.CurrentLevel.MaxPassengerCount;

        DebriSpawner debriSpawner = planetObject.GetComponent<DebriSpawner>();
        debriSpawner.maxDebriCount = GameManager.SharedInstance.CurrentLevel.MaxDebriCount;
        debriSpawner.minDebriCount = GameManager.SharedInstance.CurrentLevel.MinDebriCount;
        debriSpawner.minOrbitRadius = GameManager.SharedInstance.CurrentLevel.DebriOrbitRadiusMin;
        debriSpawner.minOrbitRadius = GameManager.SharedInstance.CurrentLevel.DebriOrbitRadiusMax; 
    }

    public GameObject FurthestPlanetFromOrigin()
    {

    }

    public IEnumerator GenerateSolarSystem()
    {
        desiredPlanetCount = Random.Range(minPlanets, maxPlanets);
        planets = new ArrayList();

        // Instantiate Home Planet
        GameObject EarthGameObject = Instantiate(PlanetPrefab, GameManager.SharedInstance.CurrentHomePosition, Quaternion.identity) as GameObject;
        EarthGameObject.GetComponent<Planet>().HumanCount = GameManager.SharedInstance.CurrentLevel.StartingHumans;
        GameManager.SharedInstance.CurrentLevel.HumanPopulation += GameManager.SharedInstance.CurrentLevel.StartingHumans;
        GameManager.SharedInstance.CurrentSelectedPlanet = EarthGameObject;
        EarthGameObject.GetComponent<Planet>().SetSelectedState(true);

        Debug.Log("PlanetSpawner: Intended count is " + desiredPlanetCount);

        int spawnFailures = 0;
        planets.Add(EarthGameObject);
        
        while (planets.Count != desiredPlanetCount)
        {
            //place on map
            Vector2 position = Random.insideUnitCircle * GameManager.SharedInstance.CurrentLevel.SolarSystemRadius;
            GameObject _tempPlanet = Instantiate(PlanetPrefab, position, Quaternion.identity) as GameObject;
            configurePlanetObjectForCurrentlevel(_tempPlanet);

            //random planet sprite
            int planetTextureIndex = Random.Range(0, PlanetTextures.Length);
            _tempPlanet.GetComponent<SpriteRenderer>().sprite = PlanetTextures[planetTextureIndex];

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
        EventManager.PostEvent(EventManager.eSolarSystemDidFinishSpawning);
    }
}
