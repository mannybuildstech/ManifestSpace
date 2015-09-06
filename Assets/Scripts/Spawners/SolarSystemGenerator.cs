using UnityEngine;
using System.Collections;

using System.Linq;

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

    ArrayList planetsScheduledForDeletion;

    int adjustmentAttemptThreshold = 2;
    float maxPlanetScaleAdjustmentRatio = .7f;

    public void OnEnable()
    {
        EventManager.StartListening(EventManager.eCameraPannedToNewHomeEvent, RunCleanup);
    }

    public void OnDisable()
    {
        EventManager.StopListening(EventManager.eCameraPannedToNewHomeEvent, RunCleanup);
    }

    public GameObject CurrentHomePlanet()
    {
        GameObject result = null;
        if(planets!=null)
        {
            result = (GameObject)planets[0];
        }
        return result;
    }

    public GameObject FurthestPlanetFromOrigin()
    {
        GameObject furthestPlanet = gameObject;

        foreach (GameObject planet in planets)
        {
            if (planet.gameObject.transform.position.magnitude > furthestPlanet.transform.position.magnitude)
                furthestPlanet = planet;
        }


        return furthestPlanet;
    }

    int cyclePlanetsindex = 0;
    public GameObject GetNextColonizedPlanet()
    {
        GameObject result;
        GameObject[] colonizedList = planets.Cast<GameObject>().
            Where(d=>d.GetComponent<Planet>().CurrentPlanetState==Planet.PlanetStateEnum.colonized).
            ToArray<GameObject>();

        if (cyclePlanetsindex > colonizedList.Length - 1)
            cyclePlanetsindex = 0;

        result = colonizedList[cyclePlanetsindex];
        Debug.Log("Panning camera to planet: " + cyclePlanetsindex);
        cyclePlanetsindex++;
        return result;
    }

    public IEnumerator DestroyOldSolarSystem()
    {
        Debug.Log("Destroying old solar system");
        foreach(GameObject planetObject in planetsScheduledForDeletion)
        {
            planetObject.GetComponent<Planet>().DestroyChildren();
            planetObject.GetComponent<DebriSpawner>().DestroyChildren(true);
            yield return null;
        }
    }

    public IEnumerator GenerateSolarSystem()
    {
        if(planets!=null && planets.Count>0)
        {
            planetsScheduledForDeletion = planets;
        }

        desiredPlanetCount = Random.Range(minPlanets, maxPlanets);
        planets = new ArrayList();

        // Instantiate Home Planet
        GameObject EarthGameObject = Instantiate(PlanetPrefab, GameManager.SharedInstance.CurrentHomePosition, Quaternion.identity) as GameObject;
        GameManager.SharedInstance.CurrentSelectedPlanet = EarthGameObject;
        EarthGameObject.GetComponent<Planet>().SetSelectedState(true);
        planets.Add(EarthGameObject);

        //Starting humans will be awarded in other level when portal lands rocket
        if(GameManager.SharedInstance.CurrentLevel.SystemIndex==0)
        {
            EarthGameObject.GetComponent<Planet>().HumanCount = GameManager.SharedInstance.CurrentLevel.StartingHumans;
            GameManager.SharedInstance.CurrentLevel.HumanPopulation += GameManager.SharedInstance.CurrentLevel.StartingHumans;
        }

        int spawnFailures = 0;
        
        while (planets.Count != desiredPlanetCount)
        {
            //place on map
            Vector2 position = GameManager.SharedInstance.CurrentHomePosition + Random.insideUnitCircle * GameManager.SharedInstance.CurrentLevel.SolarSystemRadius;
            GameObject _tempPlanet = Instantiate(PlanetPrefab, position, Quaternion.identity) as GameObject;
            _configurePlanetObjectForCurrentlevel(_tempPlanet);

            //random planet sprite
            int planetTextureIndex = Random.Range(0, PlanetTextures.Length);
            
            SpriteRenderer renderer = _tempPlanet.GetComponent<SpriteRenderer>();
             renderer.sprite = PlanetTextures[planetTextureIndex];

            //random scale
            float planetScale = Random.Range(MinPlanetScale, MaxPlanetScale);
            _tempPlanet.transform.localScale = new Vector3(planetScale, planetScale, 1);

            //prevent planets from being way too close to eachother
            CircleCollider2D collider = _tempPlanet.GetComponent<CircleCollider2D>();
            Collider2D[] planetCollider = Physics2D.OverlapCircleAll(_tempPlanet.transform.position, planetScale + MinimumPlanetDistance);

            if (planetCollider.Length > 1)
            {
                Destroy(_tempPlanet);
                //Debug.Log(string.Format("Planet {0} is too close to {1}, removing it", _tempPlanet.transform.position, planetCollider[0].transform.position));
                spawnFailures++;

                if (spawnFailures >= adjustmentAttemptThreshold)
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
        Debug.Log("Planet count after Spawner operation:" + planets.Count);
        EventManager.PostEvent(EventManager.eSolarSystemDidFinishSpawning);
    }

    void _configurePlanetObjectForCurrentlevel(GameObject planetObject)
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

    public void RunCleanup()
    {
        StartCoroutine(DestroyOldSolarSystem());
    }
   
}
