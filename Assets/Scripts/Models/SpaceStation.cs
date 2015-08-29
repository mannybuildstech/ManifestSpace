using UnityEngine;
using System.Collections;

public class SpaceStation : MonoBehaviour {

	public GameObject gameobject_SpaceShipPrefab;
	public Sprite[] sprite_SpaceShipAndMissle;

	private GameObject gameobject_SpaceShip;

	private int int_NumOfPassengers = 5;

	private Vector3 vec3_Rotation;

	public float timer = 20;
	public bool reload = false;

	// Use this for initialization
	void Start () 
	{
		vec3_Rotation = new Vector3(0,0,0);
		this.transform.localRotation = Quaternion.Euler(vec3_Rotation);
		reload = false;
		timer= 20;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(reload == true)
		{
			timer -=  Time.deltaTime;

			if(timer < 0)
			{
				reload = false;
				timer = 20;
			}
		}
	}

	public void Launch()
	{
		if(GameManager.SharedInstance.currentLaunchMode == GameManager.LaunchMode.HumanMode)
		{
			gameobject_SpaceShipPrefab.GetComponent<SpriteRenderer>().sprite = sprite_SpaceShipAndMissle[0];
			gameobject_SpaceShipPrefab.tag = "Ship";
			gameobject_SpaceShip = Instantiate(gameobject_SpaceShipPrefab, this.transform.position + (this.transform.up * 1.5f), Quaternion.identity) as GameObject;
			gameobject_SpaceShip.GetComponent<Rigidbody2D>().AddForce(this.transform.up * 1000);
			this.gameObject.GetComponentInParent<Planet>().int_NumberOfHumans -= int_NumOfPassengers;
			gameobject_SpaceShip.GetComponent<Spaceship>().int_NumOfPassengers += int_NumOfPassengers;
			MusicPlayer.SharedInstance.humanLaunchSound();
		}

		else if(GameManager.SharedInstance.currentLaunchMode == GameManager.LaunchMode.MissileMode)
		{
			if(reload == false)
			{
				gameobject_SpaceShipPrefab.GetComponent<SpriteRenderer>().sprite = sprite_SpaceShipAndMissle[1];
				gameobject_SpaceShipPrefab.tag = "Missle";
				gameobject_SpaceShip = Instantiate(gameobject_SpaceShipPrefab, this.transform.position + (this.transform.up * 1.5f), Quaternion.identity) as GameObject;
				gameobject_SpaceShip.GetComponent<Rigidbody2D>().AddForce(this.transform.up * 1000);
				MusicPlayer.SharedInstance.missileLaunchSound();
				reload = true;
			}
		}
	}
}
