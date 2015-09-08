using UnityEngine;
using System.Collections;

public class SpaceStation : MonoBehaviour 
{
	public GameObject ProjectilePrefab;
    
    public GameObject SightLine;
    public bool isSightLineDestructive;

    private GameObject ProjectileInstance;

	public int MaxNumPassengers;
	private Vector3 vec3_Rotation;

    public float HumanLaunchForce   = 1000.0f;
    public float MissileLaunchForce = 1000.0f;

	void Start () 
	{
		vec3_Rotation = new Vector3(0,0,0);
		this.transform.localRotation = Quaternion.Euler(vec3_Rotation);
	}
	
	void Update () 
	{

	}

    public void ConfigureSightLine(bool visible)
    {
        SightLine.SetActive(visible);
    }

    public void LaunchHumans()
    {
        ProjectilePrefab.GetComponent<Projectile>().currentProjectileType = Projectile.ProjectileType.spaceship;
        ProjectilePrefab.tag = "Ship";

        ProjectileInstance = Instantiate(ProjectilePrefab, this.transform.position + (this.transform.up * 2.0f), Quaternion.identity) as GameObject;
        ProjectileInstance.GetComponent<Rigidbody2D>().AddForce(this.transform.up * HumanLaunchForce);

        if (this.gameObject.GetComponentInParent<Planet>().HumanCount < MaxNumPassengers)
        {
            ProjectileInstance.GetComponent<Projectile>().NumPassengers = this.gameObject.GetComponentInParent<Planet>().HumanCount;
            this.gameObject.GetComponentInParent<Planet>().HumanCount = 0;
        }
        else
        {
            this.gameObject.GetComponentInParent<Planet>().HumanCount -= MaxNumPassengers;
            ProjectileInstance.GetComponent<Projectile>().NumPassengers = MaxNumPassengers;
        }
            
        MusicPlayer.SharedInstance.humanLaunchSound();
    }

    public void launchMissiles()
    {
        ProjectilePrefab.GetComponent<Projectile>().currentProjectileType = Projectile.ProjectileType.missile;

        ProjectileInstance = Instantiate(ProjectilePrefab, this.transform.position + (this.transform.up * 2.0f), Quaternion.identity) as GameObject;
        ProjectileInstance.GetComponent<Projectile>().NumPassengers = 0;
        ProjectileInstance.GetComponent<Rigidbody2D>().AddForce(this.transform.up * MissileLaunchForce);
        MusicPlayer.SharedInstance.missileLaunchSound();
	}
}
