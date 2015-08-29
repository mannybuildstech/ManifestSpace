using UnityEngine;
using System.Collections;

public class PlanetSpawner : MonoBehaviour {

	public GameObject gameobject_PlanetPrefab;
	public GameObject gameobject_SpaceStationPrefab;

	public Sprite[] sprite_PlanetTextures;

	private GameObject gameobject_Earth;
	private GameObject gameobject_TempPlanet;
	private GameObject gameobject_KennedyStation;


	private Vector3 vec3_Origin = new Vector3(0,0,0);
	private Vector3 vec3_Station = new Vector3(0,3f,0);
	private int int_TotalPlanets = 100;
	private int int_RandomIndex;

	// Use this for initialization
	void Start () 
	{
		gameobject_Earth = Instantiate(gameobject_PlanetPrefab, vec3_Origin, Quaternion.identity) as GameObject;
		gameobject_Earth.tag = "Earth";
		gameobject_KennedyStation = Instantiate(gameobject_SpaceStationPrefab, vec3_Station, Quaternion.identity) as GameObject;
		gameobject_KennedyStation.transform.SetParent(gameobject_Earth.transform);

		for(int i = 0; i < int_TotalPlanets; i++)
		{
			int_RandomIndex = Random.Range(0, sprite_PlanetTextures.Length);
			gameobject_TempPlanet = Instantiate(gameobject_PlanetPrefab, Random.insideUnitCircle * 250, Quaternion.identity) as GameObject;
			gameobject_TempPlanet.GetComponent<SpriteRenderer>().sprite = sprite_PlanetTextures[int_RandomIndex];
			gameobject_TempPlanet.transform.localScale = new Vector3(int_RandomIndex * 1.5f, int_RandomIndex * 1.5f, 1);
			Collider2D[] planetCollider = Physics2D.OverlapCircleAll(gameobject_TempPlanet.transform.position, 3 * int_RandomIndex);
			if(planetCollider.Length > 1)
			{
				Destroy(gameobject_TempPlanet);
				print("too close");
			}

		}
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
}
