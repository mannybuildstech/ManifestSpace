using UnityEngine;
using System.Collections;

public class SpaceStation : MonoBehaviour {

	public GameObject ProjectilePrefab;
	public Sprite[] sprite_SpaceShipAndMissle;

	private GameObject ProjectileInstance;

	public int MaxNumPassengers;

	private Vector3 vec3_Rotation;

	public float MissileReloadSeconds;
    float lastMissileLaunchTime;

    public float HumanReloadSeconds;
    float lastHumanLaunchTime;
    
	// Use this for initialization
	void Start () 
	{
		vec3_Rotation = new Vector3(0,0,0);
		this.transform.localRotation = Quaternion.Euler(vec3_Rotation);
        lastMissileLaunchTime = 0.0f;
        lastHumanLaunchTime = 0.0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
	}

    public void launchHumans()
    {
        float timeSinceLastLaunch = Time.time - lastHumanLaunchTime;
        if(timeSinceLastLaunch>HumanReloadSeconds)
        {
            ProjectilePrefab.GetComponent<SpriteRenderer>().sprite = sprite_SpaceShipAndMissle[0];
            ProjectilePrefab.tag = "Ship";
            ProjectileInstance = Instantiate(ProjectilePrefab, this.transform.position + (this.transform.up * 2.0f), Quaternion.identity) as GameObject;
            ProjectileInstance.GetComponent<Rigidbody2D>().AddForce(this.transform.up * 1000);

            if (this.gameObject.GetComponentInParent<Planet>().HumanCount < MaxNumPassengers)
                MaxNumPassengers = this.gameObject.GetComponentInParent<Planet>().HumanCount;

            this.gameObject.GetComponentInParent<Planet>().HumanCount -= MaxNumPassengers;
            ProjectileInstance.GetComponent<Projectile>().NumPassengers = MaxNumPassengers;
            MusicPlayer.SharedInstance.humanLaunchSound();
            lastHumanLaunchTime = Time.time;
        }
    }

    public void launchMissiles()
    {
        if (Time.time - lastMissileLaunchTime > MissileReloadSeconds)
        {
            ProjectilePrefab.GetComponent<SpriteRenderer>().sprite = sprite_SpaceShipAndMissle[1];
            ProjectilePrefab.tag = "Missle";
            ProjectileInstance = Instantiate(ProjectilePrefab, this.transform.position + (this.transform.up * 2.0f), Quaternion.identity) as GameObject;
            ProjectileInstance.GetComponent<Projectile>().NumPassengers = 0;
            ProjectileInstance.GetComponent<Rigidbody2D>().AddForce(this.transform.up * 1000);
            MusicPlayer.SharedInstance.missileLaunchSound();
            lastMissileLaunchTime = Time.time;
        }
	}
}
