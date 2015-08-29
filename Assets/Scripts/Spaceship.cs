using UnityEngine;
using System.Collections;

public class Spaceship : MonoBehaviour {

	public int int_NumOfPassengers;
	public GameObject gameobject_SpaceStationPrefab;

	private GameObject gameobject_SpaceStation;

	public GameObject audiosource;
	public AudioClip clip;

	public float timer = 7;
	public bool dead = false;


	// Use this for initialization
	void Start () {
	
	}

	// Update is called once per frame
	void Update () 
	{
		Vector3 dir = this.GetComponent<Rigidbody2D>().velocity;
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		this.transform.rotation = Quaternion.AngleAxis(angle-90.0f, Vector3.forward);

		timer -= Time.deltaTime;

		if(timer < 0)
		{
			dead = true;
		}

		if(dead == true)
		{
			MusicPlayer.SharedInstance.humansDiedSound();
			Destroy(this.gameObject);
			GameManager.SharedInstance.HumanCount -= int_NumOfPassengers;
		}
	
	}

	void OnCollisionEnter2D(Collision2D col)
	{

		if(GameManager.SharedInstance.currentLaunchMode == GameManager.LaunchMode.HumanMode)
		{
			if(col.transform.tag == "Earth" || col.transform.tag == "Planet")
			{
				if(col.gameObject.GetComponent<Planet>().planetState == Planet.PlanetState.empty)
				{
					gameobject_SpaceStation = Instantiate(gameobject_SpaceStationPrefab, col.transform.position + ((col.transform.up * col.transform.localScale.x) * .9f), Quaternion.identity) as GameObject;
					gameobject_SpaceStation.transform.SetParent(col.transform);
				}

				col.gameObject.GetComponent<Planet>().planetState = Planet.PlanetState.colonized;
				GameManager.SharedInstance.PlanetCount += 1;
				col.gameObject.GetComponent<Planet>().int_NumberOfHumans += int_NumOfPassengers;
				int_NumOfPassengers = 0;

				MusicPlayer.SharedInstance.humansColonizedSound();

				if(col.gameObject.GetComponent<Planet>().bool_PlanetVisited == false)
				{
					col.gameObject.GetComponent<Planet>().bool_PlanetVisited = true;
					col.gameObject.GetComponent<Planet>().int_NumberOfHumans += 5;
					GameManager.SharedInstance.HumanCount += 5;
				}

				Destroy(this.gameObject);
			}
			else if(col.transform.tag == "Debris" || col.transform.tag == "Asteroid")
			{
				GameManager.SharedInstance.HumanCount -= int_NumOfPassengers;
				MusicPlayer.SharedInstance.humansDiedSound();
				int_NumOfPassengers = 0;
				Destroy (this.gameObject);
			}

		}

		else if (GameManager.SharedInstance.currentLaunchMode == GameManager.LaunchMode.MissileMode)
		{
			if(col.transform.tag == "Asteroid")
			{
				MusicPlayer.SharedInstance.missileBlowSound();
				Destroy(col.gameObject);
				Destroy(this.gameObject);
			}

			else if(col.transform.tag == "Debris")
			{
				Destroy(col.gameObject);
				Destroy(this.gameObject);
			}

			else if( col.transform.tag == "Earth" || col.transform.tag == "Planet")
			{
				Destroy(this.gameObject);
				MusicPlayer.SharedInstance.missileBlowSound();
			}

		}

	}
}
