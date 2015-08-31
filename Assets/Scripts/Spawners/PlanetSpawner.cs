using UnityEngine;
using System.Collections;

public class PlanetSpawner : MonoBehaviour {
    
    //TODO planet count should be based on the goal for current session
    public int minPlanets = 50;
    public int maxPlanets = 100;

    public float MinPlanetScale = 1.0f;
    public float MaxPlanetScale = 10.0f;

    float planetSizeCoefficient = 1.5f;

    public float MinimumPlanetDistance = 3.0f;

	public GameObject PlanetPrefab;
	public GameObject gameobject_SpaceStationPrefab;

	public Sprite[] sprite_PlanetTextures;

	private GameObject EarthGameObject;
	private GameObject _tempPlanet;
	private GameObject gameobject_KennedyStation;

	private Vector3 _gameOrigin = new Vector3(0,0,0);
	
    private int planetCount;

	// Use this for initialization
	void Start () 
	{
        planetCount = Random.Range(minPlanets, maxPlanets);
        
		EarthGameObject = Instantiate(PlanetPrefab, _gameOrigin, Quaternion.identity) as GameObject;
        EarthGameObject.GetComponent<Planet>().HumanCount = GameManager.SharedInstance.startingHumans;
        GameManager.SharedInstance.HumanCount += GameManager.SharedInstance.startingHumans;
        GameManager.SharedInstance.CurrentSelectedPlanet = EarthGameObject;
        EarthGameObject.GetComponent<Planet>().SetSelectedState(true);

		for(int i = 0; i < planetCount; i++)
		{
            //place on map
            Vector3 planetLocation = Random.insideUnitCircle * GameManager.SharedInstance.SolarSystemRadius;
            _tempPlanet = Instantiate(PlanetPrefab,planetLocation, Quaternion.identity) as GameObject;
            _tempPlanet.GetComponent<Planet>().HumanCount = 0;

            //random planet sprite
            int planetTextureIndex = Random.Range(0, sprite_PlanetTextures.Length);
            _tempPlanet.GetComponent<SpriteRenderer>().sprite = sprite_PlanetTextures[planetTextureIndex];
            
            //random scale
            float planetScale = Random.Range(MinPlanetScale, MaxPlanetScale);
            _tempPlanet.transform.localScale = new Vector3(planetScale, planetScale, 1);

            //prevent planets from being way too close to eachother
            CircleCollider2D collider = _tempPlanet.GetComponent<CircleCollider2D>();
            Collider2D[] planetCollider = Physics2D.OverlapCircleAll(_tempPlanet.transform.position, MinimumPlanetDistance+planetScale);
            
            if(planetCollider.Length > 1)
			{
                foreach(Transform  child in _tempPlanet.transform)
                        Destroy(child.gameObject);
				Destroy(_tempPlanet);
			}
		}
	}
}
